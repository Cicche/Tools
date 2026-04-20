using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tools.Core.Models;

namespace Tools.Core.Services
{
    public class RemoteMachineService
    {
        private readonly CredentialService credentialService = new CredentialService();
        private static readonly string[] KillProcessesList =
        {
            "LAM4g.exe","MessApp.exe","MINACOLP.exe","DDA.exe","DSSClientPrj.exe","Sico.exe",
            "TRDfilterWD.exe","sdclient.exe","SCS.exe","MMI45.exe","ScsInitApp.exe","RSAManagerSimulator.exe"
        };

        public async Task<OperationResult> PingAsync(PC pc, int timeoutMs)
        {
            OperationResult validation = ValidateMachine(pc);
            if (!validation.Success) return validation;

            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(pc.Ip, timeoutMs);
                    if (reply.Status == IPStatus.Success)
                    {
                        return OperationResult.Ok("PING OK", pc);
                    }

                    return OperationResult.Fail($"PING fallito ({reply.Status})", OperationErrorCode.NetworkUnreachable, pc);
                }
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"PING: {ex.Message}", OperationErrorCode.Unexpected, pc);
            }
        }

        public async Task<OperationResult> ExecutePowerCommandAsync(PC pc, string command)
        {
            OperationResult validation = ValidateMachine(pc);
            if (!validation.Success) return validation;

            try
            {
                if (!TryResolveCredentials(pc, out string user, out string password))
                {
                    return OperationResult.Fail("Credenziali non disponibili.", OperationErrorCode.CredentialsMissing, pc);
                }

                string remoteCommand = $"shutdown {command}";
                var send = await RunRemoteProcessAsync(pc.Ip, user, password, remoteCommand);
                if (!send.Success)
                {
                    return OperationResult.Fail($"Invio comando remoto fallito: {send.Message}", send.Code, pc);
                }

                var verify = await VerifyPowerCommandAppliedAsync(pc, command);
                if (!verify.Success)
                {
                    return verify;
                }

                return OperationResult.Ok("Comando remoto confermato.", pc);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, OperationErrorCode.ExternalProcessError, pc);
            }
        }

        public OperationResult KillProcesses(PC pc)
        {
            return KillProcessesInternal(pc);
        }

        public Task<OperationResult> KillProcessesAsync(PC pc)
        {
            return Task.Run(() => KillProcessesInternal(pc));
        }

        private OperationResult KillProcessesInternal(PC pc)
        {
            OperationResult validation = ValidateMachine(pc);
            if (!validation.Success) return validation;

            try
            {
                if (!TryResolveCredentials(pc, out string user, out string password))
                {
                    return OperationResult.Fail("Credenziali non disponibili.", OperationErrorCode.CredentialsMissing, pc);
                }

                ManagementScope scope = CreateManagementScope(pc.Ip, user, password);
                scope.Connect();

                foreach (string proc in KillProcessesList)
                {
                    string safeName = proc.Replace("'", "''");
                    var query = new ObjectQuery($"SELECT * FROM Win32_Process WHERE Name='{safeName}'");
                    using (var searcher = new ManagementObjectSearcher(scope, query))
                    using (ManagementObjectCollection processes = searcher.Get())
                    {
                        foreach (ManagementObject process in processes)
                        {
                            using (process)
                            {
                                uint terminateCode;
                                using (ManagementBaseObject inParams = process.GetMethodParameters("Terminate"))
                                {
                                    // Alcuni target richiedono esplicitamente il parametro Reason.
                                    if (inParams != null && inParams.Properties["Reason"] != null)
                                    {
                                        inParams["Reason"] = 0u;
                                    }

                                    using (ManagementBaseObject outParams = process.InvokeMethod("Terminate", inParams, null))
                                    {
                                        object rawReturn = outParams?["ReturnValue"];
                                        terminateCode = rawReturn == null ? 0u : Convert.ToUInt32(rawReturn);
                                    }
                                }

                                if (terminateCode != 0)
                                {
                                    return OperationResult.Fail(
                                        $"Kill fallito su {proc}. Codice terminate: {terminateCode}",
                                        MapWmiReturnCode(terminateCode),
                                        pc);
                                }
                            }
                        }
                    }
                }

                bool stillRunning = IsAnyTargetProcessRunning(scope);
                if (stillRunning)
                {
                    return OperationResult.Fail("Alcuni processi risultano ancora attivi dopo kill.", OperationErrorCode.ExternalProcessError, pc);
                }

                return OperationResult.Ok("Kill process confermato.", pc);
            }
            catch (ManagementException mex)
            {
                return OperationResult.Fail($"WMI kill error: {mex.Message}", OperationErrorCode.ExternalProcessError, pc);
            }
            catch (UnauthorizedAccessException ex)
            {
                return OperationResult.Fail(ex.Message, OperationErrorCode.Unauthorized, pc);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, OperationErrorCode.ExternalProcessError, pc);
            }
        }

        public OperationResult OpenFolder(PC pc)
        {
            OperationResult validation = ValidateMachine(pc);
            if (!validation.Success) return validation;

            try
            {
                if (!TryResolveCredentials(pc, out string user, out string password))
                {
                    return OperationResult.Fail("Credenziali non disponibili.", OperationErrorCode.CredentialsMissing, pc);
                }

                using (var ping = new Ping())
                {
                    var reply = ping.Send(pc.Ip, 1000);
                    if (reply.Status != IPStatus.Success)
                    {
                        return OperationResult.Fail($"Host non raggiungibile ({reply.Status}).", OperationErrorCode.NetworkUnreachable, pc);
                    }
                }

                string adminShare = BuildAdminShare(pc.Ip);
                int connectCode = ConnectShare(adminShare, user, password);
                if (connectCode != 0)
                {
                    return OperationResult.Fail($"Connessione share fallita (codice {connectCode}).", OperationErrorCode.ExternalProcessError, pc);
                }

                Process.Start(new ProcessStartInfo("explorer", adminShare));
                return OperationResult.Ok("Apertura cartella remota.", pc);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, OperationErrorCode.ExternalProcessError, pc);
            }
        }

        public Task<OperationResult> OpenFolderAsync(PC pc)
        {
            return Task.Run(() => OpenFolder(pc));
        }

        public OperationResult OpenRemoteDesktop(PC pc)
        {
            OperationResult validation = ValidateMachine(pc);
            if (!validation.Success) return validation;

            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(pc.Ip, 1000);
                    if (reply.Status != IPStatus.Success)
                    {
                        return OperationResult.Fail($"Host non raggiungibile ({reply.Status}).", OperationErrorCode.NetworkUnreachable, pc);
                    }
                }

                var mstsc = new ProcessStartInfo("mstsc", $"/v:{pc.Ip} /multimon /prompt")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(mstsc);
                return OperationResult.Ok("Avvio desktop remoto (inserimento credenziali richiesto).", pc);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, OperationErrorCode.ExternalProcessError, pc);
            }
        }

        public Task<OperationResult> OpenRemoteDesktopAsync(PC pc)
        {
            return Task.Run(() => OpenRemoteDesktop(pc));
        }

        public OperationResult CopyFolder(PC pc, string origin, string destination)
        {
            OperationResult validation = ValidateMachine(pc);
            if (!validation.Success) return validation;

            try
            {
                if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
                {
                    return OperationResult.Fail("Origine o destinazione non valide.", OperationErrorCode.InvalidInput, pc);
                }

                if (!TryResolveCredentials(pc, out string user, out string password))
                {
                    return OperationResult.Fail("Credenziali non disponibili.", OperationErrorCode.CredentialsMissing, pc);
                }

                using (var ping = new Ping())
                {
                    var reply = ping.Send(pc.Ip, 1000);
                    if (reply.Status != IPStatus.Success)
                    {
                        return OperationResult.Fail($"Host non raggiungibile ({reply.Status}).", OperationErrorCode.NetworkUnreachable, pc);
                    }
                }

                string targetShare = BuildTargetShare(pc.Ip, destination);
                int connectCode = ConnectShare(targetShare, user, password);
                if (connectCode != 0)
                {
                    return OperationResult.Fail($"Connessione share fallita (codice {connectCode}).", OperationErrorCode.ExternalProcessError, pc);
                }

                string sourcePath = origin.Trim();
                string remotePath = BuildRemoteDestinationPath(pc.Ip, destination);
                CopyPath(sourcePath, remotePath);
                Process.Start("explorer", remotePath);
                return OperationResult.Ok("Copia completata.", pc);
            }
            catch (IOException ioEx)
            {
                return OperationResult.Fail(ioEx.Message, OperationErrorCode.ExternalProcessError, pc);
            }
            catch (UnauthorizedAccessException unAuth)
            {
                return OperationResult.Fail(unAuth.Message, OperationErrorCode.Unauthorized, pc);
            }
            catch (Exception ex)
            {
                return OperationResult.Fail(ex.Message, OperationErrorCode.ExternalProcessError, pc);
            }
        }

        public Task<OperationResult> CopyFolderAsync(PC pc, string origin, string destination)
        {
            return Task.Run(() => CopyFolder(pc, origin, destination));
        }

        private bool TryResolveCredentials(PC pc, out string user, out string password)
        {
            return credentialService.TryGetCredentials(pc, out user, out password);
        }

        private static async Task<OperationResult> VerifyPowerCommandAppliedAsync(PC pc, string command)
        {
            bool shouldGoOffline = command.IndexOf("-s", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   command.IndexOf("-r", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!shouldGoOffline)
            {
                return OperationResult.Ok(machine: pc);
            }

            for (int attempt = 0; attempt < 20; attempt++)
            {
                try
                {
                    using (var ping = new Ping())
                    {
                        PingReply reply = await ping.SendPingAsync(pc.Ip, 1000);
                        if (reply.Status != IPStatus.Success)
                        {
                            return OperationResult.Ok("Comando remoto confermato (host in transizione offline).", pc);
                        }
                    }
                }
                catch
                {
                    return OperationResult.Ok("Comando remoto confermato (host non raggiungibile).", pc);
                }

                await Task.Delay(1000);
            }

            return OperationResult.Fail(
                "Comando inviato ma nessuna transizione offline rilevata entro 20s.",
                OperationErrorCode.Timeout,
                pc);
        }

        private static ManagementScope CreateManagementScope(string ip, string user, string password)
        {
            var options = new ConnectionOptions
            {
                Username = user,
                Password = password,
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.PacketPrivacy,
                EnablePrivileges = true
            };

            return new ManagementScope($@"\\{ip}\root\cimv2", options);
        }

        private static async Task<ProcessAckResult> RunRemoteProcessAsync(string ip, string user, string password, string commandLine)
        {
            return await Task.Run(() =>
            {
                try
                {
                    ManagementScope scope = CreateManagementScope(ip, user, password);
                    scope.Connect();

                    using (var processClass = new ManagementClass(scope, new ManagementPath("Win32_Process"), null))
                    using (ManagementBaseObject inParams = processClass.GetMethodParameters("Create"))
                    {
                        inParams["CommandLine"] = commandLine;
                        using (ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null))
                        {
                        uint ret = outParams == null ? 1u : Convert.ToUInt32(outParams["ReturnValue"]);
                        if (ret != 0)
                        {
                            return ProcessAckResult.Fail($"WMI Create ReturnValue={ret}", MapWmiReturnCode(ret));
                        }

                        return ProcessAckResult.Ok();
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    return ProcessAckResult.Fail(ex.Message, OperationErrorCode.Unauthorized);
                }
                catch (ManagementException mex)
                {
                    return ProcessAckResult.Fail(mex.Message, OperationErrorCode.ExternalProcessError);
                }
                catch (Exception ex)
                {
                    return ProcessAckResult.Fail(ex.Message, OperationErrorCode.ExternalProcessError);
                }
            });
        }

        private static bool IsAnyTargetProcessRunning(ManagementScope scope)
        {
            foreach (string proc in KillProcessesList)
            {
                string safeName = proc.Replace("'", "''");
                var query = new ObjectQuery($"SELECT Name FROM Win32_Process WHERE Name='{safeName}'");
                using (var searcher = new ManagementObjectSearcher(scope, query))
                using (ManagementObjectCollection processes = searcher.Get())
                {
                    if (processes.Count > 0) return true;
                }
            }

            return false;
        }

        private static string BuildAdminShare(string ip)
        {
            return $@"\\{ip}\c$";
        }

        private static string BuildTargetShare(string ip, string destination)
        {
            string normalized = NormalizeDestination(destination);
            if (TryGetAdminShareName(normalized, out string shareName))
            {
                return $@"\\{ip}\{shareName}";
            }

            return BuildAdminShare(ip);
        }

        private static string BuildRemoteDestinationPath(string ip, string destination)
        {
            string normalized = NormalizeDestination(destination);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return BuildAdminShare(ip);
            }

            if (normalized.StartsWith(@"\\", StringComparison.Ordinal))
            {
                return normalized;
            }

            if (TrySplitShareAndTail(normalized, out string shareName, out string tail))
            {
                string root = $@"\\{ip}\{shareName}";
                if (string.IsNullOrWhiteSpace(tail))
                {
                    return root;
                }

                return root + "\\" + tail;
            }

            return BuildAdminShare(ip) + "\\" + normalized.TrimStart('\\');
        }

        private static string NormalizeDestination(string destination)
        {
            return (destination ?? string.Empty).Trim().Replace('/', '\\').TrimStart('\\');
        }

        private static bool TryGetAdminShareName(string destination, out string shareName)
        {
            shareName = string.Empty;
            if (!TrySplitShareAndTail(destination, out string parsedShare, out _))
            {
                return false;
            }

            shareName = parsedShare;
            return true;
        }

        private static bool TrySplitShareAndTail(string destination, out string shareName, out string tail)
        {
            shareName = string.Empty;
            tail = string.Empty;
            if (string.IsNullOrWhiteSpace(destination)) return false;

            string normalized = destination.Trim().Replace('/', '\\').TrimStart('\\');
            int slashIndex = normalized.IndexOf('\\');
            string firstPart = slashIndex < 0 ? normalized : normalized.Substring(0, slashIndex);
            tail = slashIndex < 0 ? string.Empty : normalized.Substring(slashIndex + 1).TrimStart('\\');

            if (firstPart.Length == 2 &&
                char.IsLetter(firstPart[0]) &&
                firstPart[1] == '$')
            {
                shareName = char.ToUpperInvariant(firstPart[0]) + "$";
                return true;
            }

            return false;
        }

        private static void CopyPath(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                EnsureDirectory(Path.GetDirectoryName(destinationPath));
                string targetFile = destinationPath;
                if (Directory.Exists(destinationPath) || destinationPath.EndsWith("\\", StringComparison.Ordinal))
                {
                    EnsureDirectory(destinationPath);
                    targetFile = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
                }

                File.Copy(sourcePath, targetFile, true);
                return;
            }

            if (!Directory.Exists(sourcePath))
            {
                throw new IOException($"Origine non trovata: {sourcePath}");
            }

            CopyDirectoryRecursive(sourcePath, destinationPath);
        }

        private static void CopyDirectoryRecursive(string sourceDir, string destinationDir)
        {
            EnsureDirectory(destinationDir);

            foreach (string dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relative = dir.Substring(sourceDir.Length).TrimStart('\\');
                EnsureDirectory(Path.Combine(destinationDir, relative));
            }

            foreach (string file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(sourceDir.Length).TrimStart('\\');
                string target = Path.Combine(destinationDir, relative);
                EnsureDirectory(Path.GetDirectoryName(target));
                File.Copy(file, target, true);
            }
        }

        private static void EnsureDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            Directory.CreateDirectory(path);
        }

        private static int ConnectShare(string remoteShare, string user, string password)
        {
            var nr = new NetResource
            {
                dwType = 1,
                lpRemoteName = remoteShare
            };

            int result = WNetAddConnection2(ref nr, password, user, 0);
            if (result == 1219)
            {
                WNetCancelConnection2(remoteShare, 0, true);
                result = WNetAddConnection2(ref nr, password, user, 0);
            }

            if (result == 85 || result == 0)
            {
                return 0;
            }

            return result;
        }

        private static OperationErrorCode MapWmiReturnCode(uint returnCode)
        {
            switch (returnCode)
            {
                case 2:
                case 3:
                    return OperationErrorCode.Unauthorized;
                case 8:
                case 9:
                    return OperationErrorCode.NetworkUnreachable;
                default:
                    return OperationErrorCode.ExternalProcessError;
            }
        }

        private static OperationResult ValidateMachine(PC pc)
        {
            if (pc == null)
            {
                return OperationResult.Fail("Macchina non valorizzata.", OperationErrorCode.InvalidMachineConfig);
            }

            if (string.IsNullOrWhiteSpace(pc.Nome) || string.IsNullOrWhiteSpace(pc.Ip))
            {
                return OperationResult.Fail("Configurazione macchina incompleta (Nome/IP).", OperationErrorCode.InvalidMachineConfig, pc);
            }

            if (!IPAddress.TryParse(pc.Ip.Trim(), out _))
            {
                return OperationResult.Fail($"IP non valido: {pc.Ip}", OperationErrorCode.InvalidMachineConfig, pc);
            }

            return OperationResult.Ok(machine: pc);
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NetResource lpNetResource, string lpPassword, string lpUsername, int dwFlags);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NetResource
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }

        private sealed class ProcessAckResult
        {
            public bool Success { get; private set; }
            public string Message { get; private set; }
            public OperationErrorCode Code { get; private set; }

            public static ProcessAckResult Ok()
            {
                return new ProcessAckResult { Success = true, Message = string.Empty, Code = OperationErrorCode.None };
            }

            public static ProcessAckResult Fail(string message, OperationErrorCode code)
            {
                return new ProcessAckResult { Success = false, Message = message ?? "Errore processo remoto.", Code = code };
            }
        }
    }
}

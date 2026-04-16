using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tools.Core.Abstractions;
using Tools.Core.Models;
using Tools.Core.Services;

namespace Tools
{
    internal class Funzioni
    {
        private const int MaxParallelOps = 8;
        private readonly MachineOperationService machineOperationService = new MachineOperationService();
        private readonly IAppLogger logger = new FileLogger();
        private static Action<string> logWriter;

        public void wait(int milliseconds)
        {
            if (milliseconds <= 0) return;
            Task.Delay(milliseconds).Wait();
        }

        public async Task evento(IList<PC> listPc, IList<Button> listaBTN, IList<CheckBox> listaCHK, string type)
        {
            var tasks = new List<Task>();
            using (var gate = new SemaphoreSlim(MaxParallelOps))
            {
            foreach (PC pc in listPc)
            {
                if (!TryResolveUi(pc, listaBTN, listaCHK, out Button btn, out CheckBox chk))
                {
                    continue;
                }

                if (!chk.Checked) continue;

                tasks.Add(RunWithThrottleAsync(gate, () => HandleEventAsync(pc, btn, chk, type)));
            }

            await Task.WhenAll(tasks);
            }
        }

        private async Task HandleEventAsync(PC pc, Button btn, CheckBox chk, string type)
        {
            try
            {
                switch (type)
                {
                    case "ping":
                        await HandlePingAsync(pc, btn, chk);
                        break;
                    case "riavvio":
                        await HandleRebootAsync(pc, btn, chk);
                        break;
                    case "off":
                        await HandleShutdownAsync(pc, btn, chk);
                        break;
                    default:
                        throw new ArgumentException("Tipo non supportato.");
                }
            }
            catch (Exception ex)
            {
                LogException(pc, ex);
            }
        }

        private async Task HandlePingAsync(PC pc, Button btn, CheckBox chk)
        {
            OperationResult result = await machineOperationService.PingAsync(pc);
            if (result.Success)
            {
                btn.BackColor = Color.FromArgb(0, 255, 0);
            }
            else
            {
                btn.BackColor = Color.FromArgb(255, 0, 0);
                chk.Checked = false;
                LogError(pc, result);
            }
        }

        private async Task HandleRebootAsync(PC pc, Button btn, CheckBox chk)
        {
            await ExecuteRemoteCommandAsync(pc, btn, chk, "-r -f -t 0", "RIAVVIO");
        }

        private async Task HandleShutdownAsync(PC pc, Button btn, CheckBox chk)
        {
            await ExecuteRemoteCommandAsync(pc, btn, chk, "-s -f -t 5", "SPENGO");
        }

        private async Task ExecuteRemoteCommandAsync(PC pc, Button btn, CheckBox chk, string command, string buttonText)
        {
            LogInfo(pc, buttonText, "Richiesta operazione.");
            btn.Text = buttonText;

            OperationResult result;
            if (command == "-r -f -t 0")
            {
                result = await machineOperationService.RebootAsync(pc);
            }
            else
            {
                result = await machineOperationService.ShutdownAsync(pc);
            }
            if (result.Success)
            {
                btn.BackColor = Color.FromArgb(0, 255, 0);
            }
            else
            {
                btn.BackColor = Color.FromArgb(255, 0, 0);
                chk.Checked = false;
                LogError(pc, result);
            }
        }

        public async Task processKill(IList<PC> listPc, IList<Button> listaBTN, IList<CheckBox> listaCHK)
        {
            var tasks = new List<Task>();
            using (var gate = new SemaphoreSlim(MaxParallelOps))
            {
            foreach (PC pc in listPc)
            {
                if (!TryResolveUi(pc, listaBTN, listaCHK, out Button btn, out CheckBox chk))
                {
                    continue;
                }
                if (!chk.Checked) continue;

                tasks.Add(RunWithThrottleAsync(gate, () => HandleKillAsync(pc, btn, chk)));
            }

            await Task.WhenAll(tasks);
            }
        }

        private async Task HandleKillAsync(PC pc, Button btn, CheckBox chk)
        {
            try
            {
                LogInfo(pc, "KILL", "Richiesta operazione.");
                OperationResult result = await machineOperationService.KillProcessesAsync(pc);
                if (!result.Success)
                {
                    btn.BackColor = Color.FromArgb(255, 165, 0);
                    chk.Checked = false;
                    LogError(pc, result);
                }
            }
            catch (Exception ex)
            {
                LogException(pc, ex);
            }
        }

        public async Task openFolder(IList<PC> listPc, object sender)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            PC pc = ResolvePcByButton(listPc, btn);
            if (pc == null) return;

            try
            {
                LogInfo(pc, "OPEN_FOLDER", "Richiesta operazione.");
                OperationResult result = await machineOperationService.OpenFolderAsync(pc);
                if (result.Success)
                {
                    btn.BackColor = Color.FromArgb(0, 255, 0);
                }
                else
                {
                    btn.BackColor = Color.FromArgb(255, 0, 0);
                    LogError(pc, result);
                }
            }
            catch (Exception ex)
            {
                LogException(pc, new Exception("OPEN_FOLDER: " + ex.Message, ex));
            }
        }

        public async Task openRmDesk(IList<PC> listPc, object sender)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            PC pc = ResolvePcByButton(listPc, btn);
            if (pc == null) return;

            try
            {
                LogInfo(pc, "RDP", "Richiesta operazione.");
                OperationResult result = await machineOperationService.OpenRemoteDesktopAsync(pc);
                if (result.Success)
                {
                    btn.BackColor = Color.FromArgb(0, 255, 0);
                }
                else
                {
                    btn.BackColor = Color.FromArgb(255, 0, 0);
                    LogError(pc, result);
                }
            }
            catch (Exception ex)
            {
                LogException(pc, new Exception("RDP: " + ex.Message, ex));
            }
        }

        public async Task copyFolder(IList<PC> listPc, IList<Button> listaBTN, IList<CheckBox> listaCHK, string origine, string destin)
        {
            if (string.IsNullOrWhiteSpace(origine) || string.IsNullOrWhiteSpace(destin)) return;

            var tasks = new List<Task>();
            using (var gate = new SemaphoreSlim(MaxParallelOps))
            {
            foreach (PC pc in listPc)
            {
                if (!TryResolveUi(pc, listaBTN, listaCHK, out Button btn, out CheckBox chk))
                {
                    continue;
                }
                if (!chk.Checked) continue;

                tasks.Add(RunWithThrottleAsync(gate, () => HandleCopyAsync(pc, btn, chk, origine, destin)));
            }

            await Task.WhenAll(tasks);
            }
        }

        private async Task HandleCopyAsync(PC pc, Button btn, CheckBox chk, string origine, string destin)
        {
            try
            {
                LogInfo(pc, "COPY", "Richiesta operazione.");
                OperationResult result = await machineOperationService.CopyFolderAsync(pc, origine, destin);
                if (result.Success)
                {
                    btn.BackColor = Color.FromArgb(0, 255, 0);
                }
                else
                {
                    btn.BackColor = Color.FromArgb(255, 0, 0);
                    chk.Checked = false;
                    LogError(pc, result);
                }
            }
            catch (Exception ex)
            {
                LogException(pc, ex);
            }
        }

        public string GetComputerName(string clientIP)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(clientIP);
                string firstBit = hostEntry.HostName.Split('.')[0];
                return firstBit;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void SetLogWriter(Action<string> writer)
        {
            logWriter = writer;
        }

        private static PC ResolvePcByButton(IList<PC> listPc, Button btn)
        {
            foreach (PC pc in listPc)
            {
                if (string.Equals(btn.Name, pc.Nome, StringComparison.OrdinalIgnoreCase))
                {
                    return pc;
                }
            }
            return null;
        }

        private static bool TryResolveUi(PC pc, IList<Button> buttons, IList<CheckBox> checks, out Button btn, out CheckBox chk)
        {
            btn = null;
            chk = null;
            if (pc == null) return false;

            btn = buttons.FirstOrDefault(b => string.Equals(b?.Name, pc.Nome, StringComparison.OrdinalIgnoreCase));
            chk = checks.FirstOrDefault(c => string.Equals(c?.Name, pc.Nome, StringComparison.OrdinalIgnoreCase));
            return btn != null && chk != null;
        }

        private void LogInfo(PC pc, string operation, string detail)
        {
            string message = $"[{DateTime.Now:HH:mm:ss}] {operation} - {pc.Nome} ({pc.Ip}) - {detail}";
            Console.WriteLine(message);
            logWriter?.Invoke(message);
            logger.Info(operation, $"{pc.Nome} ({pc.Ip}) - {detail}");
        }

        private void LogException(PC pc, Exception ex)
        {
            string message = $"[{DateTime.Now:HH:mm:ss}] {pc.Nome} ({pc.Ip}) - {ex.Message}";
            Console.WriteLine(message);
            logWriter?.Invoke(message);
            logger.Error("REMOTE_OP", $"{pc.Nome} ({pc.Ip}) - {ex.Message}");
        }

        private void LogError(PC pc, OperationResult result)
        {
            string machineName = string.IsNullOrWhiteSpace(result?.MachineName) ? pc?.Nome : result.MachineName;
            string machineIp = string.IsNullOrWhiteSpace(result?.MachineIp) ? pc?.Ip : result.MachineIp;
            string code = result == null ? OperationErrorCode.Unexpected.ToString() : result.Code.ToString();
            string msg = result == null ? "Operazione fallita." : result.Message;
            string message = $"[{DateTime.Now:HH:mm:ss}] {machineName} ({machineIp}) - [{code}] {msg}";
            Console.WriteLine(message);
            logWriter?.Invoke(message);
            logger.Error("REMOTE_OP", $"{machineName} ({machineIp}) - [{code}] {msg}");
        }

        private static async Task RunWithThrottleAsync(SemaphoreSlim gate, Func<Task> action)
        {
            await gate.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                gate.Release();
            }
        }
    }
}

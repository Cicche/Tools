using System.Threading.Tasks;
using Tools.Core.Models;

namespace Tools.Core.Services
{
    public class MachineOperationService
    {
        private readonly RemoteMachineService remote;

        public MachineOperationService(RemoteMachineService remote = null)
        {
            this.remote = remote ?? new RemoteMachineService();
        }

        public Task<OperationResult> PingAsync(PC machine)
        {
            return remote.PingAsync(machine, 1000);
        }

        public Task<OperationResult> RebootAsync(PC machine)
        {
            return remote.ExecutePowerCommandAsync(machine, "-r -f -t 0");
        }

        public Task<OperationResult> ShutdownAsync(PC machine)
        {
            return remote.ExecutePowerCommandAsync(machine, "-s -f -t 5");
        }

        public OperationResult KillProcesses(PC machine)
        {
            return remote.KillProcesses(machine);
        }

        public Task<OperationResult> KillProcessesAsync(PC machine)
        {
            return remote.KillProcessesAsync(machine);
        }

        public OperationResult OpenFolder(PC machine)
        {
            return remote.OpenFolder(machine);
        }

        public Task<OperationResult> OpenFolderAsync(PC machine)
        {
            return remote.OpenFolderAsync(machine);
        }

        public OperationResult OpenRemoteDesktop(PC machine)
        {
            return remote.OpenRemoteDesktop(machine);
        }

        public Task<OperationResult> OpenRemoteDesktopAsync(PC machine)
        {
            return remote.OpenRemoteDesktopAsync(machine);
        }

        public OperationResult CopyFolder(PC machine, string origin, string destination)
        {
            return remote.CopyFolder(machine, origin, destination);
        }

        public Task<OperationResult> CopyFolderAsync(PC machine, string origin, string destination)
        {
            return remote.CopyFolderAsync(machine, origin, destination);
        }

        public OperationResult DeleteRemotePath(PC machine, string targetPath)
        {
            return remote.DeleteRemotePath(machine, targetPath);
        }

        public Task<OperationResult> DeleteRemotePathAsync(PC machine, string targetPath)
        {
            return remote.DeleteRemotePathAsync(machine, targetPath);
        }
    }
}

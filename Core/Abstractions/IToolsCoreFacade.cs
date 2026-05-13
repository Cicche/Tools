using System.Collections.Generic;
using System.Threading.Tasks;
using Tools.Core.Models;

namespace Tools.Core.Abstractions
{
    public interface IToolsCoreFacade
    {
        BootstrapResult Bootstrap(string xmlPath, bool strictMode);
        int ReimportLegacyCredentials(string xmlPath, bool sanitizeImportedFile, IEnumerable<PC> activeMachines, ICollection<string> messages);
        void ResetCredentialStore();
        void SaveSanitized(string xmlPath, IEnumerable<PC> machines);
        void SetMachineCredentials(PC machine, string user, string password);
        void SetCategoryCredentials(string category, string user, string password);
        void ApplyCredentials(IEnumerable<PC> machines);
        void RemoveCredentialsForMachine(PC machine);

        Task<OperationResult> PingAsync(PC machine);
        Task<OperationResult> RebootAsync(PC machine);
        Task<OperationResult> ShutdownAsync(PC machine);
        Task<OperationResult> KillProcessesAsync(PC machine);
        Task<OperationResult> OpenFolderAsync(PC machine);
        Task<OperationResult> OpenRemoteDesktopAsync(PC machine);
        Task<OperationResult> CopyFolderAsync(PC machine, string origin, string destination);
        Task<OperationResult> DeleteRemotePathAsync(PC machine, string targetPath);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Tools.Core.Abstractions;
using Tools.Core.Models;
using Tools.Core.Repositories;

namespace Tools.Core.Services
{
    public class ToolsCoreFacade : IToolsCoreFacade
    {
        private readonly IMachineRepository machineRepository;
        private readonly CredentialService credentialService;
        private readonly MachineBootstrapService bootstrapService;
        private readonly MachineOperationService machineOperationService;

        public ToolsCoreFacade(
            IMachineRepository machineRepository = null,
            CredentialService credentialService = null,
            IAppLogger appLogger = null,
            MachineOperationService machineOperationService = null)
        {
            this.machineRepository = machineRepository ?? new XmlMachineRepository();
            this.credentialService = credentialService ?? new CredentialService();
            this.machineOperationService = machineOperationService ?? new MachineOperationService();
            bootstrapService = new MachineBootstrapService(this.machineRepository, this.credentialService, appLogger ?? new FileLogger());
        }

        public BootstrapResult Bootstrap(string xmlPath, bool strictMode)
        {
            return bootstrapService.Bootstrap(xmlPath, strictMode);
        }

        public int ReimportLegacyCredentials(string xmlPath, bool sanitizeImportedFile, IEnumerable<PC> activeMachines, ICollection<string> messages)
        {
            var loadResult = machineRepository.LoadMachines(xmlPath);
            if (loadResult == null) return 0;

            if (messages != null)
            {
                foreach (string message in loadResult.Messages)
                {
                    messages.Add(message);
                }
            }

            if (!loadResult.Success || loadResult.Machines == null) return 0;

            var legacyMachines = loadResult.Machines;
            var categoryCredentials = machineRepository.LoadCategoryCredentials(xmlPath);
            foreach (var kv in categoryCredentials)
            {
                credentialService.SetCategoryCredentials(kv.Key, kv.Value.User, kv.Value.Password);
            }

            int migrated = credentialService.MigrateFromLegacyXml(legacyMachines);
            if (sanitizeImportedFile && (migrated > 0 || categoryCredentials.Count > 0))
            {
                machineRepository.SaveSanitized(xmlPath, legacyMachines);
            }

            if (activeMachines != null)
            {
                credentialService.ApplyCredentials(activeMachines);
            }

            return migrated;
        }

        public void ResetCredentialStore()
        {
            credentialService.ResetStore();
        }

        public void SaveSanitized(string xmlPath, IEnumerable<PC> machines)
        {
            machineRepository.SaveSanitized(xmlPath, machines);
        }

        public void SetMachineCredentials(PC machine, string user, string password)
        {
            credentialService.SetCredentials(machine, user, password);
        }

        public void SetCategoryCredentials(string category, string user, string password)
        {
            credentialService.SetCategoryCredentials(category, user, password);
        }

        public void ApplyCredentials(IEnumerable<PC> machines)
        {
            credentialService.ApplyCredentials(machines);
        }

        public void RemoveCredentialsForMachine(PC machine)
        {
            credentialService.RemoveCredentialsForMachine(machine);
        }

        public Task<OperationResult> PingAsync(PC machine)
        {
            return machineOperationService.PingAsync(machine);
        }

        public Task<OperationResult> RebootAsync(PC machine)
        {
            return machineOperationService.RebootAsync(machine);
        }

        public Task<OperationResult> ShutdownAsync(PC machine)
        {
            return machineOperationService.ShutdownAsync(machine);
        }

        public Task<OperationResult> KillProcessesAsync(PC machine)
        {
            return machineOperationService.KillProcessesAsync(machine);
        }

        public Task<OperationResult> OpenFolderAsync(PC machine)
        {
            return machineOperationService.OpenFolderAsync(machine);
        }

        public Task<OperationResult> OpenRemoteDesktopAsync(PC machine)
        {
            return machineOperationService.OpenRemoteDesktopAsync(machine);
        }

        public Task<OperationResult> CopyFolderAsync(PC machine, string origin, string destination)
        {
            return machineOperationService.CopyFolderAsync(machine, origin, destination);
        }
    }
}

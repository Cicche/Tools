using System;
using System.Linq;
using Tools.Core.Abstractions;
using Tools.Core.Models;

namespace Tools.Core.Services
{
    public class MachineBootstrapService
    {
        private readonly IMachineRepository repository;
        private readonly CredentialService credentialService;
        private readonly IAppLogger logger;
        private readonly StartupValidationService validationService;

        public MachineBootstrapService(IMachineRepository repository, CredentialService credentialService, IAppLogger logger = null)
        {
            this.repository = repository;
            this.credentialService = credentialService;
            this.logger = logger ?? new FileLogger();
            validationService = new StartupValidationService();
        }

        public BootstrapResult Bootstrap(string xmlPath, bool strictMode)
        {
            var result = new BootstrapResult();
            var fileLoad = repository.LoadMachines(xmlPath);
            if (fileLoad == null)
            {
                result.Messages.Add("Errore interno caricamento repository.");
                return result;
            }
            if (fileLoad.Messages.Count > 0)
            {
                result.Messages.AddRange(fileLoad.Messages);
            }
            result.TemplateCreated = fileLoad.TemplateCreated;
            if (!fileLoad.Success || fileLoad.Machines == null)
            {
                return result;
            }

            var list = fileLoad.Machines;

            var validation = validationService.Validate(list);
            if (validation.RejectedCount > 0)
            {
                foreach (string rejected in validation.RejectedItems)
                {
                    logger.Warn("STARTUP_VALIDATION", rejected);
                }

                string details = string.Join("\n- ", validation.RejectedItems);
                result.Messages.Add(
                    $"Validazione avvio: scartate {validation.RejectedCount} macchine dal file XML.\n" +
                    $"- {details}");
            }

            if (!credentialService.EnsureStoreIntegrity(out string integrityMessage))
            {
                result.Messages.Add(integrityMessage + "\nLo store e stato resettato automaticamente.");
                logger.Warn("CREDENTIALS", integrityMessage + " Store resettato automaticamente.");
            }

            var categoryCredentials = repository.LoadCategoryCredentials(xmlPath);
            bool hasLegacyCategoryCredentials = categoryCredentials.Count > 0;
            bool hasLegacyMachineCredentials = Macchine.HasLegacyCredentials(validation.ValidMachines);

            if ((hasLegacyMachineCredentials || hasLegacyCategoryCredentials) && strictMode)
            {
                result.BlockedByStrictMode = true;
                result.Messages.Add(
                    "Modalita STRICT attiva: credenziali in chiaro trovate nel file XML (macchina/categoria).\n" +
                    "Rimuovere User/Password dal file oppure disattivare Credentials.StrictMode.");
                logger.Warn("CREDENTIALS", "Modalita STRICT: credenziali legacy in chiaro trovate.");
                return result;
            }

            foreach (var kv in categoryCredentials)
            {
                credentialService.SetCategoryCredentials(kv.Key, kv.Value.User, kv.Value.Password);
            }

            foreach (PC pc in validation.ValidMachines)
            {
                if (pc == null || string.IsNullOrWhiteSpace(pc.Type)) continue;

                if (categoryCredentials.TryGetValue(pc.Type.Trim().ToUpperInvariant(), out CategoryCredential catCred))
                {
                    if (string.IsNullOrWhiteSpace(pc.User)) pc.User = catCred.User;
                    if (string.IsNullOrWhiteSpace(pc.Password)) pc.Password = catCred.Password;
                }
            }

            bool hasLegacyCredentials = Macchine.HasLegacyCredentials(validation.ValidMachines);
            if (hasLegacyCredentials)
            {
                int migrated = credentialService.MigrateFromLegacyXml(validation.ValidMachines);
                if (migrated > 0 || hasLegacyCategoryCredentials)
                {
                    repository.SaveSanitized(xmlPath, validation.ValidMachines);
                    logger.Info("CREDENTIALS", $"Migrazione completata. Credenziali migrate: {migrated}.");
                }
            }
            else if (hasLegacyCategoryCredentials)
            {
                repository.SaveSanitized(xmlPath, validation.ValidMachines);
                logger.Info("CREDENTIALS", "Credenziali categoria legacy rilevate e XML sanitizzato.");
            }
            else if (!credentialService.StoreExists())
            {
                result.CredentialStoreMissing = true;
                result.Messages.Add(
                    "File credenziali cifrato assente e XML senza credenziali.\n" +
                    "Le operazioni remote non funzioneranno finche non inserisci credenziali categoria/macchina.");
                logger.Warn("CREDENTIALS", "Store credenziali assente e nessuna credenziale legacy nell'XML.");
            }

            credentialService.ApplyCredentials(validation.ValidMachines);

            result.UnresolvedCredentials = validation.ValidMachines.Count(pc =>
                pc != null &&
                (string.IsNullOrWhiteSpace(pc.User) || string.IsNullOrWhiteSpace(pc.Password)));

            if (result.UnresolvedCredentials > 0)
            {
                result.Messages.Add(
                    $"Credenziali non trovate per {result.UnresolvedCredentials} macchina/e.\n" +
                    "Inserisci credenziali categoria/macchina dall'app.");
                logger.Warn("CREDENTIALS", $"Credenziali mancanti per {result.UnresolvedCredentials} macchine.");
            }

            result.Machines.AddRange(validation.ValidMachines);
            result.Loaded = true;
            logger.Info("STARTUP", $"Bootstrap completato. Macchine valide caricate: {validation.ValidMachines.Count}.");
            return result;
        }
    }
}

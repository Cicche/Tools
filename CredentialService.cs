using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace Tools
{
    public class CredentialService
    {
        private static readonly object SyncRoot = new object();
        private readonly string _storePath;

        public CredentialService(string storePath = null)
        {
            _storePath = string.IsNullOrWhiteSpace(storePath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tool_Credentials.bin")
                : storePath;
        }

        public int MigrateFromLegacyXml(IEnumerable<PC> machines)
        {
            if (machines == null) return 0;
            lock (SyncRoot)
            {
                var store = LoadStoreUnlocked();
                bool changed = false;
                int migratedCount = 0;

                foreach (var machine in machines)
                {
                    if (machine == null) continue;
                    if (string.IsNullOrWhiteSpace(machine.User) || string.IsNullOrWhiteSpace(machine.Password)) continue;

                    if (UpsertCredential(store, machine, machine.User, machine.Password))
                    {
                        changed = true;
                        migratedCount++;
                    }
                }

                if (changed)
                {
                    SaveStoreUnlocked(store);
                }

                return migratedCount;
            }
        }

        public void ApplyCredentials(IEnumerable<PC> machines)
        {
            if (machines == null) return;
            CredentialStore store;
            lock (SyncRoot)
            {
                store = LoadStoreUnlocked();
            }

            foreach (var machine in machines)
            {
                if (machine == null) continue;
                if (!string.IsNullOrWhiteSpace(machine.User) && !string.IsNullOrWhiteSpace(machine.Password)) continue;

                if (TryResolve(store, machine, out var user, out var password))
                {
                    machine.User = user;
                    machine.Password = password;
                }
            }
        }

        public bool TryGetCredentials(PC machine, out string user, out string password)
        {
            user = null;
            password = null;

            if (machine == null) return false;

            if (!string.IsNullOrWhiteSpace(machine.User) && !string.IsNullOrWhiteSpace(machine.Password))
            {
                user = machine.User;
                password = machine.Password;
                return true;
            }

            lock (SyncRoot)
            {
                var store = LoadStoreUnlocked();
                return TryResolve(store, machine, out user, out password);
            }
        }

        public void SetCredentials(PC machine, string user, string password)
        {
            if (machine == null) return;
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password)) return;
            lock (SyncRoot)
            {
                var store = LoadStoreUnlocked();
                if (UpsertCredential(store, machine, user, password))
                {
                    SaveStoreUnlocked(store);
                }
            }
        }

        public void SetCategoryCredentials(string category, string user, string password)
        {
            if (string.IsNullOrWhiteSpace(category)) return;
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password)) return;
            lock (SyncRoot)
            {
                var store = LoadStoreUnlocked();
                string key = BuildCategoryKey(category);
                var existing = store.Credentials.FirstOrDefault(c =>
                    string.Equals(c.MachineKey, key, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    store.Credentials.Add(new CredentialRecord
                    {
                        MachineKey = key,
                        User = user,
                        Password = password
                    });
                }
                else
                {
                    existing.User = user;
                    existing.Password = password;
                }

                SaveStoreUnlocked(store);
            }
        }

        public void RemoveCredentialsForMachine(PC machine)
        {
            if (machine == null) return;
            lock (SyncRoot)
            {
                var store = LoadStoreUnlocked();

                var keys = BuildMachineKeys(machine).ToList();
                if (keys.Count == 0) return;

                int removed = store.Credentials.RemoveAll(c =>
                    keys.Any(k => string.Equals(c.MachineKey, k, StringComparison.OrdinalIgnoreCase)));

                if (removed > 0)
                {
                    SaveStoreUnlocked(store);
                }
            }
        }

        public void ResetStore()
        {
            lock (SyncRoot)
            {
                try
                {
                    if (File.Exists(_storePath))
                    {
                        File.Delete(_storePath);
                    }
                }
                catch
                {
                    // Il reset non deve crashare la UI.
                }
            }
        }

        public bool StoreExists()
        {
            lock (SyncRoot)
            {
                return File.Exists(_storePath);
            }
        }

        public bool EnsureStoreIntegrity(out string details)
        {
            details = string.Empty;
            lock (SyncRoot)
            {
                if (!File.Exists(_storePath))
                {
                    return true;
                }

                if (TryLoadStoreUnlocked(out _, out Exception ex))
                {
                    return true;
                }

                string backupPath = _storePath + ".corrupt." + DateTime.Now.ToString("yyyyMMddHHmmss");
                try
                {
                    File.Move(_storePath, backupPath);
                    details = $"Store credenziali corrotto: backup creato in {Path.GetFileName(backupPath)}. Errore: {ex.Message}";
                }
                catch (Exception moveEx)
                {
                    details = $"Store credenziali corrotto e backup non riuscito: {moveEx.Message}. Errore originale: {ex.Message}";
                }

                return false;
            }
        }

        private bool TryResolve(CredentialStore store, PC machine, out string user, out string password)
        {
            user = null;
            password = null;

            if (store?.Credentials == null) return false;

            var keys = BuildMachineKeys(machine);
            foreach (var key in keys)
            {
                var match = store.Credentials.FirstOrDefault(c =>
                    string.Equals(c.MachineKey, key, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    user = match.User;
                    password = match.Password;
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(machine?.Type))
            {
                string categoryKey = BuildCategoryKey(machine.Type);
                var categoryMatch = store.Credentials.FirstOrDefault(c =>
                    string.Equals(c.MachineKey, categoryKey, StringComparison.OrdinalIgnoreCase));
                if (categoryMatch != null)
                {
                    user = categoryMatch.User;
                    password = categoryMatch.Password;
                    return true;
                }
            }

            return false;
        }

        private bool UpsertCredential(CredentialStore store, PC machine, string user, string password)
        {
            bool changed = false;
            var keys = BuildMachineKeys(machine);

            foreach (var key in keys)
            {
                var existing = store.Credentials.FirstOrDefault(c =>
                    string.Equals(c.MachineKey, key, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                {
                    store.Credentials.Add(new CredentialRecord
                    {
                        MachineKey = key,
                        User = user,
                        Password = password
                    });
                    changed = true;
                    continue;
                }

                if (!string.Equals(existing.User, user, StringComparison.Ordinal) ||
                    !string.Equals(existing.Password, password, StringComparison.Ordinal))
                {
                    existing.User = user;
                    existing.Password = password;
                    changed = true;
                }
            }

            return changed;
        }

        private static IEnumerable<string> BuildMachineKeys(PC machine)
        {
            var keys = new List<string>();

            if (!string.IsNullOrWhiteSpace(machine?.Ip))
                keys.Add(NormalizeKey(machine.Ip));

            if (!string.IsNullOrWhiteSpace(machine?.Nome))
                keys.Add(NormalizeKey(machine.Nome));

            return keys.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static string NormalizeKey(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string BuildCategoryKey(string category)
        {
            return "category:" + NormalizeKey(category);
        }

        private CredentialStore LoadStoreUnlocked()
        {
            if (TryLoadStoreUnlocked(out CredentialStore store, out _))
            {
                return store;
            }

            return new CredentialStore();
        }

        private void SaveStoreUnlocked(CredentialStore store)
        {
            if (store == null) return;

            string directory = Path.GetDirectoryName(_storePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var serializer = new XmlSerializer(typeof(CredentialStore));
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, store);
                var plainBytes = ms.ToArray();
                var encrypted = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);

                string tempPath = _storePath + ".tmp";
                File.WriteAllBytes(tempPath, encrypted);
                if (File.Exists(_storePath))
                {
                    File.Replace(tempPath, _storePath, null, true);
                }
                else
                {
                    File.Move(tempPath, _storePath);
                }
            }
        }

        private bool TryLoadStoreUnlocked(out CredentialStore store, out Exception error)
        {
            store = new CredentialStore();
            error = null;

            if (!File.Exists(_storePath))
                return true;

            try
            {
                var encrypted = File.ReadAllBytes(_storePath);
                var plainBytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);

                var serializer = new XmlSerializer(typeof(CredentialStore));
                using (var ms = new MemoryStream(plainBytes))
                {
                    store = (CredentialStore)serializer.Deserialize(ms);
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex;
                store = new CredentialStore();
                return false;
            }
        }
    }

    [Serializable]
    public class CredentialStore
    {
        public List<CredentialRecord> Credentials { get; set; } = new List<CredentialRecord>();
    }

    [Serializable]
    public class CredentialRecord
    {
        public string MachineKey { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}

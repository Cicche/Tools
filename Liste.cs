using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using Tools.Core.Abstractions;
using Tools.Core.Repositories;
using Tools.Core.Services;

namespace Tools
{
    internal class Liste
    {
        private readonly CredentialService credentialService = new CredentialService();
        private readonly IMachineRepository machineRepository = new XmlMachineRepository();
        private readonly IAppLogger appLogger = new FileLogger();
        private const string XmlFilePath = "Tool_List.xml";

        private readonly List<PC> listamacchine = new List<PC>();

        private readonly List<PC> listaCmp = new List<PC>();
        private readonly List<PC> listaTrd = new List<PC>();
        private readonly List<PC> listaDok = new List<PC>();
        private readonly List<PC> listaSrv = new List<PC>();
        private readonly List<PC> listaGw = new List<PC>();
        private readonly List<PC> listaMfc = new List<PC>();

        private readonly List<Button> listaButton = new List<Button>();

        private readonly List<Button> listaCmpButton = new List<Button>();
        private readonly List<Button> listaTrdButton = new List<Button>();
        private readonly List<Button> listaDokButton = new List<Button>();
        private readonly List<Button> listaSrvButton = new List<Button>();
        private readonly List<Button> listaGwButton = new List<Button>();
        private readonly List<Button> listaMfcButton = new List<Button>();

        private readonly List<CheckBox> listaCheck = new List<CheckBox>();

        private readonly List<CheckBox> listaCmpCheck = new List<CheckBox>();
        private readonly List<CheckBox> listaTrdCheck = new List<CheckBox>();
        private readonly List<CheckBox> listaDokCheck = new List<CheckBox>();
        private readonly List<CheckBox> listaSrvCheck = new List<CheckBox>();
        private readonly List<CheckBox> listaGwCheck = new List<CheckBox>();
        private readonly List<CheckBox> listaMfcCheck = new List<CheckBox>();

        public List<PC> getlistamacchine => listamacchine;
        public List<Button> getlistaButton => listaButton;
        public List<CheckBox> getlistaCheck => listaCheck;

        public List<Button> getCmpButton => listaCmpButton;
        public List<Button> getTrdButton => listaTrdButton;
        public List<Button> getDokButton => listaDokButton;
        public List<Button> getSrvButton => listaSrvButton;
        public List<Button> getGwButton => listaGwButton;
        public List<Button> getMfcButton => listaMfcButton;

        public List<CheckBox> getCmpCheck => listaCmpCheck;
        public List<CheckBox> getTrdCheck => listaTrdCheck;
        public List<CheckBox> getDokCheck => listaDokCheck;
        public List<CheckBox> getSrvCheck => listaSrvCheck;
        public List<CheckBox> getGwCheck => listaGwCheck;
        public List<CheckBox> getMfcCheck => listaMfcCheck;

        public void loadPC()
        {
            var bootstrapService = new MachineBootstrapService(machineRepository, credentialService, appLogger);
            var bootstrap = bootstrapService.Bootstrap(XmlFilePath, IsStrictCredentialModeEnabled());
            if (!bootstrap.Loaded || bootstrap.BlockedByStrictMode)
            {
                ShowBootstrapSummary(bootstrap.Messages, true);
                return;
            }

            ShowBootstrapSummary(bootstrap.Messages, false);

            foreach (PC pc in bootstrap.Machines)
            {
                switch (pc.Type.ToUpperInvariant())
                {
                    case "CMP":
                        listaCmp.Add(pc);
                        loadBtn(pc, listaCmpButton);
                        loadCheck(pc, listaCmpCheck);
                        break;
                    case "TRD":
                        listaTrd.Add(pc);
                        loadBtn(pc, listaTrdButton);
                        loadCheck(pc, listaTrdCheck);
                        break;
                    case "DOK":
                        listaDok.Add(pc);
                        loadBtn(pc, listaDokButton);
                        loadCheck(pc, listaDokCheck);
                        break;
                    case "SERVER":
                        listaSrv.Add(pc);
                        loadBtn(pc, listaSrvButton);
                        loadCheck(pc, listaSrvCheck);
                        break;
                    case "GW":
                        listaGw.Add(pc);
                        loadBtn(pc, listaGwButton);
                        loadCheck(pc, listaGwCheck);
                        break;
                    case "MFC":
                        listaMfc.Add(pc);
                        loadBtn(pc, listaMfcButton);
                        loadCheck(pc, listaMfcCheck);
                        break;
                    default:
                        MessageBox.Show("Error Type in file xml\n Consulta i tipi consentiti");
                        return;
                }
            }
        }

        private static void ShowBootstrapSummary(List<string> messages, bool warning)
        {
            if (messages == null || messages.Count == 0) return;
            string summary = BuildLimitedSummary(messages, 5);
            MessageBox.Show(
                summary,
                "Avvio - validazione/sicurezza",
                MessageBoxButtons.OK,
                warning ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private static string BuildLimitedSummary(List<string> messages, int maxItems)
        {
            var output = new List<string>();
            int listedErrors = 0;
            int hiddenErrors = 0;

            foreach (string block in messages)
            {
                if (string.IsNullOrWhiteSpace(block)) continue;

                string[] rawLines = block.Replace("\r", string.Empty).Split('\n');
                for (int i = 0; i < rawLines.Length; i++)
                {
                    string line = rawLines[i].TrimEnd();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    bool isBullet = line.TrimStart().StartsWith("-");
                    if (!isBullet)
                    {
                        output.Add(line);
                        continue;
                    }

                    if (listedErrors < maxItems)
                    {
                        output.Add(line);
                        listedErrors++;
                    }
                    else
                    {
                        hiddenErrors++;
                    }
                }
            }

            if (hiddenErrors > 0)
            {
                output.Add($"- ... e altri {hiddenErrors} errori");
            }

            return string.Join("\n", output);
        }

        public bool AddMachine(string type, string name, string ip, string user, string password, out Button button, out CheckBox check, out string error)
        {
            button = null;
            check = null;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(type) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(ip) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(password))
            {
                error = "Dati mancanti.";
                return false;
            }

            string normalizedType = type.Trim().ToUpperInvariant();
            string normalizedName = name.Trim();
            string normalizedIp = ip.Trim();

            if (!IsAllowedType(normalizedType))
            {
                error = "Categoria non supportata.";
                appLogger.Warn("DATA_VALIDATION", $"Categoria non supportata in inserimento: {normalizedType}.");
                return false;
            }

            if (!IPAddress.TryParse(normalizedIp, out _))
            {
                error = "IP non valido.";
                appLogger.Warn("DATA_VALIDATION", $"Inserimento bloccato per IP non valido: Nome={normalizedName}, IP={normalizedIp}.");
                return false;
            }

            bool duplicate = listamacchine.Any(x =>
                string.Equals(x.Nome, normalizedName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Ip, normalizedIp, StringComparison.OrdinalIgnoreCase));

            if (duplicate)
            {
                error = "Esiste gia una macchina con stesso Nome o IP.";
                appLogger.Warn("DATA_VALIDATION", $"Inserimento bloccato per duplicato: Nome={normalizedName}, IP={normalizedIp}.");
                return false;
            }

            var newPc = new PC
            {
                Type = normalizedType,
                Nome = normalizedName,
                Ip = normalizedIp,
                User = string.Empty,
                Password = string.Empty
            };

            try
            {
                var projected = listamacchine.Concat(new[] { newPc }).ToList();
                Macchine.SaveSanitized(XmlFilePath, projected);
                credentialService.SetCredentials(newPc, user.Trim(), password);
                credentialService.ApplyCredentials(new[] { newPc });

                switch (normalizedType)
                {
                    case "CMP":
                        listaCmp.Add(newPc);
                        button = loadBtn(newPc, listaCmpButton);
                        check = loadCheck(newPc, listaCmpCheck);
                        break;
                    case "TRD":
                        listaTrd.Add(newPc);
                        button = loadBtn(newPc, listaTrdButton);
                        check = loadCheck(newPc, listaTrdCheck);
                        break;
                    case "DOK":
                        listaDok.Add(newPc);
                        button = loadBtn(newPc, listaDokButton);
                        check = loadCheck(newPc, listaDokCheck);
                        break;
                    case "SERVER":
                        listaSrv.Add(newPc);
                        button = loadBtn(newPc, listaSrvButton);
                        check = loadCheck(newPc, listaSrvCheck);
                        break;
                    case "GW":
                        listaGw.Add(newPc);
                        button = loadBtn(newPc, listaGwButton);
                        check = loadCheck(newPc, listaGwCheck);
                        break;
                    case "MFC":
                        listaMfc.Add(newPc);
                        button = loadBtn(newPc, listaMfcButton);
                        check = loadCheck(newPc, listaMfcCheck);
                        break;
                    default:
                        error = "Categoria non supportata.";
                        appLogger.Warn("DATA_VALIDATION", $"Categoria non supportata in inserimento: {normalizedType}.");
                        return false;
                }

                listamacchine.Add(newPc);
                listaButton.Add(button);
                listaCheck.Add(check);
                appLogger.Info("MACHINE", $"Aggiunta macchina {normalizedName} ({normalizedIp}) categoria {normalizedType}.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"Errore salvataggio: {ex.Message}";
                appLogger.Error("MACHINE", $"Errore inserimento {normalizedName} ({normalizedIp}): {ex.Message}");
                return false;
            }
        }

        public void ResetCredentialStore()
        {
            credentialService.ResetStore();
            appLogger.Warn("CREDENTIALS", "Richiesto reset completo store credenziali.");
            foreach (PC pc in listamacchine)
            {
                pc.User = string.Empty;
                pc.Password = string.Empty;
            }
        }

        public int ReimportCredentialsFromLegacyXml(string xmlPath, bool sanitizeImportedFile)
        {
            var loadResult = Macchine.DeserializeWithResult(xmlPath);
            if (loadResult == null) return 0;

            if (loadResult.Messages.Count > 0)
            {
                foreach (string message in loadResult.Messages)
                {
                    appLogger.Warn("XML_IMPORT", message);
                }
            }

            var legacy = loadResult.Machines;
            if (!loadResult.Success || legacy == null) return 0;

            var categoryCredentials = Macchine.LoadCategoryCredentials(xmlPath);
            foreach (var kv in categoryCredentials)
            {
                credentialService.SetCategoryCredentials(kv.Key, kv.Value.User, kv.Value.Password);
            }

            int migrated = credentialService.MigrateFromLegacyXml(legacy);
            if (sanitizeImportedFile && (migrated > 0 || categoryCredentials.Count > 0))
            {
                Macchine.SaveSanitized(xmlPath, legacy);
            }

            credentialService.ApplyCredentials(listamacchine);
            appLogger.Info("CREDENTIALS", $"Reimport completato da {xmlPath}. Migrate={migrated}, credenzialiCategoria={categoryCredentials.Count}.");
            return migrated;
        }

        public void SetCategoryCredentials(string category, string user, string password)
        {
            if (string.IsNullOrWhiteSpace(category)) return;
            credentialService.SetCategoryCredentials(category, user, password);
            credentialService.ApplyCredentials(listamacchine);
            appLogger.Info("CREDENTIALS", $"Credenziali categoria aggiornate: {category.Trim().ToUpperInvariant()}.");
        }

        public bool RemoveMachine(string machineName, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(machineName))
            {
                error = "Nome macchina non valido.";
                return false;
            }

            PC pc = listamacchine.FirstOrDefault(x =>
                string.Equals(x.Nome, machineName, StringComparison.OrdinalIgnoreCase));
            if (pc == null)
            {
                error = "Macchina non trovata.";
                appLogger.Warn("MACHINE", $"Eliminazione fallita: macchina non trovata '{machineName}'.");
                return false;
            }

            try
            {
                RemovePcFromCategory(pc);

                Button btn = listaButton.FirstOrDefault(b => string.Equals(b.Name, machineName, StringComparison.OrdinalIgnoreCase));
                CheckBox chk = listaCheck.FirstOrDefault(c => string.Equals(c.Name, machineName, StringComparison.OrdinalIgnoreCase));
                if (btn != null) listaButton.Remove(btn);
                if (chk != null) listaCheck.Remove(chk);

                listamacchine.Remove(pc);
                credentialService.RemoveCredentialsForMachine(pc);
                Macchine.SaveSanitized(XmlFilePath, listamacchine);
                appLogger.Info("MACHINE", $"Macchina rimossa: {pc.Nome} ({pc.Ip}).");
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                appLogger.Error("MACHINE", $"Errore eliminazione '{machineName}': {ex.Message}");
                return false;
            }
        }

        private bool IsStrictCredentialModeEnabled()
        {
            bool strict = false;
            string raw = ConfigurationManager.AppSettings["Credentials.StrictMode"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                bool.TryParse(raw, out strict);
            }
            return strict;
        }

        private static bool IsAllowedType(string type)
        {
            return type == "CMP" || type == "TRD" || type == "DOK" ||
                   type == "SERVER" || type == "GW" || type == "MFC";
        }

        public Button loadBtn(PC pc, List<Button> list)
        {
            var btn = new Button
            {
                BackColor = Color.LightGray,
                Name = pc.Nome,
                Text = pc.Nome.ToUpperInvariant(),
                Size = new Size(80, 30),
                Tag = pc.Type
            };
            btn.Click += ClickRunTime;
            list.Add(btn);
            return btn;
        }

        public async void ClickRunTime(object sender, EventArgs e)
        {
            if (Form1.getQualeradio == 0)
            {
                await new Funzioni().openFolder(listamacchine, sender);
            }
            else
            {
                await new Funzioni().openRmDesk(listamacchine, sender);
            }
        }

        public CheckBox loadCheck(PC pc, List<CheckBox> list)
        {
            var chk = new CheckBox
            {
                Name = pc.Nome,
                Text = "",
                Size = new Size(15, 15),
                Tag = pc.Type
            };
            list.Add(chk);
            return chk;
        }

        public void loadListPC()
        {
            listamacchine.Clear();
            listamacchine.AddRange(listaCmp);
            listamacchine.AddRange(listaTrd);
            listamacchine.AddRange(listaDok);
            listamacchine.AddRange(listaSrv);
            listamacchine.AddRange(listaGw);
            listamacchine.AddRange(listaMfc);
        }

        public void loadListBTN()
        {
            listaButton.Clear();
            listaButton.AddRange(listaCmpButton);
            listaButton.AddRange(listaTrdButton);
            listaButton.AddRange(listaDokButton);
            listaButton.AddRange(listaSrvButton);
            listaButton.AddRange(listaGwButton);
            listaButton.AddRange(listaMfcButton);
        }

        public void loadListCHK()
        {
            listaCheck.Clear();
            listaCheck.AddRange(listaCmpCheck);
            listaCheck.AddRange(listaTrdCheck);
            listaCheck.AddRange(listaDokCheck);
            listaCheck.AddRange(listaSrvCheck);
            listaCheck.AddRange(listaGwCheck);
            listaCheck.AddRange(listaMfcCheck);
        }

        private void RemovePcFromCategory(PC pc)
        {
            string type = (pc.Type ?? string.Empty).Trim().ToUpperInvariant();
            switch (type)
            {
                case "CMP":
                    RemovePcAndUi(pc, listaCmp, listaCmpButton, listaCmpCheck);
                    break;
                case "TRD":
                    RemovePcAndUi(pc, listaTrd, listaTrdButton, listaTrdCheck);
                    break;
                case "DOK":
                    RemovePcAndUi(pc, listaDok, listaDokButton, listaDokCheck);
                    break;
                case "SERVER":
                    RemovePcAndUi(pc, listaSrv, listaSrvButton, listaSrvCheck);
                    break;
                case "GW":
                    RemovePcAndUi(pc, listaGw, listaGwButton, listaGwCheck);
                    break;
                case "MFC":
                    RemovePcAndUi(pc, listaMfc, listaMfcButton, listaMfcCheck);
                    break;
            }
        }

        private static void RemovePcAndUi(PC pc, List<PC> pcs, List<Button> buttons, List<CheckBox> checks)
        {
            int idx = pcs.IndexOf(pc);
            if (idx < 0) return;
            pcs.RemoveAt(idx);
            if (idx < buttons.Count) buttons.RemoveAt(idx);
            if (idx < checks.Count) checks.RemoveAt(idx);
        }
    }
}

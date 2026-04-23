using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools.Core.Models;
using System.Xml;

namespace Tools
{
    public class Macchine : IEnumerable<PC>
    {
        public string CollectionName;
        private readonly List<PC> pcArray = new List<PC>();
        public PC this[int index] => pcArray[index];

        public int Count => pcArray.Count;
        public IEnumerator<PC> GetEnumerator() => pcArray.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => pcArray.GetEnumerator();

        public void Add(PC newMacchina)
        {
            pcArray.Add(newMacchina);
        }

        public static bool HasLegacyCredentials(IEnumerable<PC> machines)
        {
            if (machines == null) return false;
            return machines.Any(pc =>
                pc != null &&
                (!string.IsNullOrWhiteSpace(pc.User) || !string.IsNullOrWhiteSpace(pc.Password)));
        }

        public static Dictionary<string, CategoryCredential> LoadCategoryCredentials(string filename)
        {
            var result = new Dictionary<string, CategoryCredential>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (!File.Exists(filename)) return result;

                var doc = LoadXmlDocumentSafe(filename);
                if (doc.DocumentElement == null) return result;

                var containers = new List<XmlNode>();
                containers.Add(doc.DocumentElement);

                var toolsConfigCategories = doc.SelectSingleNode("/ToolsConfig/ArrayOfCategory");
                if (toolsConfigCategories != null) containers.Add(toolsConfigCategories);

                var rootArrayOfPc = doc.SelectSingleNode("/ArrayOfPC");
                if (rootArrayOfPc != null) containers.Add(rootArrayOfPc);

                var rootCredCategory = doc.SelectSingleNode("/CredCategory");
                if (rootCredCategory != null) containers.Add(rootCredCategory);

                var embeddedCredCategory = doc.SelectSingleNode("/ArrayOfPC/CredCategory");
                if (embeddedCredCategory != null) containers.Add(embeddedCredCategory);

                foreach (var container in containers)
                {
                    CollectCategoryCredentialsFromContainer(container, result);
                }
            }
            catch
            {
                return result;
            }

            return result;
        }

        public static void SaveSanitized(string filename, IEnumerable<PC> machines)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false
            };

            string tempPath = filename + ".tmp";
            using (var writer = XmlWriter.Create(tempPath, settings))
            {
                writer.WriteStartDocument();
                writer.WriteComment(
                    "\r\nTipi possibili: CMP, TRD, DOK, SERVER, GW, MFC, OBTS\r\n" +
                    "Non case sensitive.\r\n" +
                    "Le funzioni (ad esempio ping) vengono eseguite tramite ip, il nome e solo grafico.\r\n" +
                    "Template credenziali categoria: lasciare vuoto per massima sicurezza e usare l'app per salvarle cifrate.\r\n" +
                    "Struttura: ToolsConfig/ArrayOfCategory + ToolsConfig/ArrayOfPC.\r\n");

                writer.WriteStartElement("ToolsConfig");

                writer.WriteStartElement("ArrayOfCategory");
                WriteSanitizedCategory(writer, "CMP");
                WriteSanitizedCategory(writer, "TRD");
                WriteSanitizedCategory(writer, "DOK");
                WriteSanitizedCategory(writer, "SERVER");
                WriteSanitizedCategory(writer, "GW");
                WriteSanitizedCategory(writer, "MFC");
                WriteSanitizedCategory(writer, "OBTS");
                writer.WriteEndElement();

                writer.WriteStartElement("ArrayOfPC");
                if (machines != null)
                {
                    foreach (var pc in machines)
                    {
                        if (pc == null) continue;

                        writer.WriteStartElement("PC");
                        writer.WriteElementString("Type", pc.Type ?? string.Empty);
                        writer.WriteElementString("Nome", pc.Nome ?? string.Empty);
                        writer.WriteElementString("Ip", pc.Ip ?? string.Empty);
                        writer.WriteStartElement("User");
                        writer.WriteFullEndElement();
                        writer.WriteStartElement("Password");
                        writer.WriteFullEndElement();
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }

            ReplaceFileAtomic(tempPath, filename);
        }

        public static MachineFileLoadResult DeserializeWithResult(string filename)
        {
            var result = new MachineFileLoadResult();
            try
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (stream.Length == 0)
                    {
                        serialize(filename);
                        result.TemplateCreated = true;
                        result.Messages.Add("File XML vuoto. Creato template di esempio.");
                        return result;
                    }
                }

                var doc = LoadXmlDocumentSafe(filename);

                XmlNode pcContainer = ResolvePcContainer(doc);
                if (pcContainer == null)
                {
                    result.Messages.Add("Formato XML non valido: nodo ArrayOfPC mancante.");
                    return result;
                }

                var list = new Macchine();
                foreach (XmlNode pcNode in pcContainer.SelectNodes("./PC"))
                {
                    if (pcNode.NodeType != XmlNodeType.Element) continue;

                    string type = ReadNodeValue(pcNode, "Type");
                    string nome = ReadNodeValue(pcNode, "Nome");
                    string ip = ReadNodeValue(pcNode, "Ip");
                    string user = ReadNodeValue(pcNode, "User") ?? ReadNodeValue(pcNode, "usr");
                    string pass = ReadNodeValue(pcNode, "Password") ?? ReadNodeValue(pcNode, "passw");

                    if (string.IsNullOrWhiteSpace(type) ||
                        string.IsNullOrWhiteSpace(nome) ||
                        string.IsNullOrWhiteSpace(ip))
                    {
                        continue;
                    }

                    list.Add(new PC
                    {
                        Type = type.Trim(),
                        Nome = nome.Trim(),
                        Ip = ip.Trim(),
                        User = string.IsNullOrWhiteSpace(user) ? string.Empty : user.Trim(),
                        Password = pass ?? string.Empty
                    });
                }

                result.Success = true;
                result.Machines = list;
                return result;
            }
            catch (FileNotFoundException)
            {
                serialize(filename);
                result.TemplateCreated = true;
                result.Messages.Add($"File non trovato: {filename}. Creato template di esempio.");
                return result;
            }
            catch (IOException ex)
            {
                result.Messages.Add($"File XML non accessibile: {ex.Message}");
                return result;
            }
            catch (XmlException ex)
            {
                result.Messages.Add($"XML non valido: {ex.Message}");
                return result;
            }
        }

        public static Macchine Deserialize(string filename)
        {
            var result = DeserializeWithResult(filename);
            return result.Success ? result.Machines : null;
        }

        public static void serialize(string filename)
        {
            string tempPath = filename + ".tmp";
            using (TextWriter writer = new StreamWriter(tempPath))
            {
                writer.Write(
                    "<!-- \r\nTipi possibili: CMP, TRD, DOK, SERVER, GW, MFC, OBTS\r\nNon case sensitive.\r\n" +
                    "Le funzioni (ad esempio ping) vengono eseguite tramite ip, il nome e solo grafico.\r\n" +
                    "Template credenziali categoria: lasciare vuoto per massima sicurezza e usare l'app per salvarle cifrate.\r\n" +
                    "Struttura: ToolsConfig/ArrayOfCategory + ToolsConfig/ArrayOfPC.\r\n-->\r\n\r\n" +
                    "<ToolsConfig>\r\n" +
                    "  <ArrayOfCategory>\r\n" +
                    "    <CMP>\r\n      <usr></usr>\r\n      <passw></passw>\r\n    </CMP>\r\n" +
                    "    <TRD>\r\n      <usr></usr>\r\n      <passw></passw>\r\n    </TRD>\r\n" +
                    "    <DOK>\r\n      <usr></usr>\r\n      <passw></passw>\r\n    </DOK>\r\n" +
                    "    <SERVER>\r\n      <usr></usr>\r\n      <passw></passw>\r\n    </SERVER>\r\n" +
                    "    <GW>\r\n      <usr></usr>\r\n      <passw></passw>\r\n    </GW>\r\n" +
                    "    <MFC>\r\n      <usr></usr>\r\n      <passw></passw>\r\n    </MFC>\r\n" +
                    "    <OBTS>\r\n      <usr></usr>\r\n      <passw></passw>\r\n    </OBTS>\r\n" +
                    "  </ArrayOfCategory>\r\n" +
                    "  <ArrayOfPC>\r\n" +
                    "    <PC>\r\n      <Type>cmp</Type>\r\n      <Nome>cmp01</Nome>\r\n      <Ip>10.1.146.156</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>cmp</Type>\r\n      <Nome>cmp02</Nome>\r\n      <Ip>10.1.146.157</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>trd</Type>\r\n      <Nome>trd01</Nome>\r\n      <Ip>10.1.146.167</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>trd</Type>\r\n      <Nome>trd02</Nome>\r\n      <Ip>10.1.146.168</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>trd</Type>\r\n      <Nome>trd03</Nome>\r\n      <Ip>10.1.146.169</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>DOK</Type>\r\n      <Nome>DOK01</Nome>\r\n      <Ip>10.1.146.173</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>DOK</Type>\r\n      <Nome>DOK02</Nome>\r\n      <Ip>10.1.146.174</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>SERVER</Type>\r\n      <Nome>SERVER-1</Nome>\r\n      <Ip>10.1.146.3</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>SERVER</Type>\r\n      <Nome>SERVER-2</Nome>\r\n      <Ip>10.1.146.4</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>GW</Type>\r\n      <Nome>GW-1</Nome>\r\n      <Ip>10.1.146.7</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>GW</Type>\r\n      <Nome>GW-2</Nome>\r\n      <Ip>10.1.146.8</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "    <PC>\r\n      <Type>MFC</Type>\r\n      <Nome>MFC01</Nome>\r\n      <Ip>10.1.146.101</Ip>\r\n      <User></User>\r\n      <Password></Password>\r\n    </PC>\r\n" +
                    "  </ArrayOfPC>\r\n" +
                    "</ToolsConfig>");
            }

            ReplaceFileAtomic(tempPath, filename);

            // Non aprire editor automaticamente dal Core.
        }

        private static void WriteSanitizedCategory(XmlWriter writer, string category)
        {
            writer.WriteStartElement(category);
            writer.WriteWhitespace("\r\n      ");
            writer.WriteStartElement("usr");
            writer.WriteFullEndElement();
            writer.WriteWhitespace("\r\n      ");
            writer.WriteStartElement("passw");
            writer.WriteFullEndElement();
            writer.WriteWhitespace("\r\n    ");
            writer.WriteEndElement();
        }

        private static string ReadNodeValue(XmlNode parent, string childName)
        {
            foreach (XmlNode child in parent.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                if (string.Equals(child.Name, childName, StringComparison.OrdinalIgnoreCase))
                {
                    return child.InnerText;
                }
            }
            return null;
        }

        private static void CollectCategoryCredentialsFromContainer(XmlNode container, IDictionary<string, CategoryCredential> output)
        {
            if (container == null || output == null) return;

            foreach (XmlNode node in container.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element) continue;
                if (string.Equals(node.Name, "PC", StringComparison.OrdinalIgnoreCase)) continue;

                string category = node.Name.Trim().ToUpperInvariant();
                if (string.Equals(category, "CATEGORY", StringComparison.OrdinalIgnoreCase))
                {
                    category = (ReadNodeValue(node, "Type") ?? string.Empty).Trim().ToUpperInvariant();
                }

                if (category != "CMP" && category != "TRD" && category != "DOK" &&
                    category != "SERVER" && category != "GW" && category != "MFC" &&
                    category != "OBTS")
                {
                    continue;
                }

                string user = ReadNodeValue(node, "usr") ?? ReadNodeValue(node, "user");
                string pass = ReadNodeValue(node, "passw") ?? ReadNodeValue(node, "password");

                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                {
                    output[category] = new CategoryCredential
                    {
                        User = user.Trim(),
                        Password = pass
                    };
                }
            }
        }

        private static XmlNode ResolvePcContainer(XmlDocument doc)
        {
            if (doc == null) return null;

            XmlNode container = doc.SelectSingleNode("/ToolsConfig/ArrayOfPC");
            if (container != null) return container;

            container = doc.SelectSingleNode("/ArrayOfPC");
            if (container != null) return container;

            return null;
        }

        private static XmlDocument LoadXmlDocumentSafe(string filename)
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            var doc = new XmlDocument
            {
                XmlResolver = null
            };

            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = XmlReader.Create(stream, settings))
            {
                doc.Load(reader);
            }

            return doc;
        }

        private static void ReplaceFileAtomic(string tempPath, string targetPath)
        {
            if (File.Exists(targetPath))
            {
                File.Replace(tempPath, targetPath, null, true);
            }
            else
            {
                File.Move(tempPath, targetPath);
            }
        }
    }

    public class CategoryCredential
    {
        public string User { get; set; }
        public string Password { get; set; }
    }
}

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace Tools
{
    public class Macchine : ICollection
    {
        public string CollectionName;
        private ArrayList pcArray = new ArrayList();
        public PC this[int index] => (PC)pcArray[index];

        public void CopyTo(Array a, int index)
        {
            pcArray.CopyTo(a, index);
        }

        public int Count => pcArray.Count;

        public object SyncRoot => this;

        public bool IsSynchronized => false;

        public IEnumerator GetEnumerator() => pcArray.GetEnumerator();

        public void Add(PC newMacchina)
        {
            pcArray.Add(newMacchina);
        }

        //LEGGE DAL FILE XML
        public static Macchine Deserialize(string filename)
        {
            

                XmlSerializer serializer = new XmlSerializer(typeof(Macchine));
                try
                {

                    FileStream stream = new FileStream(filename, FileMode.Open);

                    if (stream.Length == 0) {
                        MessageBox.Show("File Vuoto!\nCreo file con un esempio di CMP");
                        stream.Close();
                        Macchine.serialize("Tool_List.xml");
                        return null;

                    }

                    return (Macchine)serializer.Deserialize(stream);


                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("File non trovato!\nCreo file \"Tool_List.xml\"");
                    Macchine.serialize("Tool_List.xml");
                    return null;
                }
             

           //}
        }

        //SE FILE XML NON TROVATO LO CREA CON QUALCHE MACCHINA DI ESEMPIO
        public static void serialize(String filename)
        {
            
            using (TextWriter writer = new StreamWriter(@filename))
            {

                writer.Write("<!-- \r\nTipi possibili: CMP, TRD, DOK, SERVER, GW, MFC\r\nNon case sensitive.\r\nLe " +
                    "funzioni (ad esempio ping) vengono eseguite tramite ip, il nome è solo \"grafico\"\r\n-->\r\n\r\n<ArrayOfPC>\r\n<PC>\r\n    " +
                    "<Type>cmp</Type>\r\n    <Nome>cmp01</Nome>\r\n    <Ip>10.1.146.156</Ip>\r\n    <User>sms</User>\r\n    " +
                    "<Password>ppa</Password>\r\n  </PC>\r\n<PC>\r\n    <Type>cmp</Type>\r\n    <Nome>cmp02</Nome>\r\n    " +
                    "<Ip>10.1.146.157</Ip>\r\n    <User>sms</User>\r\n    <Password>ppa</Password>\r\n  </PC>\r\n<PC>\r\n    " +
                    "<Type>trd</Type>\r\n    <Nome>trd01</Nome>\r\n    <Ip>10.1.146.167</Ip>\r\n    <User>sms</User>\r\n    " +
                    "<Password>ppa</Password>\r\n  </PC>\r\n<PC>\r\n    <Type>trd</Type>\r\n    <Nome>trd02</Nome>\r\n    " +
                    "<Ip>10.1.146.168</Ip>\r\n    <User>sms</User>\r\n    <Password>ppa</Password>\r\n  </PC>\r\n<PC>\r\n   " +
                    " <Type>trd</Type>\r\n    <Nome>trd03</Nome>\r\n    <Ip>10.1.146.169</Ip>\r\n    <User>sms</User>\r\n    " +
                    "<Password>ppa</Password>\r\n  </PC>\r\n <PC>\r\n    <Type>DOK</Type>\r\n    <Nome>DOK01</Nome>\r\n    " +
                    "<Ip>10.1.146.173</Ip>\r\n    <User>sms</User>\r\n    <Password>ppa</Password>\r\n  </PC>\r\n<PC>\r\n    " +
                    "<Type>DOK</Type>\r\n    <Nome>DOK02</Nome>\r\n    <Ip>10.1.146.174</Ip>\r\n    <User>sms</User>\r\n    " +
                    "<Password>ppa</Password>\r\n  </PC>\r\n<PC>\r\n    <Type>SERVER</Type>\r\n    <Nome>SERVER-1</Nome>\r\n    " +
                    "<Ip>10.1.146.3</Ip>\r\n    <User>sms</User>\r\n    <Password>ppa</Password>\r\n  </PC>\r\n<PC>\r\n    " +
                    "<Type>SERVER</Type>\r\n    <Nome>SERVER-2</Nome>\r\n    <Ip>10.1.146.4</Ip>\r\n    <User>sms</User>\r\n    " +
                    "<Password>ppa</Password>\r\n  </PC>\r\n<PC>\r\n    <Type>GW</Type>\r\n    <Nome>GW-1</Nome>\r\n    " +
                    "<Ip>10.1.146.7</Ip>\r\n    <User>seastema</User>\r\n    <Password>.1q2w3e!</Password>\r\n  </PC>\r\n<PC>\r\n    " +
                    "<Type>GW</Type>\r\n    <Nome>GW-2</Nome>\r\n    <Ip>10.1.146.8</Ip>\r\n    <User>seastema</User>\r\n    " +
                    "<Password>.1q2w3e!</Password>\r\n  </PC>\r\n<PC>\r\n    <Type>MFC</Type>\r\n    <Nome>MFC01</Nome>\r\n    " +
                    "<Ip>10.1.146.101</Ip>\r\n    <User>seastema</User>\r\n    <Password>.1q2w3e!</Password>\r\n  </PC>\r\n </ArrayOfPC>");
                writer.Close();
                var open = new ProcessStartInfo("notepad.exe");
                open.Arguments = "Tool_List.xml";
                Process.Start(open);
            }
        }
    }
}

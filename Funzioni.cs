using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Tools
{
    internal class Funzioni
    {
        //TEMPO IN MILLISECONDI

        
        public void wait(int milliseconds)
        {
            var timer1 = new System.Windows.Forms.Timer();
            if (milliseconds == 0 || milliseconds < 0) return;

            // Console.WriteLine("start wait timer");
            timer1.Interval = milliseconds;
            timer1.Enabled  = true;
            timer1.Start();

            timer1.Tick += (s, e) =>
            {
                timer1.Enabled = false;
                timer1.Stop();
                // Console.WriteLine("stop wait timer");
            };

            while (timer1.Enabled)
            {
                Application.DoEvents();
            }
        }


        public void evento(ArrayList listPc, ArrayList listaBTN, ArrayList listaCHK, string type)
        {


            for (int i = 0; i < listaCHK.Count; i++)


            {
                CheckBox chk = (CheckBox)listaCHK[i];
                PC pc = (listPc[i] as PC);
                Button btn = (listaBTN[i] as Button);

                if (chk.Checked)
                {

                if (type == "ping")
                    {

                        try
                        {
                         
                             Ping myPing = new Ping();
                             PingReply reply = myPing.Send(pc.Ip, 10);




                            if (reply.Status == IPStatus.Success) {

                                /*
                                string name = GetComputerName(pc.Ip);

                                //string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);

                                //if (pc.Nome == name)
                                if (string.Equals(pc.Nome, name, StringComparison.CurrentCultureIgnoreCase))
                                {
                                */
                                    //btn.Text = pc.Nome;
                                    btn.BackColor = Color.FromArgb(0, 255, 0);
                                /*
                                }
                                else 
                                {
                                    //btn.Text = pc.Nome;
                                    btn.BackColor = Color.FromArgb(255, 255, 0);
                                } */
                            }
                            else
                            {
                                //btn.Text = pc.Nome;
                                btn.BackColor = Color.FromArgb(255, 0, 0);
                                chk.Checked = false;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }

                if (type == "riavvio")
                    {

                        try

                        {
                            btn.Text = "RIAVVIO";
                            btn.BackColor = Color.FromArgb(0, 255, 0);
                            //prova shutdown
                            var credenziali = new ProcessStartInfo("net", String.Format(@" use \\{0}\IPC$ {1} /USER:{2}", pc.Ip, pc.Password, pc.User));
                            credenziali.CreateNoWindow = true;
                            credenziali.UseShellExecute = false;
                            Process.Start(credenziali);
                            var shutdown = new ProcessStartInfo("shutdown", String.Format(@" -m \\{0} -r -f -t 0", pc.Ip));
                            shutdown.CreateNoWindow = true;
                            shutdown.UseShellExecute = false;

                            //RIPETI 3 VOLTE
                            for (int r=0; r <3;r++) {
                                Process.Start(shutdown);

                                wait(500);

                            }

                            if (shutdown.ErrorDialog == true)
                            {
                                // = ($"ERROR RIAVVIO{scritta}\r - {pc.Nome}");

                                // btn.Text = "ERROR";
                                btn.BackColor = Color.FromArgb(0, 255, 0);
                                chk.Checked = false;
                            }
                            else
                            {
                                //btn.Text = pc.Nome + "OK";
                                btn.BackColor = Color.FromArgb(255, 0, 0);
                                chk.Checked = true;
                            }
                        }

                        catch (Exception)
                        {

                        }


                    }

                if (type == "off")
                    {

                        try

                        {
                            btn.Text = "SPENGO";
                            btn.BackColor = Color.FromArgb(0, 255, 0);
                            //prova shutdown
                            var credenziali = new ProcessStartInfo("net", String.Format(@" use \\{0}\IPC$ {1} /USER:{2}", pc.Ip, pc.Password, pc.User));
                            credenziali.CreateNoWindow = true;
                            credenziali.UseShellExecute = false;
                            Process.Start(credenziali);
                            var shutdown = new ProcessStartInfo("shutdown", String.Format(@" -m \\{0} -s -f -t 5", pc.Ip));
                            shutdown.CreateNoWindow = true;
                            shutdown.UseShellExecute = false;

                            //RIPETI 3 VOLTE
                            for (int r = 0; r < 3; r++)
                            {
                                Process.Start(shutdown);
                                wait(500);
                            }

                            if (shutdown.ErrorDialog == true)
                            {
                          
                                btn.BackColor = Color.FromArgb(255, 0, 0);
                                chk.Checked = false;
                            }
                            else
                            {
                                //  btn.Text = "SPENTO";
                                btn.BackColor = Color.FromArgb(255, 0, 0);
                                chk.Checked = false;
                            }
                        }

                        catch (Exception)
                        {

                        }


                    }

                }


            }

        }

        private void _processKill(string macchina, string userM, string passwM)
        {

            try
            {
                string name = macchina;
                string user = userM;
                string passw = passwM;



                var lam = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM LAM4g.exe", name, user, passw));
                lam.CreateNoWindow = true;
                lam.UseShellExecute = false;

                Process.Start(lam);
                var messapp = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM MessApp.exe", name, user, passw));
                messapp.CreateNoWindow = true;
                messapp.UseShellExecute = false;
                Process.Start(messapp);
                var minacolp = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM MINACOLP.exe", name, user, passw));
                minacolp.CreateNoWindow = true;
                minacolp.UseShellExecute = false;
                Process.Start(minacolp);
                var dda = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM DDA.exe", name, user, passw));
                dda.CreateNoWindow = true;
                dda.UseShellExecute = false;
                Process.Start(dda);
                var dssclient = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM DSSClientPrj.exe", name, user, passw));
                dssclient.CreateNoWindow = true;
                dssclient.UseShellExecute = false;
                Process.Start(dssclient);
                var Sico = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM Sico.exe", name, user, passw));
                Sico.CreateNoWindow = true;
                Sico.UseShellExecute = false;
                Process.Start(Sico);
                var TRDfilterWD = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM TRDfilterWD.exe", name, user, passw));
                TRDfilterWD.CreateNoWindow = true;
                TRDfilterWD.UseShellExecute = false;
                Process.Start(TRDfilterWD);
                var sdclient = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM sdclient.exe", name, user, passw));
                sdclient.CreateNoWindow = true;
                sdclient.UseShellExecute = false;
                Process.Start(sdclient);
                var SCS = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM SCS.exe", name, user, passw));
                SCS.CreateNoWindow = true;
                SCS.UseShellExecute = false;
                Process.Start(SCS);
                var MMI45 = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM MMI45.exe", name, user, passw));
                MMI45.CreateNoWindow = true;
                MMI45.UseShellExecute = false;
                Process.Start(MMI45);
                var ScsInitApp = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM ScsInitApp.exe", name, user, passw));
                ScsInitApp.CreateNoWindow = true;
                ScsInitApp.UseShellExecute = false;
                Process.Start(ScsInitApp);
                var RSAManagerSimulator = new ProcessStartInfo("taskkill", String.Format(@"/S {0} /U {1} /P {2} /IM RSAManagerSimulator.exe", name, user, passw));
                RSAManagerSimulator.CreateNoWindow = true;
                RSAManagerSimulator.UseShellExecute = false;
                Process.Start(RSAManagerSimulator);
            }

            catch (Exception)
            {

                //errmsg.Text = ($"salta in eccezione");
            }
        }

        public void processKill(ArrayList listPc, ArrayList listaBTN, ArrayList listaCHK)
        {

            for (int i = 0; i < listaCHK.Count; i++)
            {

                CheckBox chk = (CheckBox)listaCHK[i];
                PC pc = (listPc[i] as PC);
                Button btn = (listaBTN[i] as Button);



                if (chk.Checked)

                {

                    try
                    {

                        //RIPETI 3 VOLTE
                        for (int r = 0; r < 3; r++)
                        {
                            _processKill(pc.Ip, pc.User, pc.Password);

                            wait(500);

                        }
                        

                        //if (errmsg.Text.Count() == 0)
                        //{
                        //    scritta = $"Provo a chiudere i processi su:\r\n";
                        //    errmsg.Text = ($"{scritta}\r{PC.Nome}");
                        //    macchina.Text = ($"{PC.Nome}");
                        //    macchina.BackColor = Color.Blue;
                        //}
                        //else
                        //{
                        //    errmsg.Text = ($"{scritta}\r - {PC.Nome}");
                        //    macchina.BackColor = Color.Blue;
                        //}


                    }

                    catch (Exception)
                    {

                        // errmsg.Text = ($"salta in eccezione");
                    }
                }


            }


        }

        public void openFolder(ArrayList listPc, object sender)
        {


            {

                Button btn = sender as Button;
                int questo = 0;
                foreach (PC obj in listPc)
                {
                    if ((btn.Name).Equals(obj.Nome, StringComparison.OrdinalIgnoreCase))
                    {
                        questo = listPc.IndexOf(obj);
                        break;
                    }
                }
                PC pc = (listPc[questo] as PC);

                try
                {
                    Ping myPing = new Ping();
                    PingReply reply = myPing.Send(pc.Ip, 15);

                    if (reply.Status == IPStatus.Success)
                    {
                        btn.BackColor = Color.FromArgb(0, 255, 0);
                        var credenziali = new ProcessStartInfo("net", String.Format(@"use \\{0}\C$ {1} /USER:{2}", pc.Ip, pc.Password, pc.User));
                        credenziali.CreateNoWindow = true;
                        credenziali.UseShellExecute = false;
                        Process.Start(credenziali);
                        var open = new ProcessStartInfo("explorer", String.Format(@"\\{0}\c$", pc.Ip));
                        wait(500);
                        Process.Start(open);
                    }
                    else
                    {
                        //btn.Text = pc.Nome;
                        btn.BackColor = Color.FromArgb(255, 0, 0);
                    }

                }
                catch (Exception)
                { }
            }
        }
        public void openRmDesk(ArrayList listPc, object sender)
        {


            {

                Button btn = sender as Button;
                int questo = 0;
                foreach (PC obj in listPc)
                {
                    if ((btn.Name).Equals(obj.Nome, StringComparison.OrdinalIgnoreCase))
                    {
                        questo = listPc.IndexOf(obj);
                        break;
                    }
                }
                PC pc = (listPc[questo] as PC);
                try
                {

                    //    cmdkey /generic:"192.1.2.170" /user:"sms" /pass:"ppa"
                    //or
                    // cmdkey /generic:192.1.2.170 /user:sms /pass:ppa

                    //    mstsc /v:192.1.2.170
                    //    cmdkey /delete:192.1.2.170

                    Ping myPing = new Ping();
                    PingReply reply = myPing.Send(pc.Ip, 15);

                    if (reply.Status == IPStatus.Success)
                    {

                        btn.BackColor = Color.FromArgb(0, 255, 0);

                        var credenziali = new ProcessStartInfo("cmdkey", String.Format(@"/generic:{0} /user:{1} /pass:{2}", pc.Ip, pc.User, pc.Password));
                        credenziali.CreateNoWindow = true;
                        credenziali.UseShellExecute = false;

                        Process.Start(credenziali);
                        wait(500);
                        var open = new ProcessStartInfo("mstsc", String.Format(@"/v:{0} /multimon", pc.Ip));
                        Process.Start(open);
                        wait(1000);

                        var DELcredenziali = new ProcessStartInfo("cmdkey", String.Format(@"/delete:{0}", pc.Ip));
                        DELcredenziali.CreateNoWindow = true;
                        DELcredenziali.UseShellExecute = false;

                        Process.Start(DELcredenziali);
                    }
                    else
                    {
                        //btn.Text = pc.Nome;
                        btn.BackColor = Color.FromArgb(255, 0, 0);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        // COPIA CARTELLE
        
        public void copyFolder(ArrayList listPc, ArrayList listaBTN, ArrayList listaCHK, string origine, string destin)
        {
            // COPIA CARTELLE PROVA
            if (origine != "" && destin != "")
            {
               
                
                
                for (int i = 0; i < listPc.Count; i++)


                {
                    CheckBox chk = (CheckBox)listaCHK[i];
                    PC pc = (listPc[i] as PC);
                    Button btn = (listaBTN[i] as Button);

                    if (chk.Checked)
                    {

                        try
                        {
                        
                            Ping myPing = new Ping();
                            PingReply reply = myPing.Send(pc.Ip, 200);
                            if (reply.Status == IPStatus.Success)
                            {
                                btn.BackColor = Color.FromArgb(0, 255, 0);

                            var credenziali = new ProcessStartInfo("net", String.Format(@"use \\{0}\IPC$ {1} /USER:{2}", pc.Ip, pc.Password, pc.User));

                            string pathOrig = origine;
                            string pathDest = @"\\" + pc.Ip + @"\c$" + destin;

                            Process.Start("Xcopy", String.Format(@"{0} {1} /E /H /C /I /q /k /y", pathOrig, pathDest));
                            Process.Start("explorer", String.Format(@"{0}", pathDest));
                            }
                            else
                            {
                                //btn.Text = pc.Nome;
                                btn.BackColor = Color.FromArgb(255, 0, 0);
                            }

                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Copy exception");
                        }
                    }
                }
            }
            else { MessageBox.Show("Specificare Origine e Destinazione"); }

        }

        public string GetComputerName(string clientIP)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(clientIP);
                string firstBit = hostEntry.HostName.Split('.')[0];
                return firstBit;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }


    } //class
} //namespace





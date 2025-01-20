using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Tools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            lista.loadPC();
            lista.loadListPC();
            lista.loadListBTN();
            lista.loadListCHK();
            caricaBtn();
            caricaCheck();
            textDest.Text = @"\sms-xxx\bin\  (es.)";
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        Liste lista = new Liste();
        Funzioni function = new Funzioni();
        static int qualeRadio = 0;

        public static int getQualeradio { get => qualeRadio; }

        private void caricaBtn()
        {

            foreach (Button btn in lista.getCmpButton)
            {
                panelCMP.Controls.Add(btn);
            }
            foreach (Button btn in lista.getTrdButton)
            {
                panelTRD.Controls.Add(btn);
            }
            foreach (Button btn in lista.getDokButton)
            {
                panelDOK.Controls.Add(btn);
            }
            foreach (Button btn in lista.getSrvButton)
            {
                panelSRV.Controls.Add(btn);
            }
            foreach (Button btn in lista.getGwButton)
            {
                panelGW.Controls.Add(btn);
            }
            foreach (Button btn in lista.getMfcButton)
            {
                panelMfc.Controls.Add(btn);
            }

        }
        private void caricaCheck()
        {


            foreach (CheckBox check in lista.getCmpCheck)
            {
                panelCMP.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getTrdCheck)
            {
                panelTRD.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getDokCheck)
            {
                panelDOK.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getSrvCheck)
            {
                panelSRV.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getGwCheck)
            {
                panelGW.Controls.Add(check);
            }
            foreach (CheckBox check in lista.getMfcCheck)
            {
                panelMfc.Controls.Add(check);
            }


        }

       
        //PROVA ping ASYNC iniziale al momento non è asincrono
        private async Task selTutto() {
            foreach (CheckBox check in lista.getlistaCheck) check.Checked = true;
        }

        //NUOVO SX SEL TUTTO, DX DESEL. TUTTO
        private void panelCMP_MouseClick(object sender, MouseEventArgs e)
        {
   
                foreach (CheckBox check in lista.getCmpCheck)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        check.Checked = false;
                    }
                    else
                    {
                        check.Checked = true;
                    }
                }
        }

        private void panelTRD_MouseClick(object sender, MouseEventArgs e)
        {
                    foreach (CheckBox check in lista.getTrdCheck)
                    {
                        if (e.Button == System.Windows.Forms.MouseButtons.Right)
                        {
                            check.Checked = false;
                        }
                        else
                        {
                            check.Checked = true;
                        }
                    }
        }

        private void panelSRV_MouseClick(object sender, MouseEventArgs e)
        {
                    foreach (CheckBox check in lista.getSrvCheck)
                    {
                        if (e.Button == System.Windows.Forms.MouseButtons.Right)
                        {
                            check.Checked = false;
                        }
                        else
                        {
                            check.Checked = true;
                        }
                    }
            }

        private void panelGW_MouseClick(object sender, MouseEventArgs e)
        {
                foreach (CheckBox check in lista.getGwCheck)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        check.Checked = false;
                    }
                    else
                    {
                        check.Checked = true;
                    }
                }
            }

        private void panelDOK_MouseClick(object sender, MouseEventArgs e)
        {
                foreach (CheckBox check in lista.getDokCheck)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        check.Checked = false;
                    }
                    else
                    {
                        check.Checked = true;
                    }
                }
        }
        private void panelMFC_MouseClick(object sender, MouseEventArgs e)
        {
                foreach (CheckBox check in lista.getMfcCheck)
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    {
                        check.Checked = false;
                    }
                    else
                    {
                        check.Checked = true;
                    }
                }
        }

        private void ping_MouseClick(object sender, MouseEventArgs e)
        {
            refresh();
            function.evento(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, "ping");
        }

        private void refresh()
        {
            foreach (Button btn in lista.getlistaButton)
            {
                btn.Text = btn.Name.ToUpper();
                btn.BackColor = Color.LightGray;
            }
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            refresh();
        }

        private void Riavvio_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.label1.Text = "Sicuro di voler riavviare le macchine selezionate?";
            form2.ShowDialog();
           
            if (form2.getScelta == 1)
            {
                form2.Close();
                function.evento(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, "riavvio");
            }
            form2.Close();
        }

        private void off_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.label1.Text= "Sicuro di voler spegnere le macchine selezionate?";   
            form2.ShowDialog();
            if (form2.getScelta == 1)
            {
                form2.Close();
                function.evento(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, "off");
            }
            form2.Close();
        }

        private void Kill_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.label1.Text = "Vuoi Killare i proc. sulle macchine selezionate?";
            form2.ShowDialog();
            if (form2.getScelta == 1)
            {
                form2.Close();
                function.processKill(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck);
            }
            form2.Close();
        }

        private void Changeradio(object sender, EventArgs e)
        {
            if (radioFile.Checked)
            {
                qualeRadio = 0;
            }
            else { qualeRadio = 1; }
        }

        public string readOrigin()
        {
             return textorigine.Text;
        }
        public string readDest()
        {
            return textDest.Text;
        }

        private void textorigine_TextChanged(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textorigine.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void CopyBtn_Click(object sender, EventArgs e)
        {
            refresh();
            function.copyFolder(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, readOrigin(), readDest());
        }
    }
}
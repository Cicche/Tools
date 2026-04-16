using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tools
{
    public partial class Form2 : Form
    {
        private int scelta = 0;

        public int getScelta { get => scelta; }

        public Form2()
        {
            InitializeComponent();
        }

        Liste lista = new Liste();
        Funzioni function = new Funzioni();

        private void ok_Click(object sender, EventArgs e)
        {

            if (radioSi.Checked)
            {
                scelta = 1;
                Hide();
            }
            else { scelta = 0; Hide(); }
        
        }

        private void radioSi_CheckedChanged(object sender, EventArgs e)
        {
            if (radioSi.Checked)
            {
                ok.Text = "CONFERMA";    
            }
            else { ok.Text = "ANNULLA";  }
        }
    }
}

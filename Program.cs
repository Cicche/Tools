using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tools
{
    internal static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MessageBox.Show(
                "La UI WinForms e deprecata.\nAvvia il progetto Tools.Wpf per usare la versione supportata.",
                "Tools - WinForms deprecato",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
    
}

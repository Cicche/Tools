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
            Application.Run(new Form1());
            //Funzioni function = new Funzioni();
            //Liste lista= new Liste();
            //function.pingAllasync(lista.getlistamacchine, lista.getlistaButton, lista.getlistaCheck, "ping");
        }
    }
    
}

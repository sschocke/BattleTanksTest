using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace BattleTanksTest
{
    static class Program
    {
        private static Form1 mainForm;
        public static Form1 MainForm { get { return mainForm; } }

        //public static StreamWriter abDebug;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new Form1();
            //abDebug = new StreamWriter("abDebug.txt");
            Application.Run(mainForm);
        }
    }
}

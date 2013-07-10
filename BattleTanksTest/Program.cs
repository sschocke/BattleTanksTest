using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BattleTanksTest
{
    static class Program
    {
        private static Form1 mainForm;
        public static Form1 Main { get { return mainForm; } }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new Form1();
            Application.Run(mainForm);
        }
    }
}

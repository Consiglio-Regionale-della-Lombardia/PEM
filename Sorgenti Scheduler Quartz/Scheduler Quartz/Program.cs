using System;
using System.Windows.Forms;
using PortaleRegione.Logger;

namespace Scheduler
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Log.Initialize();
            Application.Run(new MainPage());
        }
    }
}
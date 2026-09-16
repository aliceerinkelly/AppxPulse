using System;
using System.Windows.Forms;

namespace AppXPulse
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Explicitly run the Form class from our custom namespace
            Application.Run(new AppXPulse.Form());
        }
    }
}

using System;
using System.Windows.Forms;

namespace MinigameMenu
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Launch your menu form
            Application.Run(new MainMenuForm());
        }
    }
}
using System;
using System.Windows.Forms;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.UI;

namespace SarasaviLibrarySystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            LibraryDbContext.InitializeDatabase();
            Application.Run(new MainForm());
        }
    }
}
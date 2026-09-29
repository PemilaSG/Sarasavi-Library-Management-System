using System;
using System.Windows.Forms;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Services;
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

            var catalogService = new CatalogService();
            catalogService.AddBookTitleWithCopies("Harry Potter 3", "J.K. Rowling", "Bloomsbury Publishing", 'F', 3, false, out string msg);

            Application.Run(new MainForm());
        }
    }
}
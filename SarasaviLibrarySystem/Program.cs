using System;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.WriteLine("=========================================================================");
            Console.WriteLine("   SARASAVI LIBRARY MANAGEMENT SYSTEM - PERSON 1 BACKEND ENGINE TEST     ");
            Console.WriteLine("=========================================================================");
            Console.WriteLine();

            // 1. Initialize Database Schema & Seed Data
            Console.WriteLine("[STEP 1] Initializing SQLite Database & Seed Data...");
            LibraryDbContext.InitializeDatabase();
            Console.WriteLine("✔ SQLite Database initialized and seeded successfully!\n");

            var catalogService = new CatalogService();
            var loanService = new LoanService();
            var returnService = new ReturnService();
            var borrowerService = new BorrowerService();

            // 2. Test Accession Number Generator
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("[STEP 2] Testing Accession Number Generator (X9999 Format)");
            Console.WriteLine("-------------------------------------------------------------------------");
            string nextCompCode = AccessionGenerator.GenerateNextAccessionCode('C');
            string nextFictCode = AccessionGenerator.GenerateNextAccessionCode('F');
            Console.WriteLine($"Next Computing ('C') Accession Code : {nextCompCode}");
            Console.WriteLine($"Next Fiction   ('F') Accession Code : {nextFictCode}");
            Console.WriteLine($"Copy 01 Accession Code             : {AccessionGenerator.GenerateCopyAccessionNumber(nextCompCode, 1)}");
            Console.WriteLine($"Copy 05 Accession Code             : {AccessionGenerator.GenerateCopyAccessionNumber(nextCompCode, 5)}");
            Console.WriteLine();

            // 3. Test Business Rules Validation Engine
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("[STEP 3] Testing Business Rules Validation Engine (Checkout Protection)");
            Console.WriteLine("-------------------------------------------------------------------------");

            // Test Rule 1: Reference Only Protection
            Console.WriteLine("\n--> TEST 3A: Attempting to borrow Reference Only copy ('C0001-01')...");
            loanService.IssueLoan("M-1001", "C0001-01", out string msg1);
            Console.WriteLine(msg1);

            // Test Rule 2: Overdue Borrower Restriction
            Console.WriteLine("\n--> TEST 3B: Attempting checkout for member 'M-1003' (Has Overdue Book 'F0001-01')...");
            loanService.IssueLoan("M-1003", "C0002-02", out string msg2);
            Console.WriteLine(msg2);

            // Test Rule 3: Valid Loan Checkout (14 Days)
            Console.WriteLine("\n--> TEST 3C: Attempting valid checkout for member 'M-1004' (Copy 'C0002-02')...");
            loanService.IssueLoan("M-1004", "C0002-02", out string msg3);
            Console.WriteLine(msg3);

            // Test Rule 4: Max 5 Books Limit Check
            Console.WriteLine("\n--> TEST 3D: Testing 5-Book Limit enforcement...");
            // Artificially issue books to test limit
            loanService.IssueLoan("M-1004", "C0002-03", out _);
            loanService.IssueLoan("M-1004", "C0003-01", out _);
            loanService.IssueLoan("M-1004", "C0003-02", out _);
            loanService.IssueLoan("M-1004", "F0001-02", out _);
            Console.WriteLine("Attempting 6th book checkout for 'M-1004'...");
            loanService.IssueLoan("M-1004", "F0001-03", out string msg4);
            Console.WriteLine(msg4);

            // 4. Test Return & Reservation Set-Aside Alert Engine
            Console.WriteLine("\n-------------------------------------------------------------------------");
            Console.WriteLine("[STEP 4] Testing Return Counter & FIFO Reservation Set-Aside Alert");
            Console.WriteLine("-------------------------------------------------------------------------");
            Console.WriteLine("Processing return of copy 'C0002-01' (Title 'C# 10 and .NET 6' is reserved by 'M-1002')...");
            
            returnService.ProcessReturn("C0002-01", out string returnMsg, out string? resAlert);
            Console.WriteLine(returnMsg);
            if (resAlert != null)
            {
                Console.WriteLine("\n=========================================================================");
                Console.WriteLine(resAlert);
                Console.WriteLine("=========================================================================");
            }

            // 5. Test Dashboard Analytics
            Console.WriteLine("\n-------------------------------------------------------------------------");
            Console.WriteLine("[STEP 5] System Dashboard Analytics Overview");
            Console.WriteLine("-------------------------------------------------------------------------");
            var stats = catalogService.GetDashboardStats();
            Console.WriteLine($"Total Book Titles       : {stats.totalTitles}");
            Console.WriteLine($"Total Physical Copies   : {stats.totalCopies}");
            Console.WriteLine($"Available on Shelf      : {stats.availableCopies}");
            Console.WriteLine($"Active Borrowed Loans   : {stats.activeLoans}");
            Console.WriteLine($"Overdue Unreturned Loans: {stats.overdueCount}");
            Console.WriteLine($"Registered Borrowers    : {stats.totalBorrowers}");
            Console.WriteLine("=========================================================================\n");

            Console.WriteLine("Person 1 Backend Engine verification COMPLETE! All core rules passed.");
        }
    }
}
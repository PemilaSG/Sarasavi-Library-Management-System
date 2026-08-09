using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Models;

namespace SarasaviLibrarySystem.Services
{
    public class CatalogService
    {
        public bool AddBookTitleWithCopies(string title, string author, string publisher, char classificationCode, int copyCount, bool refOnlyFirstCopy, out string resultMessage)
        {
            if (copyCount < 1 || copyCount > 10)
            {
                resultMessage = "Copy count must be between 1 and 10 per book registration.";
                return false;
            }

            string accessionCode = AccessionGenerator.GenerateNextAccessionCode(classificationCode);

            using var conn = LibraryDbContext.GetConnection();
            using var transaction = conn.BeginTransaction();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO BookTitles (AccessionCode, Title, Author, Publisher, ClassificationCode)
                               VALUES (@code, @title, @author, @pub, @cls);
                               SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("@code", accessionCode);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@author", author);
            cmd.Parameters.AddWithValue("@pub", publisher);
            cmd.Parameters.AddWithValue("@cls", char.ToUpper(classificationCode).ToString());

            long newTitleId = (long)(cmd.ExecuteScalar() ?? 0);

            for (int i = 1; i <= copyCount; i++)
            {
                string copyAcc = AccessionGenerator.GenerateCopyAccessionNumber(accessionCode, i);
                string copyType = (i == 1 && refOnlyFirstCopy) ? "Reference Only" : "Borrowable";

                using var copyCmd = conn.CreateCommand();
                copyCmd.CommandText = "INSERT INTO BookCopies (AccessionNumber, TitleId, CopyType, Status) VALUES (@acc, @tid, @type, 'Available');";
                copyCmd.Parameters.AddWithValue("@acc", copyAcc);
                copyCmd.Parameters.AddWithValue("@tid", newTitleId);
                copyCmd.Parameters.AddWithValue("@type", copyType);
                copyCmd.ExecuteNonQuery();
            }

            transaction.Commit();

            resultMessage = $"[BOOK REGISTERED SUCCESSFUL] Registered '{title}' under Accession Code '{accessionCode}' with {copyCount} physical copies ({accessionCode}-01 to {accessionCode}-{copyCount:D2}).";
            return true;
        }

        public List<BookInventoryItem> GetInventoryItems(string query)
        {
            var list = new List<BookInventoryItem>();
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"SELECT c.AccessionNumber, t.Title, t.Author, t.ClassificationCode, c.CopyType, c.Status
                                FROM BookCopies c
                                JOIN BookTitles t ON c.TitleId = t.TitleId
                                WHERE LOWER(c.AccessionNumber) LIKE @q 
                                   OR LOWER(t.AccessionCode) LIKE @q
                                   OR LOWER(t.Title) LIKE @q 
                                   OR LOWER(t.Author) LIKE @q;";
            cmd.Parameters.AddWithValue("@q", $"%{query.Trim().ToLower()}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string accCode = reader.GetString(0);
                string title = reader.GetString(1);
                string author = reader.GetString(2);
                string clsCode = reader.GetString(3);
                string copyType = reader.GetString(4);
                string rawStatus = reader.GetString(5);

                string category = clsCode.ToUpper() switch
                {
                    "C" => "Computing",
                    "F" => "Fiction",
                    "S" => "Science",
                    "M" => "Management",
                    _ => "General"
                };

                string displayStatus = rawStatus;
                if (copyType.Equals("Reference Only", StringComparison.OrdinalIgnoreCase) && rawStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
                {
                    displayStatus = "Reference Only";
                }

                list.Add(new BookInventoryItem
                {
                    AccessionCode = accCode,
                    Title = title,
                    Author = author,
                    Category = category,
                    Status = displayStatus
                });
            }
            return list;
        }

        public List<BookCopy> SearchCatalog(string query)
        {
            var list = new List<BookCopy>();
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"SELECT c.AccessionNumber, c.TitleId, t.Title, t.Author, c.CopyType, c.Status
                                FROM BookCopies c
                                JOIN BookTitles t ON c.TitleId = t.TitleId
                                WHERE LOWER(c.AccessionNumber) LIKE @q 
                                   OR LOWER(t.AccessionCode) LIKE @q
                                   OR LOWER(t.Title) LIKE @q 
                                   OR LOWER(t.Author) LIKE @q;";
            cmd.Parameters.AddWithValue("@q", $"%{query.Trim().ToLower()}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new BookCopy
                {
                    AccessionNumber = reader.GetString(0),
                    TitleId = reader.GetInt32(1),
                    TitleName = reader.GetString(2),
                    AuthorName = reader.GetString(3),
                    CopyType = reader.GetString(4),
                    Status = reader.GetString(5)
                });
            }
            return list;
        }

        public (int totalTitles, int totalCopies, int availableCopies, int activeLoans, int overdueCount, int totalBorrowers) GetDashboardStats()
        {
            using var conn = LibraryDbContext.GetConnection();

            int totalTitles = Convert.ToInt32(ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM BookTitles;"));
            int totalCopies = Convert.ToInt32(ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM BookCopies;"));
            int availableCopies = Convert.ToInt32(ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM BookCopies WHERE Status = 'Available';"));
            int activeLoans = Convert.ToInt32(ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM LoanRecords WHERE Status = 'Active';"));
            
            int overdueCount = Convert.ToInt32(ExecuteScalarQuery(conn, 
                "SELECT COUNT(*) FROM LoanRecords WHERE Status = 'Active' AND DateTime(DueDate) < DateTime('now');"));
            
            int totalBorrowers = Convert.ToInt32(ExecuteScalarQuery(conn, "SELECT COUNT(*) FROM Borrowers;"));

            return (totalTitles, totalCopies, availableCopies, activeLoans, overdueCount, totalBorrowers);
        }

        private object ExecuteScalarQuery(SqliteConnection conn, string query)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            return cmd.ExecuteScalar() ?? 0;
        }
    }
}

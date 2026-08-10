using System;
using System.Collections.Generic;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Models;

namespace SarasaviLibrarySystem.Services
{
    public class CatalogService
    {
        public bool AddBookTitleWithCopies(string title, string author, string publisher, char classificationCode, int copyCount, bool isFirstCopyReferenceOnly, out string resultMessage)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
            {
                resultMessage = "Title and Author are required fields.";
                return false;
            }

            if (copyCount < 1 || copyCount > 10)
            {
                resultMessage = "Copy count must be between 1 and 10.";
                return false;
            }

            string baseCode = AccessionGenerator.GenerateNextAccessionCode(classificationCode);

            using var conn = LibraryDbContext.GetConnection();
            using var tx = conn.BeginTransaction();

            try
            {
                using (var checkCmd = conn.CreateCommand())
                {
                    checkCmd.Transaction = tx;
                    checkCmd.CommandText = "SELECT COUNT(*) FROM BookTitles WHERE LOWER(Title) = LOWER(@title) AND LOWER(Author) = LOWER(@author);";
                    LibraryDbContext.AddParam(checkCmd, "@title", title);
                    LibraryDbContext.AddParam(checkCmd, "@author", author);
                    long exists = Convert.ToInt64(checkCmd.ExecuteScalar() ?? 0);
                    if (exists > 0)
                    {
                        resultMessage = $"Book '{title}' by {author} is already registered in the library.";
                        tx.Rollback();
                        return false;
                    }
                }

                int newTitleId = 1;
                using (var maxCmd = conn.CreateCommand())
                {
                    maxCmd.Transaction = tx;
                    maxCmd.CommandText = "SELECT MAX(TitleId) FROM BookTitles;";
                    object? val = maxCmd.ExecuteScalar();
                    if (val != DBNull.Value && val != null) newTitleId = Convert.ToInt32(val) + 1;
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"INSERT INTO BookTitles (TitleId, AccessionCode, Title, Author, Publisher, ClassificationCode) 
                                       VALUES (@id, @code, @title, @author, @pub, @cls);";
                    LibraryDbContext.AddParam(cmd, "@id", newTitleId);
                    LibraryDbContext.AddParam(cmd, "@code", baseCode);
                    LibraryDbContext.AddParam(cmd, "@title", title);
                    LibraryDbContext.AddParam(cmd, "@author", author);
                    LibraryDbContext.AddParam(cmd, "@pub", publisher);
                    LibraryDbContext.AddParam(cmd, "@cls", classificationCode.ToString());
                    cmd.ExecuteNonQuery();
                }

                for (int i = 1; i <= copyCount; i++)
                {
                    string copyCode = AccessionGenerator.GenerateCopyAccessionNumber(baseCode, i);
                    string copyType = (i == 1 && isFirstCopyReferenceOnly) ? "Reference Only" : "Borrowable";

                    using var copyCmd = conn.CreateCommand();
                    copyCmd.Transaction = tx;
                    copyCmd.CommandText = "INSERT INTO BookCopies (AccessionNumber, TitleId, CopyType, Status) VALUES (@acc, @tid, @type, 'Available');";
                    LibraryDbContext.AddParam(copyCmd, "@acc", copyCode);
                    LibraryDbContext.AddParam(copyCmd, "@tid", newTitleId);
                    LibraryDbContext.AddParam(copyCmd, "@type", copyType);
                    copyCmd.ExecuteNonQuery();
                }

                tx.Commit();
                resultMessage = $"Successfully registered '{title}' with Accession Code {baseCode} and {copyCount} physical copies.";
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                resultMessage = $"Database error: {ex.Message}";
                return false;
            }
        }

        public List<BookInventoryItem> GetInventoryItems(string searchQuery = "")
        {
            var list = new List<BookInventoryItem>();
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            string sql = @"
                SELECT c.AccessionNumber, t.Title, t.Author, t.ClassificationCode, c.Status, c.CopyType
                FROM BookCopies c
                JOIN BookTitles t ON c.TitleId = t.TitleId
                WHERE 1=1 ";

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " AND (c.AccessionNumber LIKE @q OR t.Title LIKE @q OR t.Author LIKE @q) ";
                LibraryDbContext.AddParam(cmd, "@q", $"%{searchQuery}%");
            }

            sql += " ORDER BY c.AccessionNumber ASC;";
            cmd.CommandText = sql;

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                string acc = r.GetValue(0)?.ToString() ?? "";
                string title = r.GetValue(1)?.ToString() ?? "";
                string author = r.GetValue(2)?.ToString() ?? "";
                string clsCode = r.GetValue(3)?.ToString() ?? "C";
                string status = r.GetValue(4)?.ToString() ?? "Available";
                string copyType = r.GetValue(5)?.ToString() ?? "Borrowable";

                string categoryName = GetCategoryName(clsCode.Length > 0 ? clsCode[0] : 'C');
                string displayStatus = (status == "Available" && copyType == "Reference Only") ? "Reference Only" : status;

                list.Add(new BookInventoryItem
                {
                    AccessionCode = acc,
                    Title = title,
                    Author = author,
                    Category = categoryName,
                    Status = displayStatus
                });
            }

            return list;
        }

        public (int totalTitles, int availableCopies, int activeLoans, int overdueCount) GetDashboardStats()
        {
            using var conn = LibraryDbContext.GetConnection();

            int totalTitles = 0;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM BookTitles;";
                totalTitles = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }

            int availableCopies = 0;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM BookCopies WHERE Status = 'Available';";
                availableCopies = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }

            int activeLoans = 0;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM LoanRecords WHERE Status = 'Active';";
                activeLoans = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }

            int overdueCount = 0;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM LoanRecords WHERE Status = 'Active' AND DueDate < @now;";
                LibraryDbContext.AddParam(cmd, "@now", DateTime.Now);
                overdueCount = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }

            return (totalTitles, availableCopies, activeLoans, overdueCount);
        }

        private string GetCategoryName(char clsCode)
        {
            return char.ToUpper(clsCode) switch
            {
                'C' => "Computing",
                'F' => "Fiction",
                'S' => "Science",
                'M' => "Management",
                _ => "General"
            };
        }
    }
}

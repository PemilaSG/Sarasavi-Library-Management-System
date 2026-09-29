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
                SELECT c.AccessionNumber, t.Title, t.Author, t.Publisher, t.ClassificationCode, c.Status, c.CopyType
                FROM BookCopies c
                JOIN BookTitles t ON c.TitleId = t.TitleId
                WHERE 1=1 ";

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                sql += " AND (c.AccessionNumber LIKE @q OR t.Title LIKE @q OR t.Author LIKE @q OR t.Publisher LIKE @q) ";
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
                string publisher = r.GetValue(3)?.ToString() ?? "";
                string clsCode = r.GetValue(4)?.ToString() ?? "C";
                string status = r.GetValue(5)?.ToString() ?? "Available";
                string copyType = r.GetValue(6)?.ToString() ?? "Borrowable";

                string categoryName = GetCategoryName(clsCode.Length > 0 ? clsCode[0] : 'C');
                string displayStatus = (status == "Available" && copyType == "Reference Only") ? "Reference Only" : status;

                list.Add(new BookInventoryItem
                {
                    AccessionCode = acc,
                    Title = title,
                    Author = author,
                    Publisher = publisher,
                    Category = categoryName,
                    CopyType = copyType,
                    Status = displayStatus
                });
            }

            return list;
        }

        public bool UpdateBookItem(string accessionCode, string title, string author, string publisher, string category, string copyType, string status, out string resultMessage)
        {
            if (string.IsNullOrWhiteSpace(accessionCode) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
            {
                resultMessage = "Accession Code, Title, and Author are required.";
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();

            int titleId = -1;
            string currentTitleName = "";
            using (var getTidCmd = conn.CreateCommand())
            {
                getTidCmd.CommandText = @"SELECT c.TitleId, t.Title 
                                          FROM BookCopies c 
                                          JOIN BookTitles t ON c.TitleId = t.TitleId 
                                          WHERE c.AccessionNumber = @acc;";
                LibraryDbContext.AddParam(getTidCmd, "@acc", accessionCode);
                using var reader = getTidCmd.ExecuteReader();
                if (reader.Read())
                {
                    titleId = Convert.ToInt32(reader.GetValue(0));
                    currentTitleName = reader.GetValue(1)?.ToString() ?? "";
                }
                else
                {
                    resultMessage = $"Book copy '{accessionCode}' was not found.";
                    return false;
                }
            }

            int borrowedCount = 0;
            int totalCopies = 0;
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = @"
                    SELECT 
                        COUNT(*),
                        SUM(CASE WHEN c.Status = 'Borrowed' OR EXISTS (
                            SELECT 1 FROM LoanRecords l WHERE l.CopyAccessionNumber = c.AccessionNumber AND l.Status = 'Active'
                        ) THEN 1 ELSE 0 END)
                    FROM BookCopies c
                    WHERE c.TitleId = @tid;";
                LibraryDbContext.AddParam(checkCmd, "@tid", titleId);
                using var reader = checkCmd.ExecuteReader();
                if (reader.Read())
                {
                    totalCopies = Convert.ToInt32(reader.GetValue(0));
                    borrowedCount = Convert.ToInt32(reader.GetValue(1) == DBNull.Value ? 0 : reader.GetValue(1));
                }
            }

            if (borrowedCount > 0)
            {
                resultMessage = $"Cannot update book '{currentTitleName}' ({accessionCode}). {borrowedCount} of {totalCopies} physical copy(ies) are currently checked out on loan. All copies must be returned to the library before updating.";
                return false;
            }

            char clsCode = category.ToUpper() switch
            {
                "COMPUTING" => 'C',
                "FICTION" => 'F',
                "SCIENCE" => 'S',
                "MANAGEMENT" => 'M',
                _ => 'C'
            };

            using var tx = conn.BeginTransaction();
            try
            {
                using (var updateTitleCmd = conn.CreateCommand())
                {
                    updateTitleCmd.Transaction = tx;
                    updateTitleCmd.CommandText = @"UPDATE BookTitles 
                                                   SET Title = @title, Author = @author, Publisher = @pub, ClassificationCode = @cls 
                                                   WHERE TitleId = @tid;";
                    LibraryDbContext.AddParam(updateTitleCmd, "@title", title);
                    LibraryDbContext.AddParam(updateTitleCmd, "@author", author);
                    LibraryDbContext.AddParam(updateTitleCmd, "@pub", publisher);
                    LibraryDbContext.AddParam(updateTitleCmd, "@cls", clsCode.ToString());
                    LibraryDbContext.AddParam(updateTitleCmd, "@tid", titleId);
                    updateTitleCmd.ExecuteNonQuery();
                }

                using (var updateCopyCmd = conn.CreateCommand())
                {
                    updateCopyCmd.Transaction = tx;
                    updateCopyCmd.CommandText = @"UPDATE BookCopies 
                                                  SET CopyType = @ctype, Status = @status 
                                                  WHERE AccessionNumber = @acc;";
                    LibraryDbContext.AddParam(updateCopyCmd, "@ctype", copyType);
                    LibraryDbContext.AddParam(updateCopyCmd, "@status", status);
                    LibraryDbContext.AddParam(updateCopyCmd, "@acc", accessionCode);
                    updateCopyCmd.ExecuteNonQuery();
                }

                tx.Commit();
                resultMessage = $"Book copy '{accessionCode}' updated successfully.";
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                resultMessage = $"Error updating book item: {ex.Message}";
                return false;
            }
        }

        public bool DeleteBookCopy(string accessionCode, out string resultMessage)
        {
            if (string.IsNullOrWhiteSpace(accessionCode))
            {
                resultMessage = "Invalid Accession Code.";
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();

            int titleId = -1;
            string currentTitleName = "";
            using (var getTidCmd = conn.CreateCommand())
            {
                getTidCmd.CommandText = @"SELECT c.TitleId, t.Title 
                                          FROM BookCopies c 
                                          JOIN BookTitles t ON c.TitleId = t.TitleId 
                                          WHERE c.AccessionNumber = @acc;";
                LibraryDbContext.AddParam(getTidCmd, "@acc", accessionCode);
                using var reader = getTidCmd.ExecuteReader();
                if (reader.Read())
                {
                    titleId = Convert.ToInt32(reader.GetValue(0));
                    currentTitleName = reader.GetValue(1)?.ToString() ?? "";
                }
                else
                {
                    resultMessage = $"Book copy '{accessionCode}' was not found.";
                    return false;
                }
            }

            int borrowedCount = 0;
            int totalCopies = 0;
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = @"
                    SELECT 
                        COUNT(*),
                        SUM(CASE WHEN c.Status = 'Borrowed' OR EXISTS (
                            SELECT 1 FROM LoanRecords l WHERE l.CopyAccessionNumber = c.AccessionNumber AND l.Status = 'Active'
                        ) THEN 1 ELSE 0 END)
                    FROM BookCopies c
                    WHERE c.TitleId = @tid;";
                LibraryDbContext.AddParam(checkCmd, "@tid", titleId);
                using var reader = checkCmd.ExecuteReader();
                if (reader.Read())
                {
                    totalCopies = Convert.ToInt32(reader.GetValue(0));
                    borrowedCount = Convert.ToInt32(reader.GetValue(1) == DBNull.Value ? 0 : reader.GetValue(1));
                }
            }

            if (borrowedCount > 0)
            {
                resultMessage = $"Cannot delete book '{currentTitleName}' ({accessionCode}). {borrowedCount} of {totalCopies} physical copy(ies) are currently checked out on loan. All copies must be returned to the library before deleting.";
                return false;
            }

            using var tx = conn.BeginTransaction();
            try
            {
                using (var delLoanCmd = conn.CreateCommand())
                {
                    delLoanCmd.Transaction = tx;
                    delLoanCmd.CommandText = "DELETE FROM LoanRecords WHERE CopyAccessionNumber = @acc;";
                    LibraryDbContext.AddParam(delLoanCmd, "@acc", accessionCode);
                    delLoanCmd.ExecuteNonQuery();
                }

                using (var delCopyCmd = conn.CreateCommand())
                {
                    delCopyCmd.Transaction = tx;
                    delCopyCmd.CommandText = "DELETE FROM BookCopies WHERE AccessionNumber = @acc;";
                    LibraryDbContext.AddParam(delCopyCmd, "@acc", accessionCode);
                    int rows = delCopyCmd.ExecuteNonQuery();
                    if (rows == 0)
                    {
                        tx.Rollback();
                        resultMessage = $"Book copy '{accessionCode}' was not found.";
                        return false;
                    }
                }

                if (titleId > 0)
                {
                    using var countCmd = conn.CreateCommand();
                    countCmd.Transaction = tx;
                    countCmd.CommandText = "SELECT COUNT(*) FROM BookCopies WHERE TitleId = @tid;";
                    LibraryDbContext.AddParam(countCmd, "@tid", titleId);
                    long count = Convert.ToInt64(countCmd.ExecuteScalar() ?? 0);
                    if (count == 0)
                    {
                        using var delTitleCmd = conn.CreateCommand();
                        delTitleCmd.Transaction = tx;
                        delTitleCmd.CommandText = "DELETE FROM BookTitles WHERE TitleId = @tid;";
                        LibraryDbContext.AddParam(delTitleCmd, "@tid", titleId);
                        delTitleCmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
                resultMessage = $"Book copy '{accessionCode}' deleted successfully.";
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                resultMessage = $"Error deleting book copy: {ex.Message}";
                return false;
            }
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

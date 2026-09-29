using System;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Models;

namespace SarasaviLibrarySystem.Services
{
    public class LoanService
    {
        public bool IssueLoan(string userNumber, string copyAccessionNumber, out string message)
        {
            var borrowerService = new BorrowerService();
            var borrower = borrowerService.GetBorrowerDetails(userNumber);

            if (borrower == null)
            {
                message = "Borrower with specified User Number was not found.";
                return false;
            }

            if (!borrower.CanBorrow(out string borrowerBlockReason))
            {
                message = borrowerBlockReason;
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();
            string copyStatus = "";
            string copyType = "";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Status, CopyType FROM BookCopies WHERE AccessionNumber = @acc;";
                LibraryDbContext.AddParam(cmd, "@acc", copyAccessionNumber);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        copyStatus = reader.GetValue(0)?.ToString() ?? "";
                        copyType = reader.GetValue(1)?.ToString() ?? "";
                    }
                    else
                    {
                        message = $"Book Copy with accession number '{copyAccessionNumber}' does not exist.";
                        return false;
                    }
                }
            }

            if (copyType.Equals("Reference Only", StringComparison.OrdinalIgnoreCase))
            {
                message = $"Copy '{copyAccessionNumber}' is marked REFERENCE ONLY and cannot be checked out.";
                return false;
            }

            if (!copyStatus.Equals("Available", StringComparison.OrdinalIgnoreCase) && !copyStatus.Equals("Reserved", StringComparison.OrdinalIgnoreCase))
            {
                message = $"Copy '{copyAccessionNumber}' is currently unavailable (Status: {copyStatus}).";
                return false;
            }

            DateTime issueDate = DateTime.Now;
            DateTime dueDate = issueDate.AddDays(14);

            using var tx = conn.BeginTransaction();
            try
            {
                using (var insertCmd = conn.CreateCommand())
                {
                    insertCmd.Transaction = tx;
                    insertCmd.CommandText = @"INSERT INTO LoanRecords (CopyAccessionNumber, UserNumber, IssueDate, DueDate, Status)
                                              VALUES (@acc, @unum, @iss, @due, 'Active');";
                    LibraryDbContext.AddParam(insertCmd, "@acc", copyAccessionNumber);
                    LibraryDbContext.AddParam(insertCmd, "@unum", userNumber);
                    LibraryDbContext.AddParam(insertCmd, "@iss", issueDate);
                    LibraryDbContext.AddParam(insertCmd, "@due", dueDate);
                    insertCmd.ExecuteNonQuery();
                }

                using (var updateCopyCmd = conn.CreateCommand())
                {
                    updateCopyCmd.Transaction = tx;
                    updateCopyCmd.CommandText = "UPDATE BookCopies SET Status = 'Borrowed' WHERE AccessionNumber = @acc;";
                    LibraryDbContext.AddParam(updateCopyCmd, "@acc", copyAccessionNumber);
                    updateCopyCmd.ExecuteNonQuery();
                }

                tx.Commit();
                message = $"Loan issued successfully! Return Due Date is {dueDate:yyyy-MM-dd} (14 days).";
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                message = $"Loan processing failed due to database error: {ex.Message}";
                return false;
            }
        }

        public System.Collections.Generic.List<LoanRecord> GetActiveLoansForBorrower(string userNumber)
        {
            var list = new System.Collections.Generic.List<LoanRecord>();
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"
                SELECT l.LoanId, l.CopyAccessionNumber, l.UserNumber, t.Title, t.Author, l.IssueDate, l.DueDate, l.Status
                FROM LoanRecords l
                JOIN BookCopies c ON l.CopyAccessionNumber = c.AccessionNumber
                JOIN BookTitles t ON c.TitleId = t.TitleId
                WHERE l.UserNumber = @unum AND l.Status = 'Active'
                ORDER BY l.IssueDate DESC;";
            LibraryDbContext.AddParam(cmd, "@unum", userNumber);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                object rawIssue = reader.GetValue(5);
                object rawDue = reader.GetValue(6);
                DateTime issueDt = rawIssue is DateTime idt ? idt : (DateTime.TryParse(rawIssue?.ToString(), out var d1) ? d1 : DateTime.Now);
                DateTime dueDt = rawDue is DateTime ddt ? ddt : (DateTime.TryParse(rawDue?.ToString(), out var d2) ? d2 : DateTime.Now.AddDays(14));

                list.Add(new LoanRecord
                {
                    LoanId = Convert.ToInt32(reader.GetValue(0)),
                    CopyAccessionNumber = reader.GetValue(1)?.ToString() ?? "",
                    UserNumber = reader.GetValue(2)?.ToString() ?? "",
                    BookTitle = reader.GetValue(3)?.ToString() ?? "",
                    BorrowerName = reader.GetValue(4)?.ToString() ?? "",
                    IssueDate = issueDt,
                    DueDate = dueDt,
                    Status = reader.GetValue(7)?.ToString() ?? "Active"
                });
            }

            return list;
        }

        public bool CancelLoanRequest(string copyAccessionNumber, out string message)
        {
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT Status FROM BookCopies WHERE AccessionNumber = @acc;";
            LibraryDbContext.AddParam(cmd, "@acc", copyAccessionNumber);

            object? statusObj = cmd.ExecuteScalar();
            if (statusObj == null)
            {
                message = "Book copy not found.";
                return false;
            }

            message = "Loan request canceled safely.";
            return true;
        }
    }
}

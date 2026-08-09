using System;
using Microsoft.Data.Sqlite;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Models;

namespace SarasaviLibrarySystem.Services
{
    public class LoanService
    {
        public bool IssueLoan(string userNumber, string copyAccessionNumber, out string resultMessage)
        {
            using var conn = LibraryDbContext.GetConnection();

            var borrowerService = new BorrowerService();
            var borrower = borrowerService.GetBorrowerDetails(userNumber);

            if (borrower == null)
            {
                resultMessage = $"[CHECKOUT FAILED] Borrower '{userNumber}' does not exist in the system.";
                return false;
            }

            if (!borrower.CanBorrow(out string ineligibleReason))
            {
                resultMessage = $"[CHECKOUT BLOCKED] {ineligibleReason}";
                return false;
            }

            using var copyCmd = conn.CreateCommand();
            copyCmd.CommandText = @"SELECT c.AccessionNumber, c.CopyType, c.Status, t.Title 
                                   FROM BookCopies c
                                   JOIN BookTitles t ON c.TitleId = t.TitleId
                                   WHERE c.AccessionNumber = @acc;";
            copyCmd.Parameters.AddWithValue("@acc", copyAccessionNumber);

            using var reader = copyCmd.ExecuteReader();
            if (!reader.Read())
            {
                resultMessage = $"[CHECKOUT FAILED] Book Copy '{copyAccessionNumber}' not found.";
                return false;
            }

            string copyType = reader.GetString(1);
            string copyStatus = reader.GetString(2);
            string bookTitle = reader.GetString(3);

            if (copyType.Equals("Reference Only", StringComparison.OrdinalIgnoreCase))
            {
                resultMessage = $"[CHECKOUT BLOCKED] Copy '{copyAccessionNumber}' ({bookTitle}) is marked REFERENCE ONLY and cannot be issued for loan.";
                return false;
            }

            if (!copyStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
            {
                resultMessage = $"[CHECKOUT BLOCKED] Copy '{copyAccessionNumber}' is currently '{copyStatus}' and unavailable for checkout.";
                return false;
            }

            DateTime issueDate = DateTime.Now;
            DateTime dueDate = issueDate.AddDays(14);

            using var transaction = conn.BeginTransaction();

            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO LoanRecords (CopyAccessionNumber, UserNumber, IssueDate, DueDate, Status)
                                      VALUES (@acc, @user, @iss, @due, 'Active');";
            insertCmd.Parameters.AddWithValue("@acc", copyAccessionNumber);
            insertCmd.Parameters.AddWithValue("@user", userNumber);
            insertCmd.Parameters.AddWithValue("@iss", issueDate.ToString("o"));
            insertCmd.Parameters.AddWithValue("@due", dueDate.ToString("o"));
            insertCmd.ExecuteNonQuery();

            using var updateCopyCmd = conn.CreateCommand();
            updateCopyCmd.CommandText = "UPDATE BookCopies SET Status = 'Borrowed' WHERE AccessionNumber = @acc;";
            updateCopyCmd.Parameters.AddWithValue("@acc", copyAccessionNumber);
            updateCopyCmd.ExecuteNonQuery();

            transaction.Commit();

            resultMessage = $"[CHECKOUT SUCCESSFUL] Issued '{bookTitle}' ({copyAccessionNumber}) to {borrower.Name} ({userNumber}). Due Date: {dueDate:yyyy-MM-dd} (14 Days).";
            return true;
        }

        public bool CancelLoanRequest(string copyAccessionNumber, out string message)
        {
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE BookCopies SET Status = 'Available' WHERE AccessionNumber = @acc AND Status = 'Borrowed';";
            cmd.Parameters.AddWithValue("@acc", copyAccessionNumber);
            int rows = cmd.ExecuteNonQuery();

            if (rows > 0)
            {
                message = $"Loan request for copy {copyAccessionNumber} cancelled by librarian.";
                return true;
            }
            message = "Loan request cancellation failed or copy was not checked out.";
            return false;
        }
    }
}

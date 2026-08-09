using System;
using Microsoft.Data.Sqlite;
using SarasaviLibrarySystem.Data;

namespace SarasaviLibrarySystem.Services
{
    public class ReturnService
    {
        public bool ProcessReturn(string copyAccessionNumber, out string resultMessage, out string? reservationAlertNotification)
        {
            reservationAlertNotification = null;
            using var conn = LibraryDbContext.GetConnection();

            using var findCmd = conn.CreateCommand();
            findCmd.CommandText = @"SELECT l.LoanId, l.UserNumber, c.TitleId, t.Title
                                    FROM LoanRecords l
                                    JOIN BookCopies c ON l.CopyAccessionNumber = c.AccessionNumber
                                    JOIN BookTitles t ON c.TitleId = t.TitleId
                                    WHERE l.CopyAccessionNumber = @acc AND l.Status = 'Active';";
            findCmd.Parameters.AddWithValue("@acc", copyAccessionNumber);

            int loanId = 0;
            string userNumber = string.Empty;
            int titleId = 0;
            string bookTitle = string.Empty;

            using (var reader = findCmd.ExecuteReader())
            {
                if (!reader.Read())
                {
                    resultMessage = $"[RETURN FAILED] Copy '{copyAccessionNumber}' has no active checkout record.";
                    return false;
                }
                loanId = reader.GetInt32(0);
                userNumber = reader.GetString(1);
                titleId = reader.GetInt32(2);
                bookTitle = reader.GetString(3);
            }

            using var transaction = conn.BeginTransaction();

            using var updateLoanCmd = conn.CreateCommand();
            updateLoanCmd.CommandText = @"UPDATE LoanRecords 
                                          SET Status = 'Returned', ReturnDate = @ret 
                                          WHERE LoanId = @lid;";
            updateLoanCmd.Parameters.AddWithValue("@ret", DateTime.Now.ToString("o"));
            updateLoanCmd.Parameters.AddWithValue("@lid", loanId);
            updateLoanCmd.ExecuteNonQuery();

            using var resCmd = conn.CreateCommand();
            resCmd.CommandText = @"SELECT r.ReservationId, r.UserNumber, b.Name, r.RequestDate 
                                   FROM ReservationRecords r
                                   JOIN Borrowers b ON r.UserNumber = b.UserNumber
                                   WHERE r.TitleId = @tid AND r.Status = 'Pending'
                                   ORDER BY r.RequestDate ASC LIMIT 1;";
            resCmd.Parameters.AddWithValue("@tid", titleId);

            int resId = 0;
            string resUserNumber = string.Empty;
            string resUserName = string.Empty;
            string resRequestDate = string.Empty;
            bool hasReservation = false;

            using (var resReader = resCmd.ExecuteReader())
            {
                if (resReader.Read())
                {
                    resId = resReader.GetInt32(0);
                    resUserNumber = resReader.GetString(1);
                    resUserName = resReader.GetString(2);
                    resRequestDate = resReader.GetString(3);
                    hasReservation = true;
                }
            }

            if (hasReservation)
            {
                using var updateCopyCmd = conn.CreateCommand();
                updateCopyCmd.CommandText = "UPDATE BookCopies SET Status = 'Reserved' WHERE AccessionNumber = @acc;";
                updateCopyCmd.Parameters.AddWithValue("@acc", copyAccessionNumber);
                updateCopyCmd.ExecuteNonQuery();

                using var updateResCmd = conn.CreateCommand();
                updateResCmd.CommandText = "UPDATE ReservationRecords SET Status = 'Fulfilled' WHERE ReservationId = @rid;";
                updateResCmd.Parameters.AddWithValue("@rid", resId);
                updateResCmd.ExecuteNonQuery();

                reservationAlertNotification = $"🔔 [RESERVATION SET-ASIDE ALERT]\n" +
                                               $"Book Title: '{bookTitle}'\n" +
                                               $"Copy Returned: {copyAccessionNumber}\n\n" +
                                               $"⚠️ THIS TITLE HAS AN ACTIVE RESERVATION!\n" +
                                               $"Please SET ASIDE this book for Member: {resUserName} ({resUserNumber})\n" +
                                               $"Reservation Request Date: {resRequestDate}";
            }
            else
            {
                using var updateCopyCmd = conn.CreateCommand();
                updateCopyCmd.CommandText = "UPDATE BookCopies SET Status = 'Available' WHERE AccessionNumber = @acc;";
                updateCopyCmd.Parameters.AddWithValue("@acc", copyAccessionNumber);
                updateCopyCmd.ExecuteNonQuery();
            }

            transaction.Commit();

            resultMessage = $"[RETURN SUCCESSFUL] Copy '{copyAccessionNumber}' ({bookTitle}) returned successfully by {userNumber}.";
            return true;
        }
    }
}

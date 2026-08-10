using System;
using SarasaviLibrarySystem.Data;

namespace SarasaviLibrarySystem.Services
{
    public class ReservationService
    {
        public bool ReserveTitle(int titleId, string userNumber, out string resultMessage)
        {
            var borrowerService = new BorrowerService();
            var borrower = borrowerService.GetBorrowerDetails(userNumber);

            if (borrower == null)
            {
                resultMessage = "Borrower with specified User Number was not found.";
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();
            using var checkCmd = conn.CreateCommand();

            checkCmd.CommandText = "SELECT COUNT(*) FROM ReservationRecords WHERE TitleId = @tid AND UserNumber = @unum AND Status = 'Pending';";
            LibraryDbContext.AddParam(checkCmd, "@tid", titleId);
            LibraryDbContext.AddParam(checkCmd, "@unum", userNumber);

            long existingCount = Convert.ToInt64(checkCmd.ExecuteScalar() ?? 0);
            if (existingCount > 0)
            {
                resultMessage = $"Borrower {userNumber} already has an active pending reservation for this title.";
                return false;
            }

            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO ReservationRecords (TitleId, UserNumber, RequestDate, Status)
                                      VALUES (@tid, @unum, @req, 'Pending');";
            LibraryDbContext.AddParam(insertCmd, "@tid", titleId);
            LibraryDbContext.AddParam(insertCmd, "@unum", userNumber);
            LibraryDbContext.AddParam(insertCmd, "@req", DateTime.Now.ToString("o"));

            int rows = insertCmd.ExecuteNonQuery();
            if (rows > 0)
            {
                resultMessage = $"Reservation placed successfully for Borrower {userNumber}. You will be notified when a copy is returned.";
                return true;
            }
            else
            {
                resultMessage = "Failed to create reservation record.";
                return false;
            }
        }

        public bool CancelReservation(long reservationId, out string resultMessage)
        {
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "UPDATE ReservationRecords SET Status = 'Cancelled' WHERE ReservationId = @rid;";
            LibraryDbContext.AddParam(cmd, "@rid", reservationId);

            int rows = cmd.ExecuteNonQuery();
            if (rows > 0)
            {
                resultMessage = "Reservation cancelled successfully.";
                return true;
            }
            else
            {
                resultMessage = "Reservation record not found or already processed.";
                return false;
            }
        }
    }
}

using System;
using SarasaviLibrarySystem.Data;

namespace SarasaviLibrarySystem.Services
{
    public class ReservationService
    {
        public bool ReserveCopy(string accessionNumber, string userNumber, out string resultMessage)
        {
            var borrowerService = new BorrowerService();
            var borrower = borrowerService.GetBorrowerDetails(userNumber);

            if (borrower == null)
            {
                resultMessage = "Borrower with specified User Number was not found.";
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();
            int titleId = -1;
            string copyStatus = "";
            string copyType = "";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT TitleId, Status, CopyType FROM BookCopies WHERE AccessionNumber = @acc;";
                LibraryDbContext.AddParam(cmd, "@acc", accessionNumber);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    titleId = Convert.ToInt32(reader.GetValue(0));
                    copyStatus = reader.GetValue(1)?.ToString() ?? "";
                    copyType = reader.GetValue(2)?.ToString() ?? "";
                }
                else
                {
                    resultMessage = $"Book copy with accession code '{accessionNumber}' was not found.";
                    return false;
                }
            }

            if (copyType.Equals("Reference Only", StringComparison.OrdinalIgnoreCase))
            {
                resultMessage = $"Copy '{accessionNumber}' is Reference Only and cannot be reserved.";
                return false;
            }

            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "SELECT COUNT(*) FROM ReservationRecords WHERE TitleId = @tid AND UserNumber = @unum AND Status = 'Pending';";
                LibraryDbContext.AddParam(checkCmd, "@tid", titleId);
                LibraryDbContext.AddParam(checkCmd, "@unum", userNumber);
                long existingCount = Convert.ToInt64(checkCmd.ExecuteScalar() ?? 0);
                if (existingCount > 0)
                {
                    resultMessage = $"Borrower {userNumber} already has an active pending reservation for this title.";
                    return false;
                }
            }

            using var tx = conn.BeginTransaction();
            try
            {
                using (var insertCmd = conn.CreateCommand())
                {
                    insertCmd.Transaction = tx;
                    insertCmd.CommandText = @"INSERT INTO ReservationRecords (TitleId, UserNumber, RequestDate, Status)
                                              VALUES (@tid, @unum, @req, 'Pending');";
                    LibraryDbContext.AddParam(insertCmd, "@tid", titleId);
                    LibraryDbContext.AddParam(insertCmd, "@unum", userNumber);
                    LibraryDbContext.AddParam(insertCmd, "@req", DateTime.Now.ToString("o"));
                    insertCmd.ExecuteNonQuery();
                }

                if (copyStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
                {
                    using var updateCmd = conn.CreateCommand();
                    updateCmd.Transaction = tx;
                    updateCmd.CommandText = "UPDATE BookCopies SET Status = 'Reserved' WHERE AccessionNumber = @acc;";
                    LibraryDbContext.AddParam(updateCmd, "@acc", accessionNumber);
                    updateCmd.ExecuteNonQuery();
                }
                else
                {
                    using var findAvailCmd = conn.CreateCommand();
                    findAvailCmd.Transaction = tx;
                    findAvailCmd.CommandText = "SELECT AccessionNumber FROM BookCopies WHERE TitleId = @tid AND Status = 'Available' LIMIT 1;";
                    LibraryDbContext.AddParam(findAvailCmd, "@tid", titleId);
                    object? availAccObj = findAvailCmd.ExecuteScalar();
                    if (availAccObj != null && availAccObj != DBNull.Value)
                    {
                        string availAcc = availAccObj.ToString()!;
                        using var updateAvailCmd = conn.CreateCommand();
                        updateAvailCmd.Transaction = tx;
                        updateAvailCmd.CommandText = "UPDATE BookCopies SET Status = 'Reserved' WHERE AccessionNumber = @acc;";
                        LibraryDbContext.AddParam(updateAvailCmd, "@acc", availAcc);
                        updateAvailCmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
                resultMessage = $"Reservation placed successfully for Borrower {userNumber}. Copy status updated to Reserved.";
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                resultMessage = $"Failed to place reservation: {ex.Message}";
                return false;
            }
        }

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

            using var tx = conn.BeginTransaction();
            try
            {
                using (var insertCmd = conn.CreateCommand())
                {
                    insertCmd.Transaction = tx;
                    insertCmd.CommandText = @"INSERT INTO ReservationRecords (TitleId, UserNumber, RequestDate, Status)
                                              VALUES (@tid, @unum, @req, 'Pending');";
                    LibraryDbContext.AddParam(insertCmd, "@tid", titleId);
                    LibraryDbContext.AddParam(insertCmd, "@unum", userNumber);
                    LibraryDbContext.AddParam(insertCmd, "@req", DateTime.Now.ToString("o"));
                    insertCmd.ExecuteNonQuery();
                }

                using (var findAvailCmd = conn.CreateCommand())
                {
                    findAvailCmd.Transaction = tx;
                    findAvailCmd.CommandText = "SELECT AccessionNumber FROM BookCopies WHERE TitleId = @tid AND Status = 'Available' LIMIT 1;";
                    LibraryDbContext.AddParam(findAvailCmd, "@tid", titleId);
                    object? availAccObj = findAvailCmd.ExecuteScalar();
                    if (availAccObj != null && availAccObj != DBNull.Value)
                    {
                        string availAcc = availAccObj.ToString()!;
                        using var updateAvailCmd = conn.CreateCommand();
                        updateAvailCmd.Transaction = tx;
                        updateAvailCmd.CommandText = "UPDATE BookCopies SET Status = 'Reserved' WHERE AccessionNumber = @acc;";
                        LibraryDbContext.AddParam(updateAvailCmd, "@acc", availAcc);
                        updateAvailCmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
                resultMessage = $"Reservation placed successfully for Borrower {userNumber}. Book status updated to Reserved.";
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                resultMessage = $"Failed to place reservation: {ex.Message}";
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

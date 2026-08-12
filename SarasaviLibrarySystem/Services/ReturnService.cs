using System;
using SarasaviLibrarySystem.Data;

namespace SarasaviLibrarySystem.Services
{
    public class ReturnService
    {
        public bool ProcessReturn(string copyAccessionNumber, out string resultMessage, out bool isReservedSetAsideNeeded, out string reservationUserNumber)
        {
            isReservedSetAsideNeeded = false;
            reservationUserNumber = string.Empty;

            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = @"SELECT LoanId, UserNumber, IssueDate, DueDate, Status 
                               FROM LoanRecords 
                               WHERE CopyAccessionNumber = @acc AND Status = 'Active';";
            LibraryDbContext.AddParam(cmd, "@acc", copyAccessionNumber);

            long loanId = -1;
            string borrowerUserNumber = "";
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    loanId = Convert.ToInt64(reader.GetValue(0));
                    borrowerUserNumber = reader.GetValue(1)?.ToString() ?? "";
                }
                else
                {
                    resultMessage = $"No active loan record found for accession code '{copyAccessionNumber}'.";
                    return false;
                }
            }

            int titleId = -1;
            using (var titleCmd = conn.CreateCommand())
            {
                titleCmd.CommandText = "SELECT TitleId FROM BookCopies WHERE AccessionNumber = @acc;";
                LibraryDbContext.AddParam(titleCmd, "@acc", copyAccessionNumber);
                object? tidObj = titleCmd.ExecuteScalar();
                if (tidObj != null) titleId = Convert.ToInt32(tidObj);
            }

            using var tx = conn.BeginTransaction();
            try
            {
                DateTime returnDate = DateTime.Now;
                using (var updateLoanCmd = conn.CreateCommand())
                {
                    updateLoanCmd.Transaction = tx;
                    updateLoanCmd.CommandText = @"UPDATE LoanRecords 
                                                  SET Status = 'Returned', ReturnDate = @ret 
                                                  WHERE LoanId = @lid;";
                    LibraryDbContext.AddParam(updateLoanCmd, "@ret", returnDate);
                    LibraryDbContext.AddParam(updateLoanCmd, "@lid", loanId);
                    updateLoanCmd.ExecuteNonQuery();
                }

                long pendingResId = -1;
                string pendingUserNum = "";
                if (titleId > 0)
                {
                    using (var resCmd = conn.CreateCommand())
                    {
                        resCmd.Transaction = tx;
                        resCmd.CommandText = @"SELECT ReservationId, UserNumber 
                                               FROM ReservationRecords 
                                               WHERE TitleId = @tid AND Status = 'Pending' 
                                               ORDER BY RequestDate ASC;";
                        LibraryDbContext.AddParam(resCmd, "@tid", titleId);

                        using (var resReader = resCmd.ExecuteReader())
                        {
                            if (resReader.Read())
                            {
                                pendingResId = Convert.ToInt64(resReader.GetValue(0));
                                pendingUserNum = resReader.GetValue(1)?.ToString() ?? "";
                            }
                        }
                    }
                }

                if (pendingResId > 0)
                {
                    isReservedSetAsideNeeded = true;
                    reservationUserNumber = pendingUserNum;

                    using var setCopyReservedCmd = conn.CreateCommand();
                    setCopyReservedCmd.Transaction = tx;
                    setCopyReservedCmd.CommandText = "UPDATE BookCopies SET Status = 'Reserved' WHERE AccessionNumber = @acc;";
                    LibraryDbContext.AddParam(setCopyReservedCmd, "@acc", copyAccessionNumber);
                    setCopyReservedCmd.ExecuteNonQuery();

                    using var fulfillResCmd = conn.CreateCommand();
                    fulfillResCmd.Transaction = tx;
                    fulfillResCmd.CommandText = "UPDATE ReservationRecords SET Status = 'Fulfilled' WHERE ReservationId = @rid;";
                    LibraryDbContext.AddParam(fulfillResCmd, "@rid", pendingResId);
                    fulfillResCmd.ExecuteNonQuery();

                    resultMessage = $"Return processed successfully for '{copyAccessionNumber}'. ATTENTION: Book is RESERVED for member {pendingUserNum}. Put copy aside on reservation shelf.";
                }
                else
                {
                    using var setCopyAvailableCmd = conn.CreateCommand();
                    setCopyAvailableCmd.Transaction = tx;
                    setCopyAvailableCmd.CommandText = "UPDATE BookCopies SET Status = 'Available' WHERE AccessionNumber = @acc;";
                    LibraryDbContext.AddParam(setCopyAvailableCmd, "@acc", copyAccessionNumber);
                    setCopyAvailableCmd.ExecuteNonQuery();

                    resultMessage = $"Return processed successfully for '{copyAccessionNumber}'. Copy is now available on main shelf.";
                }

                tx.Commit();
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                resultMessage = $"Return processing failed due to database error: {ex.Message}";
                return false;
            }
        }
    }
}

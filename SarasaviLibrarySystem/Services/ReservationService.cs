using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Models;

namespace SarasaviLibrarySystem.Services
{
    public class ReservationService
    {
        public bool CreateReservation(int titleId, string userNumber, out string message)
        {
            using var conn = LibraryDbContext.GetConnection();

            var borrowerService = new BorrowerService();
            var borrower = borrowerService.GetBorrowerDetails(userNumber);
            if (borrower == null)
            {
                message = $"Borrower '{userNumber}' not found.";
                return false;
            }

            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM ReservationRecords WHERE TitleId = @tid AND UserNumber = @user AND Status = 'Pending';";
            checkCmd.Parameters.AddWithValue("@tid", titleId);
            checkCmd.Parameters.AddWithValue("@user", userNumber);
            long existing = (long)(checkCmd.ExecuteScalar() ?? 0);
            if (existing > 0)
            {
                message = $"Member {borrower.Name} ({userNumber}) already has an active pending reservation for this book title.";
                return false;
            }

            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO ReservationRecords (TitleId, UserNumber, RequestDate, Status)
                                      VALUES (@tid, @user, @req, 'Pending');";
            insertCmd.Parameters.AddWithValue("@tid", titleId);
            insertCmd.Parameters.AddWithValue("@user", userNumber);
            insertCmd.Parameters.AddWithValue("@req", DateTime.Now.ToString("o"));
            insertCmd.ExecuteNonQuery();

            message = $"Reservation successfully recorded for {borrower.Name} ({userNumber}).";
            return true;
        }

        public List<ReservationRecord> GetPendingReservations(int titleId)
        {
            var list = new List<ReservationRecord>();
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT r.ReservationId, r.TitleId, t.Title, r.UserNumber, b.Name, r.RequestDate, r.Status
                                FROM ReservationRecords r
                                JOIN BookTitles t ON r.TitleId = t.TitleId
                                JOIN Borrowers b ON r.UserNumber = b.UserNumber
                                WHERE r.TitleId = @tid AND r.Status = 'Pending'
                                ORDER BY r.RequestDate ASC;";
            cmd.Parameters.AddWithValue("@tid", titleId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ReservationRecord
                {
                    ReservationId = reader.GetInt32(0),
                    TitleId = reader.GetInt32(1),
                    BookTitle = reader.GetString(2),
                    UserNumber = reader.GetString(3),
                    BorrowerName = reader.GetString(4),
                    RequestDate = DateTime.Parse(reader.GetString(5)),
                    Status = reader.GetString(6)
                });
            }
            return list;
        }
    }
}

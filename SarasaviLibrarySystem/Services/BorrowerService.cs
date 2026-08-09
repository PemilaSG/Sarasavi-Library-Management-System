using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Models;

namespace SarasaviLibrarySystem.Services
{
    public class BorrowerService
    {
        public bool RegisterBorrower(string name, string sex, string nic, string address, out string resultMessage, out string userNumber)
        {
            userNumber = string.Empty;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(nic))
            {
                resultMessage = "Borrower Name and NIC Number are required.";
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();

            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT UserNumber FROM Borrowers WHERE NIC = @nic;";
            checkCmd.Parameters.AddWithValue("@nic", nic.Trim());
            var existingUser = checkCmd.ExecuteScalar();
            if (existingUser != null)
            {
                resultMessage = $"Borrower with NIC '{nic}' is already registered as '{existingUser}'.";
                return false;
            }

            using var maxCmd = conn.CreateCommand();
            maxCmd.CommandText = "SELECT UserNumber FROM Borrowers ORDER BY UserNumber DESC;";
            int maxId = 1000;
            using (var reader = maxCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string idStr = reader.GetString(0);
                    if (idStr.StartsWith("M-") && int.TryParse(idStr.Substring(2), out int parsedId))
                    {
                        if (parsedId > maxId) maxId = parsedId;
                    }
                }
            }

            userNumber = $"M-{maxId + 1}";

            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO Borrowers (UserNumber, Name, Sex, NIC, Address, RegistrationDate)
                                      VALUES (@id, @name, @sex, @nic, @addr, @reg);";
            insertCmd.Parameters.AddWithValue("@id", userNumber);
            insertCmd.Parameters.AddWithValue("@name", name.Trim());
            insertCmd.Parameters.AddWithValue("@sex", sex);
            insertCmd.Parameters.AddWithValue("@nic", nic.Trim());
            insertCmd.Parameters.AddWithValue("@addr", address.Trim());
            insertCmd.Parameters.AddWithValue("@reg", DateTime.Now.ToString("o"));
            insertCmd.ExecuteNonQuery();

            resultMessage = $"[BORROWER REGISTERED] Registered {name} successfully with User Number: '{userNumber}'.";
            return true;
        }

        public Borrower? GetBorrowerDetails(string userNumber)
        {
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserNumber, Name, Sex, NIC, Address, RegistrationDate FROM Borrowers WHERE UserNumber = @id;";
            cmd.Parameters.AddWithValue("@id", userNumber.Trim());

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            var borrower = new Borrower
            {
                UserNumber = reader.GetString(0),
                Name = reader.GetString(1),
                Sex = reader.GetString(2),
                NIC = reader.GetString(3),
                Address = reader.GetString(4),
                RegistrationDate = DateTime.Parse(reader.GetString(5))
            };

            using var loanCmd = conn.CreateCommand();
            loanCmd.CommandText = @"SELECT COUNT(*), 
                                           SUM(CASE WHEN DateTime(DueDate) < DateTime('now') THEN 1 ELSE 0 END)
                                    FROM LoanRecords 
                                    WHERE UserNumber = @id AND Status = 'Active';";
            loanCmd.Parameters.AddWithValue("@id", userNumber.Trim());

            using var loanReader = loanCmd.ExecuteReader();
            if (loanReader.Read())
            {
                borrower.ActiveLoansCount = loanReader.GetInt32(0);
                int overdueCount = loanReader.IsDBNull(1) ? 0 : loanReader.GetInt32(1);
                borrower.HasOverdueLoans = overdueCount > 0;
            }

            return borrower;
        }

        public List<Borrower> GetAllBorrowers()
        {
            var list = new List<Borrower>();
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserNumber FROM Borrowers ORDER BY UserNumber ASC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var b = GetBorrowerDetails(reader.GetString(0));
                if (b != null) list.Add(b);
            }
            return list;
        }
    }
}

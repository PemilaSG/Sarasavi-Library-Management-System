using System;
using System.Collections.Generic;
using SarasaviLibrarySystem.Data;
using SarasaviLibrarySystem.Models;

namespace SarasaviLibrarySystem.Services
{
    public class BorrowerService
    {
        public bool RegisterBorrower(string name, string sex, string nic, string address, out string userNumber, out string resultMessage)
        {
            userNumber = string.Empty;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(nic) || string.IsNullOrWhiteSpace(address))
            {
                resultMessage = "Name, NIC, and Address are required.";
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();
            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "SELECT UserNumber FROM Borrowers WHERE NIC = @nic;";
                LibraryDbContext.AddParam(checkCmd, "@nic", nic);

                object? existing = checkCmd.ExecuteScalar();
                if (existing != null)
                {
                    resultMessage = $"Borrower with NIC '{nic}' is already registered with User Number {existing}.";
                    return false;
                }
            }

            int nextId = 1001;
            using (var maxCmd = conn.CreateCommand())
            {
                maxCmd.CommandText = "SELECT UserNumber FROM Borrowers ORDER BY UserNumber DESC;";
                using var reader = maxCmd.ExecuteReader();
                while (reader.Read())
                {
                    string code = reader.GetValue(0)?.ToString() ?? "";
                    if (code.StartsWith("M-") && int.TryParse(code.Substring(2), out int val))
                    {
                        if (val >= nextId) nextId = val + 1;
                    }
                }
            }

            userNumber = $"M-{nextId}";

            using (var insertCmd = conn.CreateCommand())
            {
                insertCmd.CommandText = @"INSERT INTO Borrowers (UserNumber, Name, Sex, NIC, Address, RegistrationDate)
                                          VALUES (@unum, @name, @sex, @nic, @addr, @reg);";
                LibraryDbContext.AddParam(insertCmd, "@unum", userNumber);
                LibraryDbContext.AddParam(insertCmd, "@name", name);
                LibraryDbContext.AddParam(insertCmd, "@sex", sex);
                LibraryDbContext.AddParam(insertCmd, "@nic", nic);
                LibraryDbContext.AddParam(insertCmd, "@addr", address);
                LibraryDbContext.AddParam(insertCmd, "@reg", DateTime.Now);

                insertCmd.ExecuteNonQuery();
            }

            resultMessage = $"Borrower '{name}' registered successfully! Assigned User Number: {userNumber}";
            return true;
        }

        public Borrower? GetBorrowerDetails(string userNumber)
        {
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT UserNumber, Name, Sex, NIC, Address, RegistrationDate FROM Borrowers WHERE UserNumber = @unum OR NIC = @unum;";
            LibraryDbContext.AddParam(cmd, "@unum", userNumber);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                object rawDate = reader.GetValue(5);
                DateTime regDate = rawDate is DateTime dt ? dt : (DateTime.TryParse(rawDate?.ToString(), out var d) ? d : DateTime.Now);

                var b = new Borrower
                {
                    UserNumber = reader.GetValue(0)?.ToString() ?? "",
                    Name = reader.GetValue(1)?.ToString() ?? "",
                    Sex = reader.GetValue(2)?.ToString() ?? "",
                    NIC = reader.GetValue(3)?.ToString() ?? "",
                    Address = reader.GetValue(4)?.ToString() ?? "",
                    RegistrationDate = regDate
                };
                reader.Close();

                b.ActiveLoansCount = GetActiveLoansCount(b.UserNumber, conn);
                b.HasOverdueLoans = CheckHasOverdueLoans(b.UserNumber, conn);
                return b;
            }

            return null;
        }

        public List<Borrower> GetAllBorrowers()
        {
            var list = new List<Borrower>();
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT UserNumber, Name, Sex, NIC, Address, RegistrationDate FROM Borrowers ORDER BY UserNumber ASC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                object rawDate = reader.GetValue(5);
                DateTime regDate = rawDate is DateTime dt ? dt : (DateTime.TryParse(rawDate?.ToString(), out var d) ? d : DateTime.Now);

                var b = new Borrower
                {
                    UserNumber = reader.GetValue(0)?.ToString() ?? "",
                    Name = reader.GetValue(1)?.ToString() ?? "",
                    Sex = reader.GetValue(2)?.ToString() ?? "",
                    NIC = reader.GetValue(3)?.ToString() ?? "",
                    Address = reader.GetValue(4)?.ToString() ?? "",
                    RegistrationDate = regDate
                };
                list.Add(b);
            }

            foreach (var item in list)
            {
                item.ActiveLoansCount = GetActiveLoansCount(item.UserNumber, conn);
                item.HasOverdueLoans = CheckHasOverdueLoans(item.UserNumber, conn);
            }

            return list;
        }

        private int GetActiveLoansCount(string userNumber, System.Data.Common.DbConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM LoanRecords WHERE UserNumber = @unum AND Status = 'Active';";
            LibraryDbContext.AddParam(cmd, "@unum", userNumber);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        private bool CheckHasOverdueLoans(string userNumber, System.Data.Common.DbConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM LoanRecords WHERE UserNumber = @unum AND Status = 'Active' AND DueDate < @now;";
            LibraryDbContext.AddParam(cmd, "@unum", userNumber);
            LibraryDbContext.AddParam(cmd, "@now", DateTime.Now);
            long count = Convert.ToInt64(cmd.ExecuteScalar() ?? 0);
            return count > 0;
        }
    }
}

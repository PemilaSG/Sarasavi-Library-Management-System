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

        public bool UpdateBorrower(string userNumber, string name, string sex, string nic, string address, out string resultMessage)
        {
            if (string.IsNullOrWhiteSpace(userNumber) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(nic) || string.IsNullOrWhiteSpace(address))
            {
                resultMessage = "All fields (Name, NIC, Address) are required.";
                return false;
            }

            int activeLoans = GetActiveLoansCount(userNumber);
            if (activeLoans > 0)
            {
                resultMessage = $"Cannot update borrower '{userNumber}' because they currently have {activeLoans} active checked-out loan(s). All borrowed books must be returned first.";
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();

            using (var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandText = "SELECT UserNumber FROM Borrowers WHERE NIC = @nic AND UserNumber != @unum;";
                LibraryDbContext.AddParam(checkCmd, "@nic", nic);
                LibraryDbContext.AddParam(checkCmd, "@unum", userNumber);

                object? existing = checkCmd.ExecuteScalar();
                if (existing != null)
                {
                    resultMessage = $"NIC '{nic}' is already registered to another borrower ({existing}).";
                    return false;
                }
            }

            using (var updateCmd = conn.CreateCommand())
            {
                updateCmd.CommandText = @"UPDATE Borrowers 
                                          SET Name = @name, Sex = @sex, NIC = @nic, Address = @addr 
                                          WHERE UserNumber = @unum;";
                LibraryDbContext.AddParam(updateCmd, "@name", name);
                LibraryDbContext.AddParam(updateCmd, "@sex", sex);
                LibraryDbContext.AddParam(updateCmd, "@nic", nic);
                LibraryDbContext.AddParam(updateCmd, "@addr", address);
                LibraryDbContext.AddParam(updateCmd, "@unum", userNumber);

                int rows = updateCmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    resultMessage = $"Borrower '{userNumber}' updated successfully.";
                    return true;
                }
                else
                {
                    resultMessage = $"Borrower '{userNumber}' was not found.";
                    return false;
                }
            }
        }

        public bool DeleteBorrower(string userNumber, out string resultMessage)
        {
            if (string.IsNullOrWhiteSpace(userNumber))
            {
                resultMessage = "Invalid User Number.";
                return false;
            }

            int activeLoans = GetActiveLoansCount(userNumber);
            if (activeLoans > 0)
            {
                resultMessage = $"Cannot delete borrower '{userNumber}' because they currently have {activeLoans} active checked-out loan(s). All borrowed books must be returned first.";
                return false;
            }

            using var conn = LibraryDbContext.GetConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                using (var delResCmd = conn.CreateCommand())
                {
                    delResCmd.Transaction = tx;
                    delResCmd.CommandText = "DELETE FROM ReservationRecords WHERE UserNumber = @unum;";
                    LibraryDbContext.AddParam(delResCmd, "@unum", userNumber);
                    delResCmd.ExecuteNonQuery();
                }

                using (var delLoanCmd = conn.CreateCommand())
                {
                    delLoanCmd.Transaction = tx;
                    delLoanCmd.CommandText = "DELETE FROM LoanRecords WHERE UserNumber = @unum;";
                    LibraryDbContext.AddParam(delLoanCmd, "@unum", userNumber);
                    delLoanCmd.ExecuteNonQuery();
                }

                using (var delCmd = conn.CreateCommand())
                {
                    delCmd.Transaction = tx;
                    delCmd.CommandText = "DELETE FROM Borrowers WHERE UserNumber = @unum;";
                    LibraryDbContext.AddParam(delCmd, "@unum", userNumber);
                    int rows = delCmd.ExecuteNonQuery();
                    if (rows == 0)
                    {
                        tx.Rollback();
                        resultMessage = $"Borrower '{userNumber}' was not found.";
                        return false;
                    }
                }

                tx.Commit();
                resultMessage = $"Borrower '{userNumber}' deleted successfully.";
                return true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                resultMessage = $"Error deleting borrower: {ex.Message}";
                return false;
            }
        }

        public Borrower? GetBorrowerDetails(string userNumber)
        {
            using var conn = LibraryDbContext.GetConnection();
            
            Borrower? b = null;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT UserNumber, Name, Sex, NIC, Address, RegistrationDate FROM Borrowers WHERE UserNumber = @unum OR NIC = @unum;";
                LibraryDbContext.AddParam(cmd, "@unum", userNumber);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        object rawDate = reader.GetValue(5);
                        DateTime regDate = rawDate is DateTime dt ? dt : (DateTime.TryParse(rawDate?.ToString(), out var d) ? d : DateTime.Now);

                        b = new Borrower
                        {
                            UserNumber = reader.GetValue(0)?.ToString() ?? "",
                            Name = reader.GetValue(1)?.ToString() ?? "",
                            Sex = reader.GetValue(2)?.ToString() ?? "",
                            NIC = reader.GetValue(3)?.ToString() ?? "",
                            Address = reader.GetValue(4)?.ToString() ?? "",
                            RegistrationDate = regDate
                        };
                    }
                }
            }

            if (b != null)
            {
                b.ActiveLoansCount = GetActiveLoansCount(b.UserNumber);
                b.HasOverdueLoans = CheckHasOverdueLoans(b.UserNumber);
                return b;
            }

            return null;
        }

        public List<Borrower> GetAllBorrowers()
        {
            var list = new List<Borrower>();
            using var conn = LibraryDbContext.GetConnection();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT UserNumber, Name, Sex, NIC, Address, RegistrationDate FROM Borrowers ORDER BY UserNumber ASC;";

                using (var reader = cmd.ExecuteReader())
                {
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
                }
            }

            foreach (var item in list)
            {
                item.ActiveLoansCount = GetActiveLoansCount(item.UserNumber);
                item.HasOverdueLoans = CheckHasOverdueLoans(item.UserNumber);
            }

            return list;
        }

        private int GetActiveLoansCount(string userNumber)
        {
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM LoanRecords WHERE UserNumber = @unum AND Status = 'Active';";
            LibraryDbContext.AddParam(cmd, "@unum", userNumber);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        private bool CheckHasOverdueLoans(string userNumber)
        {
            using var conn = LibraryDbContext.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM LoanRecords WHERE UserNumber = @unum AND Status = 'Active' AND DueDate < @now;";
            LibraryDbContext.AddParam(cmd, "@unum", userNumber);
            LibraryDbContext.AddParam(cmd, "@now", DateTime.Now);
            long count = Convert.ToInt64(cmd.ExecuteScalar() ?? 0);
            return count > 0;
        }
    }
}

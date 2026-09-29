using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using SarasaviLibrarySystem.Models;

namespace SarasaviLibrarySystem.Data
{
    public static class LibraryDbContext
    {
        public static bool IsMySqlActive { get; private set; } = false;

        private static readonly string MySqlConnectionString = "Server=localhost;Database=sarasavi_library;Uid=root;Pwd=;";
        private static readonly string SqlitePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sarasavi_library.db");
        private static readonly string SqliteConnectionString = $"Data Source={SqlitePath}";

        public static DbConnection GetConnection()
        {
            if (IsMySqlActive)
            {
                var conn = new MySqlConnection(MySqlConnectionString);
                conn.Open();
                return conn;
            }
            else
            {
                var conn = new SqliteConnection(SqliteConnectionString);
                conn.Open();
                return conn;
            }
        }

        public static void InitializeDatabase()
        {
            try
            {
                using var conn = new MySqlConnection(MySqlConnectionString);
                conn.Open();
                IsMySqlActive = true;

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS BookTitles (
                        TitleId INT AUTO_INCREMENT PRIMARY KEY,
                        AccessionCode VARCHAR(10) UNIQUE NOT NULL,
                        Title VARCHAR(255) NOT NULL,
                        Author VARCHAR(255) NOT NULL,
                        Publisher VARCHAR(255) NOT NULL,
                        ClassificationCode CHAR(1) NOT NULL
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS BookCopies (
                        AccessionNumber VARCHAR(20) PRIMARY KEY,
                        TitleId INT NOT NULL,
                        CopyType VARCHAR(20) NOT NULL DEFAULT 'Borrowable',
                        Status VARCHAR(20) NOT NULL DEFAULT 'Available'
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS Borrowers (
                        UserNumber VARCHAR(20) PRIMARY KEY,
                        Name VARCHAR(255) NOT NULL,
                        Sex VARCHAR(10) NOT NULL DEFAULT 'Male',
                        NIC VARCHAR(20) NOT NULL UNIQUE,
                        Address TEXT NOT NULL,
                        RegistrationDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS LoanRecords (
                        LoanId INT AUTO_INCREMENT PRIMARY KEY,
                        CopyAccessionNumber VARCHAR(20) NOT NULL,
                        UserNumber VARCHAR(20) NOT NULL,
                        IssueDate DATETIME NOT NULL,
                        DueDate DATETIME NOT NULL,
                        ReturnDate DATETIME NULL,
                        Status VARCHAR(20) NOT NULL DEFAULT 'Active'
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

                    CREATE TABLE IF NOT EXISTS ReservationRecords (
                        ReservationId INT AUTO_INCREMENT PRIMARY KEY,
                        TitleId INT NOT NULL,
                        UserNumber VARCHAR(20) NOT NULL,
                        RequestDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        Status VARCHAR(20) NOT NULL DEFAULT 'Pending'
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
                ";
                cmd.ExecuteNonQuery();
                SeedSampleData(conn);
                return;
            }
            catch
            {
                IsMySqlActive = false;
            }

            using (var conn = new SqliteConnection(SqliteConnectionString))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS BookTitles (
                        TitleId INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccessionCode TEXT UNIQUE NOT NULL,
                        Title TEXT NOT NULL,
                        Author TEXT NOT NULL,
                        Publisher TEXT NOT NULL,
                        ClassificationCode TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS BookCopies (
                        AccessionNumber TEXT PRIMARY KEY,
                        TitleId INTEGER NOT NULL,
                        CopyType TEXT NOT NULL,
                        Status TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Borrowers (
                        UserNumber TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Sex TEXT NOT NULL,
                        NIC TEXT NOT NULL,
                        Address TEXT NOT NULL,
                        RegistrationDate TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS LoanRecords (
                        LoanId INTEGER PRIMARY KEY AUTOINCREMENT,
                        CopyAccessionNumber TEXT NOT NULL,
                        UserNumber TEXT NOT NULL,
                        IssueDate TEXT NOT NULL,
                        DueDate TEXT NOT NULL,
                        ReturnDate TEXT,
                        Status TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS ReservationRecords (
                        ReservationId INTEGER PRIMARY KEY AUTOINCREMENT,
                        TitleId INTEGER NOT NULL,
                        UserNumber TEXT NOT NULL,
                        RequestDate TEXT NOT NULL,
                        Status TEXT NOT NULL
                    );
                ";
                cmd.ExecuteNonQuery();
                SeedSampleData(conn);
            }
        }

        private static void SeedSampleData(DbConnection conn)
        {
            using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM BookTitles;";
            long titleCount = Convert.ToInt64(checkCmd.ExecuteScalar() ?? 0);
            if (titleCount > 0) return;

            using var transaction = conn.BeginTransaction();

            var borrowers = new List<(string id, string name, string sex, string nic, string addr)>
            {
                ("M-1001", "Kamal Perera", "Male", "921820482V", "123 Galle Road, Colombo 03"),
                ("M-1002", "Nimali Fernando", "Female", "956123456V", "45 Kandy Road, Kiribathgoda"),
                ("M-1003", "Sunil Jayasinghe", "Male", "882049182V", "88 Station Road, Nugegoda"),
                ("M-1004", "Dilhani Silva", "Female", "981293847V", "12 Main Street, Maharagama"),
                ("M-1005", "Anura Bandara", "Male", "851928473V", "56 Lake Road, Kurunegala")
            };

            foreach (var b in borrowers)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Borrowers (UserNumber, Name, Sex, NIC, Address, RegistrationDate) VALUES (@id, @name, @sex, @nic, @addr, @reg);";
                AddParam(cmd, "@id", b.id);
                AddParam(cmd, "@name", b.name);
                AddParam(cmd, "@sex", b.sex);
                AddParam(cmd, "@nic", b.nic);
                AddParam(cmd, "@addr", b.addr);
                AddParam(cmd, "@reg", DateTime.Now.AddDays(-30).ToString("o"));
                cmd.ExecuteNonQuery();
            }

            var titles = new List<(string code, string title, string author, string pub, char cls, int copies, bool refOnlyFirst)>
            {
                ("C0001", "Access 2022 All-in-One Desk Reference", "Alan Simpson & Margaret Levine Young", "Wiley Publishing", 'C', 3, true),
                ("C0002", "C# 10 and .NET 6 Modern Cross-Platform Development", "Mark J. Price", "Packt Publishing", 'C', 4, false),
                ("C0003", "Clean Code: A Handbook of Agile Software Craftsmanship", "Robert C. Martin", "Prentice Hall", 'C', 2, false),
                ("F0001", "Madol Doova", "Martin Wickramasinghe", "Sarasa Publishers", 'F', 5, false),
                ("F0002", "Gamperaliya", "Martin Wickramasinghe", "Sarasa Publishers", 'F', 3, false),
                ("S0001", "A Brief History of Time", "Stephen Hawking", "Bantam Books", 'S', 2, true),
                ("M0001", "Principles of Marketing", "Philip Kotler", "Pearson", 'M', 4, false)
            };

            int titleIdCounter = 1;
            foreach (var t in titles)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO BookTitles (TitleId, AccessionCode, Title, Author, Publisher, ClassificationCode) 
                                   VALUES (@id, @code, @title, @author, @pub, @cls);";
                AddParam(cmd, "@id", titleIdCounter);
                AddParam(cmd, "@code", t.code);
                AddParam(cmd, "@title", t.title);
                AddParam(cmd, "@author", t.author);
                AddParam(cmd, "@pub", t.pub);
                AddParam(cmd, "@cls", t.cls.ToString());
                cmd.ExecuteNonQuery();

                for (int i = 1; i <= t.copies; i++)
                {
                    string copyAcc = $"{t.code}-{i:D2}";
                    string copyType = (i == 1 && t.refOnlyFirst) ? "Reference Only" : "Borrowable";

                    using var copyCmd = conn.CreateCommand();
                    copyCmd.CommandText = "INSERT INTO BookCopies (AccessionNumber, TitleId, CopyType, Status) VALUES (@acc, @tid, @type, 'Available');";
                    AddParam(copyCmd, "@acc", copyAcc);
                    AddParam(copyCmd, "@tid", titleIdCounter);
                    AddParam(copyCmd, "@type", copyType);
                    copyCmd.ExecuteNonQuery();
                }

                titleIdCounter++;
            }

            using (var lCmd = conn.CreateCommand())
            {
                lCmd.CommandText = @"INSERT INTO LoanRecords (CopyAccessionNumber, UserNumber, IssueDate, DueDate, Status)
                                     VALUES ('C0002-01', 'M-1001', @iss, @due, 'Active');";
                AddParam(lCmd, "@iss", DateTime.Now.AddDays(-5).ToString("o"));
                AddParam(lCmd, "@due", DateTime.Now.AddDays(9).ToString("o"));
                lCmd.ExecuteNonQuery();

                using var statusCmd = conn.CreateCommand();
                statusCmd.CommandText = "UPDATE BookCopies SET Status = 'Borrowed' WHERE AccessionNumber = 'C0002-01';";
                statusCmd.ExecuteNonQuery();
            }

            using (var lCmd2 = conn.CreateCommand())
            {
                lCmd2.CommandText = @"INSERT INTO LoanRecords (CopyAccessionNumber, UserNumber, IssueDate, DueDate, Status)
                                      VALUES ('F0001-01', 'M-1003', @iss, @due, 'Active');";
                AddParam(lCmd2, "@iss", DateTime.Now.AddDays(-20).ToString("o"));
                AddParam(lCmd2, "@due", DateTime.Now.AddDays(-6).ToString("o"));
                lCmd2.ExecuteNonQuery();

                using var statusCmd = conn.CreateCommand();
                statusCmd.CommandText = "UPDATE BookCopies SET Status = 'Borrowed' WHERE AccessionNumber = 'F0001-01';";
                statusCmd.ExecuteNonQuery();
            }

            using (var rCmd = conn.CreateCommand())
            {
                rCmd.CommandText = @"INSERT INTO ReservationRecords (TitleId, UserNumber, RequestDate, Status)
                                     VALUES (2, 'M-1002', @req, 'Pending');";
                AddParam(rCmd, "@req", DateTime.Now.AddDays(-2).ToString("o"));
                rCmd.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static void AddParam(DbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }
    }
}

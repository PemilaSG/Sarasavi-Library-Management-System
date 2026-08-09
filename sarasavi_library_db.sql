CREATE DATABASE IF NOT EXISTS sarasavi_library;
USE sarasavi_library;

DROP TABLE IF EXISTS ReservationRecords;
DROP TABLE IF EXISTS LoanRecords;
DROP TABLE IF EXISTS BookCopies;
DROP TABLE IF EXISTS BookTitles;
DROP TABLE IF EXISTS Borrowers;

CREATE TABLE BookTitles (
    TitleId INT AUTO_INCREMENT PRIMARY KEY,
    AccessionCode VARCHAR(10) UNIQUE NOT NULL,
    Title VARCHAR(255) NOT NULL,
    Author VARCHAR(255) NOT NULL,
    Publisher VARCHAR(255) NOT NULL,
    ClassificationCode CHAR(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE BookCopies (
    AccessionNumber VARCHAR(20) PRIMARY KEY,
    TitleId INT NOT NULL,
    CopyType VARCHAR(20) NOT NULL DEFAULT 'Borrowable',
    Status VARCHAR(20) NOT NULL DEFAULT 'Available',
    FOREIGN KEY (TitleId) REFERENCES BookTitles(TitleId) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE Borrowers (
    UserNumber VARCHAR(20) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Sex VARCHAR(10) NOT NULL DEFAULT 'Male',
    NIC VARCHAR(20) NOT NULL UNIQUE,
    Address TEXT NOT NULL,
    RegistrationDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE LoanRecords (
    LoanId INT AUTO_INCREMENT PRIMARY KEY,
    CopyAccessionNumber VARCHAR(20) NOT NULL,
    UserNumber VARCHAR(20) NOT NULL,
    IssueDate DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Active',
    FOREIGN KEY (CopyAccessionNumber) REFERENCES BookCopies(AccessionNumber),
    FOREIGN KEY (UserNumber) REFERENCES Borrowers(UserNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE ReservationRecords (
    ReservationId INT AUTO_INCREMENT PRIMARY KEY,
    TitleId INT NOT NULL,
    UserNumber VARCHAR(20) NOT NULL,
    RequestDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    FOREIGN KEY (TitleId) REFERENCES BookTitles(TitleId),
    FOREIGN KEY (UserNumber) REFERENCES Borrowers(UserNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO Borrowers (UserNumber, Name, Sex, NIC, Address, RegistrationDate) VALUES
('M-1001', 'Kamal Perera', 'Male', '921820482V', '123 Galle Road, Colombo 03', '2026-07-10 10:00:00'),
('M-1002', 'Nimali Fernando', 'Female', '956123456V', '45 Kandy Road, Kiribathgoda', '2026-07-12 11:30:00'),
('M-1003', 'Sunil Jayasinghe', 'Male', '882049182V', '88 Station Road, Nugegoda', '2026-07-15 09:15:00'),
('M-1004', 'Dilhani Silva', 'Female', '981293847V', '12 Main Street, Maharagama', '2026-07-20 14:00:00'),
('M-1005', 'Anura Bandara', 'Male', '851928473V', '56 Lake Road, Kurunegala', '2026-07-25 16:45:00');

INSERT INTO BookTitles (TitleId, AccessionCode, Title, Author, Publisher, ClassificationCode) VALUES
(1, 'C0001', 'Access 2022 All-in-One Desk Reference for Dummies', 'Alan Simpson & Margaret Levine Young', 'Wiley Publishing', 'C'),
(2, 'C0002', 'C# 10 and .NET 6 Modern Cross-Platform Development', 'Mark J. Price', 'Packt Publishing', 'C'),
(3, 'C0003', 'Clean Code: A Handbook of Agile Software Craftsmanship', 'Robert C. Martin', 'Prentice Hall', 'C'),
(4, 'F0001', 'Madol Doova', 'Martin Wickramasinghe', 'Sarasa Publishers', 'F'),
(5, 'F0002', 'Gamperaliya', 'Martin Wickramasinghe', 'Sarasa Publishers', 'F'),
(6, 'S0001', 'A Brief History of Time', 'Stephen Hawking', 'Bantam Books', 'S'),
(7, 'M0001', 'Principles of Marketing', 'Philip Kotler', 'Pearson', 'M');

INSERT INTO BookCopies (AccessionNumber, TitleId, CopyType, Status) VALUES
('C0001-01', 1, 'Reference Only', 'Available'),
('C0001-02', 1, 'Borrowable', 'Available'),
('C0001-03', 1, 'Borrowable', 'Available'),
('C0002-01', 2, 'Borrowable', 'Borrowed'),
('C0002-02', 2, 'Borrowable', 'Available'),
('C0002-03', 2, 'Borrowable', 'Available'),
('C0002-04', 2, 'Borrowable', 'Available'),
('C0003-01', 3, 'Borrowable', 'Available'),
('C0003-02', 3, 'Borrowable', 'Available'),
('F0001-01', 4, 'Borrowable', 'Borrowed'),
('F0001-02', 4, 'Borrowable', 'Available'),
('F0001-03', 4, 'Borrowable', 'Available'),
('F0001-04', 4, 'Borrowable', 'Available'),
('F0001-05', 4, 'Borrowable', 'Available'),
('F0002-01', 5, 'Borrowable', 'Available'),
('F0002-02', 5, 'Borrowable', 'Available'),
('F0002-03', 5, 'Borrowable', 'Available'),
('S0001-01', 6, 'Reference Only', 'Available'),
('S0001-02', 6, 'Borrowable', 'Available'),
('M0001-01', 7, 'Borrowable', 'Available'),
('M0001-02', 7, 'Borrowable', 'Available'),
('M0001-03', 7, 'Borrowable', 'Available'),
('M0001-04', 7, 'Borrowable', 'Available');

INSERT INTO LoanRecords (LoanId, CopyAccessionNumber, UserNumber, IssueDate, DueDate, ReturnDate, Status) VALUES
(1, 'C0002-01', 'M-1001', '2026-08-04 10:00:00', '2026-08-18 10:00:00', NULL, 'Active'),
(2, 'F0001-01', 'M-1003', '2026-07-20 09:30:00', '2026-08-03 09:30:00', NULL, 'Active');

INSERT INTO ReservationRecords (ReservationId, TitleId, UserNumber, RequestDate, Status) VALUES
(1, 2, 'M-1002', '2026-08-07 14:20:00', 'Pending');

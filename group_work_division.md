# Sarasavi Library Management System - Group Work Division Plan

**Course:** DIT2145 - Software Development Project (Final Project)  
**Institution:** Institute of Management & Business Studies (IMBS Green Campus)  
**Target:** 5-Member Team Allocation Strategy  

---

## 📊 Team Allocation & Complexity Overview

```
┌────────────────────────────────────────────────────────────────────────┐
│ Person 1 (LEAD BACKEND DEVELOPER) - 50% Complexity                     │
│ ├─ Database Layer & SQLite Persistence                                 │
│ ├─ Loan Rules Engine (5-book limit, overdue block, reference block)    │
│ ├─ Return Engine & Reservation Queue + Popup Alert                     │
│ └─ X9999 Accession Code Algorithm                                      │
├────────────────────────────────────────────────────────────────────────┤
│ Person 2 (BOOK REGISTRATION) - 12.5% Complexity                        │
│ └─ Basic Form Inputs for Title, Author, Publisher, Copy Count (1-10)   │
├────────────────────────────────────────────────────────────────────────┤
│ Person 3 (BORROWER REGISTRATION) - 12.5% Complexity                    │
│ └─ Basic Form Inputs for Name, Sex, NIC, Address                       │
├────────────────────────────────────────────────────────────────────────┤
│ Person 4 (CATALOG SEARCH UI) - 12.5% Complexity                        │
│ └─ Search Textbox + Data Table Grid with Status Badges                 │
├────────────────────────────────────────────────────────────────────────┤
│ Person 5 (DASHBOARD & MENU) - 12.5% Complexity                         │
│ └─ Sidebar Menu Navigation + Summary Display Cards                     │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 🔥 Person 1: Lead Developer / System Architect (Highest Complexity)

> **Focus:** Backend Logic, Database Infrastructure, Rule Engines & OOP Algorithms

Person 1 acts as the team technical lead and builds the core business logic engine and data persistence layer.

### Features & Responsibilities:
1. **Database & Data Access Layer (`LibraryDbContext.cs`):**
   * Configure SQLite database connection (`sarasavi_library.db`).
   * Create auto-migration & table creation scripts for all entities.
   * Write seed script to auto-generate 30+ sample books, 80+ physical copies, borrowers, active loans, and overdue records.
2. **Loan Validation Rules Engine (`LoanService.cs`):**
   * **Rule 1 (5-Book Maximum Limit):** Query active loans and block checkout if count $\ge 5$.
   * **Rule 2 (Overdue Block):** Query borrower history and block checkout if member holds unreturned overdue books.
   * **Rule 3 (Reference Only Block):** Verify copy type and block checkout if marked `Reference Only`.
   * **Rule 4 (14-Day Duration):** Auto-calculate return due date (Issue Date + 14 days).
3. **Return Engine & Reservation Queue (`ReturnService.cs`, `ReservationService.cs`):**
   * Process returned copy $\rightarrow$ Update status to `Available`.
   * Scan active title reservations ordered by FIFO timestamp (oldest request first).
   * Trigger **Set-Aside Notification Popup Alert** instructing librarian to hold book for the reserving member.
4. **Accession Generator Algorithm (`AccessionGenerator.cs`):**
   * Logic to format 1-byte classification code + 4-digit sequential index (`X9999`).
   * Append copy suffix logic (`C0001-01` to `C0001-10`).

* **Owned Files:** `LibraryDbContext.cs`, `LoanService.cs`, `ReturnService.cs`, `ReservationService.cs`, `AccessionGenerator.cs`, `DatabaseInitializer.cs`, `Models/*.cs`

---

## 🟢 Person 2: Book Registration Specialist (Low Complexity)

> **Focus:** Book Details Form Entry & Copy Selection

Person 2 builds the straightforward data entry form for adding new books to the catalog.

### Features & Responsibilities:
1. **Book Entry Form UI (`BookRegistrationView.cs`):**
   * Inputs for Book Title, Author Name, Publisher.
   * Dropdown selector for Classification (`C` - Computing, `F` - Fiction, `S` - Science, `M` - Management).
2. **Copy Count Controls:**
   * Numeric spinner control for selecting number of copies (1 to 10 max).
3. **Copy Type Selector:**
   * Radio button options for `Borrowable` vs. `Reference Only`.
4. **Save Handler:**
   * Save button click handler that calls Person 1's backend method (`BookService.AddBook()`).

* **Owned UI Screen:** `Book Registration Form Screen`
* **Owned Files:** `BookRegistrationView.cs`

---

## 🟢 Person 3: Borrower Registration Specialist (Low Complexity)

> **Focus:** Member Entry Form & Validation

Person 3 builds the user registration form for adding new library members.

### Features & Responsibilities:
1. **Borrower Registration Form UI (`BorrowerRegistrationView.cs`):**
   * Fields for User Number (Auto-ID display), Full Name, Sex/Gender (Male/Female radio buttons), NIC Number, and Address text area.
2. **Basic Field Validation:**
   * Ensure Name, NIC, and Address fields are not blank before submitting.
3. **Registered Members Summary Grid:**
   * Simple side table displaying list of registered members.
4. **Save Handler:**
   * Save button click handler that calls Person 1's backend method (`BorrowerService.AddBorrower()`).

* **Owned UI Screen:** `Borrower Registration Screen`
* **Owned Files:** `BorrowerRegistrationView.cs`

---

## 🟢 Person 4: Catalog Search & Inquiry Specialist (Low Complexity)

> **Focus:** Inventory Search Bar & Data Grid View

Person 4 builds the catalog inquiry screen allowing users to search books and view copy availability.

### Features & Responsibilities:
1. **Search Bar Interface (`InquiryView.cs`):**
   * Search textbox with dropdown filter (Search by Accession Code `X9999`, Book Title, or Author).
2. **Catalog Data Grid:**
   * Data table displaying search results (Accession Code, Title, Author, Classification, Total Copies, Available Copies).
3. **Status Badges:**
   * Visual color-coded tags: `Available` (Green), `Borrowed` (Blue), `Reference Only` (Yellow), `Reserved` (Orange).
4. **Filter Event Handler:**
   * Text-changed event handler calling Person 1's backend search method (`CatalogService.Search()`).

* **Owned UI Screen:** `Catalog Inquiry Search Screen`
* **Owned Files:** `InquiryView.cs`

---

## 🟢 Person 5: Dashboard & UI Layout Specialist (Low Complexity)

> **Focus:** Main Navigation Menu, Dashboard Cards & Project Documentation

Person 5 builds the outer window container, navigation sidebar, dashboard tiles, and handles group report coordination.

### Features & Responsibilities:
1. **Main Application Shell (`MainForm.cs`):**
   * Window layout with top header bar and left sidebar navigation panel.
2. **Sidebar Menu Navigation:**
   * Tab button click handlers to switch active views (Dashboard, Book Entry, User Entry, Search).
3. **Dashboard Metric Cards UI (`DashboardView.cs`):**
   * 5 visual summary cards displaying metrics (Total Books, Available Copies, Active Loans, Overdue Count, Registered Members).
4. **Documentation Coordinator:**
   * Combine all team member write-ups into the final submission report.

* **Owned UI Screen:** `Main Dashboard Screen`
* **Owned Files:** `MainForm.cs`, `DashboardView.cs`

---

## 📋 Task Distribution Table for Viva / Presentation

| Team Member | Role | Complexity | UI Screen | Viva Presentation Topic |
| :--- | :--- | :--- | :--- | :--- |
| **Person 1** | Lead Backend Architect | 🔥 High (50%) | Backend Engine | Database Layer, Loan Business Rules & Reservation Queue |
| **Person 2** | Book Entry Developer | 🟢 Low (12.5%) | Book Registration | Book Metadata Entry & Batch Copy Count (1-10) |
| **Person 3** | Member Entry Developer | 🟢 Low (12.5%) | Borrower Registration | Member Registration & NIC Data Capture |
| **Person 4** | Search UI Developer | 🟢 Low (12.5%) | Catalog Inquiry | Search Filters & Status Badge Data Grid |
| **Person 5** | UI Shell & Dashboard Dev | 🟢 Low (12.5%) | Main Dashboard | Main Shell Navigation & Metric Summary Tiles |

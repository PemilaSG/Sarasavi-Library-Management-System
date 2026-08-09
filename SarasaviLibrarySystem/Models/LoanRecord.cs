using System;

namespace SarasaviLibrarySystem.Models
{
    public class LoanRecord
    {
        public int LoanId { get; set; }
        public string CopyAccessionNumber { get; set; } = string.Empty;
        public string UserNumber { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(14);
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Active";

        public bool IsOverdue => Status == "Active" && DateTime.Now > DueDate;
    }
}

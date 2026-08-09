using System;

namespace SarasaviLibrarySystem.Models
{
    public class Borrower
    {
        public string UserNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sex { get; set; } = "Male";
        public string NIC { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public int ActiveLoansCount { get; set; } = 0;
        public bool HasOverdueLoans { get; set; } = false;

        public bool CanBorrow(out string reason)
        {
            if (ActiveLoansCount >= 5)
            {
                reason = "Borrower has reached maximum limit of 5 active loans.";
                return false;
            }
            if (HasOverdueLoans)
            {
                reason = "Borrower has overdue unreturned books. Loans blocked until returned.";
                return false;
            }
            reason = string.Empty;
            return true;
        }
    }
}

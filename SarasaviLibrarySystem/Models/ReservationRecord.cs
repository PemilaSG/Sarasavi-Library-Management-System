using System;

namespace SarasaviLibrarySystem.Models
{
    public class ReservationRecord
    {
        public int ReservationId { get; set; }
        public int TitleId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string UserNumber { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
    }
}

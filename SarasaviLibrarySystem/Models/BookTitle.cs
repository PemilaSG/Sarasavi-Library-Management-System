using System.Collections.Generic;

namespace SarasaviLibrarySystem.Models
{
    public class BookTitle
    {
        public int TitleId { get; set; }
        public string AccessionCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public char ClassificationCode { get; set; }
        
        public List<BookCopy> Copies { get; set; } = new List<BookCopy>();
    }
}

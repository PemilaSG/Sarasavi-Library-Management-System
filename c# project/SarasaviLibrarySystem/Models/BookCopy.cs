namespace SarasaviLibrarySystem.Models
{
    public class BookCopy
    {
        public string AccessionNumber { get; set; } = string.Empty;
        public int TitleId { get; set; }
        public string TitleName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string CopyType { get; set; } = "Borrowable";
        public string Status { get; set; } = "Available";
    }
}

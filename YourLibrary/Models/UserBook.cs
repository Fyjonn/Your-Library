namespace YourLibrary.Models
{
    public enum EnumMedia { Printed, Ebook, Audiobook }
    public enum EnumReadStatus { Read, Reading, ToRead, DNF}
    public class UserBook
    {
        public int UserBookId { get; set; }
        public bool ShelfVisibility { get; set; }
        public EnumMedia Media { get; set; }
        public bool Bookmark { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
        public bool IsBorrowed { get; set; }
        public bool IsOwned { get; set; }
        public EnumReadStatus ReadStatus { get; set; }

        // Klucze obce i wlasciwosci

        // z user
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        // z book
        public int BookId { get; set; }
        public virtual Book Book { get; set; }

        // z review
        public int? ReviewId { get; set; }
        public virtual Review Review { get; set; }

        // z borrow
        public virtual List<Borrow> Borrows { get; set; } = new List<Borrow>();

        public UserBook() { }
    }
}

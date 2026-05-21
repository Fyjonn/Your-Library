namespace YourLibrary.Models
{
    public enum EnumStatusBorrow { NotBorrowed, Requested, Borrowed, Returned}
    public class Borrow
    {
        public int BorrowId { get; set; }
        public EnumStatusBorrow StatusBorrow { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime ReturnDate { get; set; }
        // Klucze obce
        
        // z userbook
        public int UserBookId { get; set; }
        public virtual UserBook UserBook { get; set; }

        // z user
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}

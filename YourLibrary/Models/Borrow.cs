namespace YourLibrary.Models
{
    public enum EnumStatusBorrow { NotBorrowed, Requested, Borrowed, Returned, Completed }
    public class Borrow
    {
        public int BorrowId { get; set; }
        public EnumStatusBorrow StatusBorrow { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime ReturnDate { get; set; }

        public string? BorrowerLocation { get; set; }
        public string? BorrowerNotes { get; set; }
        public bool BorrowerBookmark { get; set; }

        public decimal BorrowerRating { get; set; }
        public string? BorrowerReviewComment { get; set; }
        // Klucze obce

        // z userbook
        public int UserBookId { get; set; }
        public virtual UserBook UserBook { get; set; }

        // z user
        public string ApplicationUserId { get; set; }
        public EnumReadStatus OriginalOwnerReadStatus { get; set; }
        public EnumReadStatus? BorrowerFinalReadStatus { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}

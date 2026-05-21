namespace YourLibrary.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public decimal Rating { get; set; }
        public string ReviewComment { get; set; }
        // KLucze obce i wlasciwosci

        // z userbook
        public UserBook UserBook { get; set; }

        // z book
        public virtual List<Book> Books { get; set; } = new List<Book>();

    }
}

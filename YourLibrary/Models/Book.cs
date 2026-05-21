using Microsoft.Extensions.DependencyModel;

namespace YourLibrary.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public int GoogleId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public string AgeRating { get; set; }
        public string ImageURL { get; set; }
        // KLucze obce i własciwosci

        // z userbook
        public virtual List<UserBook> UserBooks { get; set; } = new List<UserBook>();

        // z review
        public int ReviewId { get; set; }
        public virtual Review Review { get; set; }
    }
}

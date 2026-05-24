using Microsoft.AspNetCore.Identity;

namespace YourLibrary.Models
{
    public class ApplicationUser:IdentityUser
    {
        public bool ShelfVisibility { get; set; } = true;
        // Klucze obce

        // z userbook
        public virtual List<UserBook> UserBooks { get; set; } = new List<UserBook>();

        // z borrow
        public virtual List<Borrow> Borrows { get; set; } = new List<Borrow>();

        // z friendship
        public virtual List<Friendship> FriendshipRequesters { get; set; } = new List<Friendship>();
        public virtual List<Friendship> FriendshipReceivers { get; set; } = new List<Friendship>();
    }
}

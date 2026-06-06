namespace YourLibrary.Models
{
    public class FriendshipPageViewModel
    {
        public List<Friendship> ReceivedInvitations { get; set; } = new();

        public List<Friendship> SentInvitations { get; set; } = new();

        public List<Friendship> Friends { get; set; } = new();

        public List<Borrow> IncomingBorrowRequests { get; set; }
        public List<Borrow> MyBorrowedBooks { get; set; }
        public List<Borrow> MyRentedBooks { get; set; }


    }
}

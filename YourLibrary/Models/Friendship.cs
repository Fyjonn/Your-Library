namespace YourLibrary.Models
{
    public enum EnumFriendStatus { Pending, Accepted, Rejected }
    public class Friendship
    {
        public int FriendshipId { get; set; }
        public EnumFriendStatus FriendStatus { get; set; }
        //Klucze obce

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // z user
        public string RequesterId { get; set; }
        public virtual ApplicationUser Requester { get; set; }
        public string ReceiverId { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
    }
}

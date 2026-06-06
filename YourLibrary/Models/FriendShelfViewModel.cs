using System.Collections.Generic;

namespace YourLibrary.Models
{
    public class FriendShelfViewModel
    {
        public string FriendName { get; set; }
        public IEnumerable<UserBook> Books { get; set; }
    }
}
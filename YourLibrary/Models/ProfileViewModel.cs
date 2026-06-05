using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace YourLibrary.Models
{
    public class ProfileViewModel
    {
        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string? NewPassword { get; set; }

        public string? ConfirmedPassword { get; set; }

        public string Avatar { get; set; }

        public IFormFile? AvatarImage { get; set; }

        public string? AvatarImagePath { get; set; }

        public List<BookViewModel> LatestBooks { get; set; } = new List<BookViewModel>();
        public List<FriendViewModel> LatestFriends { get; set; } = new List<FriendViewModel>();
    }

    public class BookViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }

    public class FriendViewModel
    {
        public string Name { get; set; }
        public string LastActive { get; set; }

        public string? Avatar { get; set; }

        public string? AvatarImagePath { get; set; }
    }
}
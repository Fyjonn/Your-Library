using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization; 
using YourLibrary.Models;
using YourLibrary.Data;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace YourLibrary.Controllers
{
    [Authorize] 
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment environment, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var latestFriends = await _context.Friendships.Include(x => x.Requester).Include(x => x.Receiver).Where(x =>
            x.FriendStatus == EnumFriendStatus.Accepted && (x.RequesterId == user.Id || x.ReceiverId == user.Id)).OrderByDescending(x => x.CreatedAt).Take(3).ToListAsync();

            var latestBooks = await _context.UserBooks.Include(b => b.Book).Where(b => b.ApplicationUserId == user.Id).Where(b => b.ApplicationUserId == user.Id).OrderByDescending(b => b.BookId).Take(3).Select(b => new BookViewModel
        {
            Id = b.Book.BookId,
            Title = b.Book.Title,
            Author = b.Book.Author
        }).ToListAsync();

            var borrowedBooks = await _context.Borrows.Include(b => b.UserBook.Book).Include(b => b.UserBook.ApplicationUser)
                    .Where(b => b.ApplicationUserId == user.Id && b.StatusBorrow == EnumStatusBorrow.Borrowed).OrderByDescending(b => b.BorrowDate).Take(3).Select(b => new BorrowedBookViewModel
                    {
                        Id = b.UserBook.BookId,
                        Title = b.UserBook.Book.Title,
                        OwnerName = b.UserBook.ApplicationUser.DisplayName ?? b.UserBook.ApplicationUser.UserName
                    }).ToListAsync();

            var rentedBooks = await _context.Borrows.Include(b => b.UserBook.Book).Include(b => b.ApplicationUser) 
                .Where(b => b.UserBook.ApplicationUserId == user.Id && b.StatusBorrow == EnumStatusBorrow.Borrowed).OrderByDescending(b => b.BorrowDate).Take(3).Select(b => new RentedBookViewModel
                {
                    Id = b.UserBook.BookId,
                    Title = b.UserBook.Book.Title,
                    BorrowerName = b.ApplicationUser.DisplayName ?? b.ApplicationUser.UserName
                }).ToListAsync();


            var model = new ProfileViewModel
            {
                DisplayName = user.DisplayName ?? "User",
                Email = user.Email ?? "",
                Avatar = string.IsNullOrEmpty(user.Avatar) ? "🌿" : user.Avatar,
                AvatarImagePath = user.AvatarImagePath,
                LatestBooks = latestBooks,
                BorrowedBooks = borrowedBooks,
                RentedBooks = rentedBooks,
                LatestFriends = latestFriends.Select(x =>
                {
                    var friend = x.RequesterId == user.Id ? x.Receiver : x.Requester;
                    return new FriendViewModel
                    {
                        Name = friend.UserName,
                        LastActive = $"Friends since: {x.CreatedAt:dd:MM:yyyy}",
                        Avatar = friend.Avatar,
                        AvatarImagePath = friend.AvatarImagePath
                    };
                })
                .ToList()
            };

         

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                model.LatestBooks = new List<BookViewModel>();
                model.LatestFriends = new List<FriendViewModel>();

                model.StayInEditMode = true;
                model.HasProfileError = true;

                return View(model);
            }

            if(model.DisplayName != user.DisplayName)
            {
                var existingLogin = await _userManager.FindByNameAsync(model.DisplayName);

                if (existingLogin != null)
                {
                    ModelState.AddModelError("", "Login already taken");

                    //model.DisplayName = user.DisplayName;
                    //model.Email = user.Email;
                    //ModelState.Remove("DisplayName");
                    //ModelState.Remove("Email");

                    model.LatestBooks = new List<BookViewModel>();
                    model.LatestFriends = new List<FriendViewModel>();

                    model.StayInEditMode = true;
                    model.HasProfileError = true;

                    return View(model);
                }
            }

            if (model.Email != user.Email)
            {
                var existingEmail = await _userManager.FindByEmailAsync(model.Email);

                if (existingEmail != null)
                {
                    ModelState.AddModelError("", "Login already taken");

                    //model.DisplayName = user.DisplayName;
                    //model.Email = user.Email;
                    //ModelState.Remove("DisplayName");
                    //ModelState.Remove("Email");

                    model.LatestBooks = new List<BookViewModel>();
                    model.LatestFriends = new List<FriendViewModel>();

                    model.StayInEditMode = true;
                    model.HasProfileError = true;

                    return View(model);
                }
            }

            user.DisplayName = model.DisplayName;
            user.UserName = model.DisplayName;
            user.Email = model.Email;
            user.Avatar = model.Avatar;

            if (model.AvatarImage == null && !string.IsNullOrEmpty(model.Avatar))
            {
                if (!string.IsNullOrEmpty(user.AvatarImagePath))
                {
                    var oldImagePath = Path.Combine(
                        _environment.WebRootPath,
                        user.AvatarImagePath.TrimStart('/'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                    user.AvatarImagePath = null;
                }
            }

            if (model.AvatarImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extensions = Path.GetExtension(model.AvatarImage.FileName).ToLower();

                if (!allowedExtensions.Contains(extensions))
                {
                    ModelState.AddModelError("", "Only JPG and PNG files are allowed");
                    model.AvatarImagePath = user.AvatarImagePath;
                    model.Avatar = user.Avatar;
                    model.LatestBooks = new List<BookViewModel>();
                    model.LatestFriends = new List<FriendViewModel>();

                    model.StayInEditMode = true;
                    model.HasProfileError = true;

                    return View(model);
                }

                if (model.AvatarImage.Length > 5 * 1024 * 1024) 
                {
                    ModelState.AddModelError("", "File size cannot exceed 5MB");
                    model.AvatarImagePath = user.AvatarImagePath;
                    model.Avatar = user.Avatar;
                    model.LatestBooks = new List<BookViewModel>();
                    model.LatestFriends = new List<FriendViewModel>();

                    model.StayInEditMode = true;
                    model.HasProfileError = true;

                    return View(model);
                }

                var filename = Guid.NewGuid().ToString() + extensions;
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");

                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, filename);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarImage.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.AvatarImagePath))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, user.AvatarImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                user.AvatarImagePath = "/uploads/avatars/" + filename;
            }

           
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (model.NewPassword == model.ConfirmedPassword)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                }
                else
                {
                    ModelState.AddModelError("", "Passwords do not match");
                    model.LatestBooks = new List<BookViewModel>();
                    model.LatestFriends = new List<FriendViewModel>();

                    model.StayInEditMode = true;
                    model.HasProfileError = true;

                    return View(model);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                model.LatestBooks = new List<BookViewModel>();
                model.LatestFriends = new List<FriendViewModel>();

                model.StayInEditMode = true;
                model.HasProfileError = true;

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization; 
using YourLibrary.Models;

namespace YourLibrary.Controllers
{
    [Authorize] 
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                DisplayName = user.DisplayName ?? "Użytkownik",
                Email = user.Email ?? "",
                Avatar = string.IsNullOrEmpty(user.Avatar) ? "🌿" : user.Avatar,
                AvatarImagePath = user.AvatarImagePath,
                LatestBooks = new List<BookViewModel>(),
                LatestFriends = new List<FriendViewModel>()
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
                return View(model);
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
                    return View(model);
                }

                if (model.AvatarImage.Length > 5 * 1024 * 1024) 
                {
                    ModelState.AddModelError("", "File size cannot exceed 5MB");
                    model.AvatarImagePath = user.AvatarImagePath;
                    model.Avatar = user.Avatar;
                    model.LatestBooks = new List<BookViewModel>();
                    model.LatestFriends = new List<FriendViewModel>();
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
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index");
        }
    }
}
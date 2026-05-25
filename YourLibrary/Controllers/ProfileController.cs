using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using YourLibrary.Models;

namespace YourLibrary.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new ProfileViewModel
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Avatar = string.IsNullOrEmpty(user.Avatar)? "🌿":user.Avatar,
            };

            return View(model); 
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync (User);

            user.DisplayName  = model.DisplayName;
            user.UserName = model.DisplayName;
            user.Email = model.Email;
            user.Avatar = model.Avatar;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (model.NewPassword == model.ConfirmedPassword)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    await _userManager.ResetPasswordAsync(user,token,model.NewPassword);
                }
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Index");
        }
    }
}

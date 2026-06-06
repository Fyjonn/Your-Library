using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YourLibrary.Models;

namespace YourLibrary.Controllers
{
    [Route("Validation")]
    public class ValidationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ValidationController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("CheckEmail")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            return Json(user!=null);
        }

        [HttpGet("CheckLogin")]
        public async Task<IActionResult> CheckLogin(string login)
        {
            var user = await _userManager.FindByNameAsync(login);

            return Json(user != null);
        }

        [HttpGet("CheckProfileLogin")]
        public async Task<IActionResult> CheckProfileLogin(string login)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var user = await _userManager.FindByNameAsync(login);

            if (user == null)
            {
                return Json(false);
            }

            return Json(user.Id != currentUser.Id);
        }

        [HttpGet("CheckProfileEmail")]
        public async Task<IActionResult> CheckProfileEmail(string email)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Json(false);
            }

            return Json(user.Id != currentUser.Id);
        }
    }
}

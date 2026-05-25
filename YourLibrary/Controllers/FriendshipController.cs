using Microsoft.AspNetCore.Mvc;

namespace YourLibrary.Controllers
{
    public class FriendshipController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

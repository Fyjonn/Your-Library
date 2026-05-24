using Microsoft.AspNetCore.Mvc;

namespace YourLibrary.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

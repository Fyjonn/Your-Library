using Microsoft.AspNetCore.Mvc;

namespace YourLibrary.Controllers
{
    public class ShelfController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

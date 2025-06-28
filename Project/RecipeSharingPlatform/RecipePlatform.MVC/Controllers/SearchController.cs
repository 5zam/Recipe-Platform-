using Microsoft.AspNetCore.Mvc;

namespace RecipePlatform.MVC.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

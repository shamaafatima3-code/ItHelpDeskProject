using Microsoft.AspNetCore.Mvc;

namespace ItHelpDesk.Controllers
{
    public class UserRolesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

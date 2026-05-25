using ItHelpDesk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Controllers
{
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();

            return View(logs);
        }
    }
}
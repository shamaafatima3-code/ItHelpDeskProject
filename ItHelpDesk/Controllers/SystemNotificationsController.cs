using ItHelpDesk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
namespace ItHelpDesk.Controllers
{
    [Authorize(Roles = "Admin,IT Support Agent")]
    public class SystemNotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SystemNotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var notifications = await _context.SystemNotifications
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return View(notifications);
        }
        public async Task<IActionResult> MarkAsRead(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.SystemNotifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;

            _context.Update(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
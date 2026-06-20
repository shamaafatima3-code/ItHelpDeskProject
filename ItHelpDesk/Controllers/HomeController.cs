using ItHelpDesk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ItHelpDesk.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalTickets = await _context.SupportTickets.CountAsync();
            ViewBag.OpenTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Open");
            ViewBag.ResolvedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Resolved");
            ViewBag.PendingTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Pending");

            ViewBag.CriticalTickets = await _context.SupportTickets.CountAsync(t => t.Priority == "Critical");
            ViewBag.TotalAttachments = await _context.TicketAttachments.CountAsync();
            ViewBag.UnreadNotifications = await _context.SystemNotifications.CountAsync(n => n.IsRead == false);

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
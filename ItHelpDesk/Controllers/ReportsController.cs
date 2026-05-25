using ItHelpDesk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalTickets = await _context.SupportTickets.CountAsync();
            ViewBag.OpenTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Open");
            ViewBag.InProgressTickets = await _context.SupportTickets.CountAsync(t => t.Status == "In Progress");
            ViewBag.PendingTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Pending");
            ViewBag.ResolvedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Resolved");
            ViewBag.ClosedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Closed");

            ViewBag.CriticalTickets = await _context.SupportTickets.CountAsync(t => t.Priority == "Critical");
            ViewBag.HighTickets = await _context.SupportTickets.CountAsync(t => t.Priority == "High");
            ViewBag.MediumTickets = await _context.SupportTickets.CountAsync(t => t.Priority == "Medium");
            ViewBag.LowTickets = await _context.SupportTickets.CountAsync(t => t.Priority == "Low");

            return View();
        }
        public async Task<IActionResult> Print()
        {
            ViewBag.TotalTickets = await _context.SupportTickets.CountAsync();
            ViewBag.OpenTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Open");
            ViewBag.InProgressTickets = await _context.SupportTickets.CountAsync(t => t.Status == "In Progress");
            ViewBag.PendingTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Pending");
            ViewBag.ResolvedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Resolved");
            ViewBag.ClosedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Closed");

            return View();
        }
    }
}
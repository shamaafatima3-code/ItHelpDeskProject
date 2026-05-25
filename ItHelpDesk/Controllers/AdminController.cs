using ItHelpDesk.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalTickets = await _context.SupportTickets.CountAsync();
            ViewBag.TotalComments = await _context.TicketComments.CountAsync();
            ViewBag.TotalAttachments = await _context.TicketAttachments.CountAsync();
            ViewBag.TotalNotifications = await _context.SystemNotifications.CountAsync();

            return View();
        }
    }
}
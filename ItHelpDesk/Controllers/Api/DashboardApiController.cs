using ItHelpDesk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Controllers.Api
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var data = new
            {
                totalTickets = await _context.SupportTickets.CountAsync(),
                openTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Open"),
                pendingTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Pending"),
                resolvedTickets = await _context.SupportTickets.CountAsync(t => t.Status == "Resolved"),
                criticalTickets = await _context.SupportTickets.CountAsync(t => t.Priority == "Critical"),
                totalAttachments = await _context.TicketAttachments.CountAsync(),
                unreadNotifications = await _context.SystemNotifications.CountAsync(n => !n.IsRead)
            };

            return Ok(data);
        }
    }
}
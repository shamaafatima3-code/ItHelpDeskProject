using ItHelpDesk.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Controllers.Api
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await _context.SystemNotifications
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.Type,
                    n.IsRead,
                    n.CreatedDate
                })
                .ToListAsync();

            return Ok(notifications);
        }
    }
}
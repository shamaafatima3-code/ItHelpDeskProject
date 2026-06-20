using ItHelpDesk.Data;
using ItHelpDesk.Hubs;
using ItHelpDesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ItHelpDesk.Controllers
{
    [Authorize(Roles = "Admin,IT Support Agent,Employee")]
    public class SupportTicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public SupportTicketsController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter, string priorityFilter)
        {
            var tickets = _context.SupportTickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                tickets = tickets.Where(t =>
                    t.Title.Contains(searchString) ||
                    t.Category.Contains(searchString) ||
                    t.Status.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
                tickets = tickets.Where(t => t.Status == statusFilter);

            if (!string.IsNullOrWhiteSpace(priorityFilter))
                tickets = tickets.Where(t => t.Priority == priorityFilter);

            return View(await tickets.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supporttickets = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (supporttickets == null) return NotFound();

            ViewBag.Comments = await _context.TicketComments
                .Where(c => c.SupportTicketsId == id)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            ViewBag.Attachments = await _context.TicketAttachments
                .Where(a => a.SupportTicketsId == id)
                .OrderByDescending(a => a.UploadedDate)
                .ToListAsync();

            ViewBag.AIRecommendation = await _context.AIRecommendations
                .FirstOrDefaultAsync(a => a.SupportTicketsId == id);

            ViewBag.InternalNotes = await _context.InternalNotes
                .Where(n => n.SupportTicketsId == id)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            ViewBag.StatusTimeline = await _context.TicketStatusTimelines
                .Where(s => s.SupportTicketsId == id)
                .OrderByDescending(s => s.ChangedDate)
                .ToListAsync();

            return View(supporttickets);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Title,Description,Category,Priority,Status,CreatedDate,AssignedTo")] SupportTickets supporttickets,
            IFormFile? attachmentFile)
        {
            if (!ModelState.IsValid)
                return View(supporttickets);

            _context.SupportTickets.Add(supporttickets);
            await _context.SaveChangesAsync();

            var notification = new SystemNotification
            {
                Message = "New ticket created: " + supporttickets.Title,
                Type = "Ticket Created",
                IsRead = false,
                CreatedDate = DateTime.Now
            };

            _context.SystemNotifications.Add(notification);

            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Ticket Created",
                UserName = GetCurrentUser(),
                Details = "Created ticket: " + supporttickets.Title,
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification.Message);

            await CreateAIRecommendation(supporttickets);

            if (attachmentFile != null && attachmentFile.Length > 0)
            {
                await SaveAttachment(supporttickets.Id, attachmentFile);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var supporttickets = await _context.SupportTickets.FindAsync(id);

            if (supporttickets == null) return NotFound();

            return View(supporttickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int? id,
            [Bind("Id,Title,Description,Category,Priority,Status,CreatedDate,AssignedTo")] SupportTickets supporttickets)
        {
            if (id != supporttickets.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(supporttickets);

            var oldTicket = await _context.SupportTickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == supporttickets.Id);

            if (oldTicket != null && oldTicket.Status != supporttickets.Status)
            {
                _context.TicketStatusTimelines.Add(new TicketStatusTimeline
                {
                    SupportTicketsId = supporttickets.Id,
                    OldStatus = oldTicket.Status,
                    NewStatus = supporttickets.Status,
                    ChangedBy = GetCurrentUser(),
                    ChangedDate = DateTime.Now
                });
            }

            try
            {
                _context.Update(supporttickets);

                _context.ActivityLogs.Add(new ActivityLog
                {
                    Action = "Ticket Updated",
                    UserName = GetCurrentUser(),
                    Details = "Updated ticket: " + supporttickets.Title,
                    CreatedDate = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupportTicketsExists(supporttickets.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var supporttickets = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (supporttickets == null) return NotFound();

            return View(supporttickets);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null) return NotFound();

            var supporttickets = await _context.SupportTickets.FindAsync(id);

            if (supporttickets == null) return NotFound();

            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Ticket Deleted",
                UserName = GetCurrentUser(),
                Details = "Deleted ticket: " + supporttickets.Title,
                CreatedDate = DateTime.Now
            });

            _context.SupportTickets.Remove(supporttickets);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int supportTicketsId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
                return RedirectToAction("Details", new { id = supportTicketsId });

            _context.TicketComments.Add(new TicketComment
            {
                SupportTicketsId = supportTicketsId,
                CommentText = commentText,
                CreatedBy = GetCurrentUser(),
                CreatedDate = DateTime.Now
            });

            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Comment Added",
                UserName = GetCurrentUser(),
                Details = "Added comment to ticket ID: " + supportTicketsId,
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = supportTicketsId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInternalNote(int supportTicketsId, string noteText)
        {
            if (string.IsNullOrWhiteSpace(noteText))
                return RedirectToAction("Details", new { id = supportTicketsId });

            _context.InternalNotes.Add(new InternalNote
            {
                SupportTicketsId = supportTicketsId,
                NoteText = noteText,
                CreatedBy = GetCurrentUser(),
                CreatedDate = DateTime.Now
            });

            _context.ActivityLogs.Add(new ActivityLog
            {
                Action = "Internal Note Added",
                UserName = GetCurrentUser(),
                Details = "Added internal note to ticket ID: " + supportTicketsId,
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = supportTicketsId });
        }

        private async Task SaveAttachment(int ticketId, IFormFile attachmentFile)
        {
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(attachmentFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await attachmentFile.CopyToAsync(fileStream);
            }

            _context.TicketAttachments.Add(new TicketAttachment
            {
                SupportTicketsId = ticketId,
                FileName = attachmentFile.FileName,
                FilePath = "/uploads/" + uniqueFileName,
                UploadedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        private async Task CreateAIRecommendation(SupportTickets supporttickets)
        {
            string suggestedCategory = supporttickets.Category;
            string suggestedPriority = supporttickets.Priority;
            string suggestedReply = "Please check the issue details and assign it to the correct support agent.";

            string description = supporttickets.Description ?? "";

            if (description.Contains("server", StringComparison.OrdinalIgnoreCase) ||
                description.Contains("down", StringComparison.OrdinalIgnoreCase))
            {
                suggestedPriority = "Critical";
                suggestedReply = "This may be a critical system issue. Please assign it immediately to the IT Support Agent.";
            }
            else if (description.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                     description.Contains("login", StringComparison.OrdinalIgnoreCase))
            {
                suggestedCategory = "Authentication";
                suggestedReply = "Ask the user to reset the password or verify login credentials.";
            }
            else if (description.Contains("printer", StringComparison.OrdinalIgnoreCase))
            {
                suggestedCategory = "Hardware";
                suggestedReply = "Check printer connection, drivers, and network availability.";
            }

            _context.AIRecommendations.Add(new AIRecommendation
            {
                SupportTicketsId = supporttickets.Id,
                SuggestedCategory = suggestedCategory,
                SuggestedPriority = suggestedPriority,
                SuggestedReply = suggestedReply,
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        private string GetCurrentUser()
        {
            return User.Identity != null && User.Identity.IsAuthenticated
                ? User.Identity.Name ?? "User"
                : "Guest";
        }

        private bool SupportTicketsExists(int? id)
        {
            return _context.SupportTickets.Any(e => e.Id == id);
        }
    }
}
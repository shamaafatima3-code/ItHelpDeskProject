using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ItHelpDesk.Models;
using ItHelpDesk.Data;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ItHelpDesk.Controllers
{
    public class SupportTicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupportTicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter, string priorityFilter)
        {
            var tickets = from t in _context.SupportTickets
                          select t;

            if (!string.IsNullOrEmpty(searchString))
            {
                tickets = tickets.Where(t =>
                    t.Title.Contains(searchString) ||
                    t.Category.Contains(searchString) ||
                    t.Status.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                tickets = tickets.Where(t => t.Status == statusFilter);
            }

            if (!string.IsNullOrEmpty(priorityFilter))
            {
                tickets = tickets.Where(t => t.Priority == priorityFilter);
            }

            return View(await tickets.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supporttickets = await _context.SupportTickets
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supporttickets == null)
            {
                return NotFound();
            }

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

            return View(supporttickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int supportTicketsId, string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                return RedirectToAction("Details", new { id = supportTicketsId });
            }

            var comment = new TicketComment
            {
                SupportTicketsId = supportTicketsId,
                CommentText = commentText,
                CreatedBy = User.Identity != null && User.Identity.IsAuthenticated
                    ? User.Identity.Name
                    : "Guest",
                CreatedDate = DateTime.Now
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = supportTicketsId });
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
            if (ModelState.IsValid)
            {
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
                await _context.SaveChangesAsync();

                string suggestedCategory = supporttickets.Category;
                string suggestedPriority = supporttickets.Priority;
                string suggestedReply = "Please check the issue details and assign it to the correct support agent.";

                if (supporttickets.Description.Contains("server", StringComparison.OrdinalIgnoreCase) ||
                    supporttickets.Description.Contains("down", StringComparison.OrdinalIgnoreCase))
                {
                    suggestedPriority = "Critical";
                    suggestedReply = "This may be a critical system issue. Please assign it immediately to the IT Support Agent.";
                }
                else if (supporttickets.Description.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                         supporttickets.Description.Contains("login", StringComparison.OrdinalIgnoreCase))
                {
                    suggestedCategory = "Authentication";
                    suggestedReply = "Ask the user to reset the password or verify login credentials.";
                }
                else if (supporttickets.Description.Contains("printer", StringComparison.OrdinalIgnoreCase))
                {
                    suggestedCategory = "Hardware";
                    suggestedReply = "Check printer connection, drivers, and network availability.";
                }

                var aiRecommendation = new AIRecommendation
                {
                    SupportTicketsId = supporttickets.Id,
                    SuggestedCategory = suggestedCategory,
                    SuggestedPriority = suggestedPriority,
                    SuggestedReply = suggestedReply,
                    CreatedDate = DateTime.Now
                };

                _context.AIRecommendations.Add(aiRecommendation);
                await _context.SaveChangesAsync();

                if (attachmentFile != null && attachmentFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + attachmentFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await attachmentFile.CopyToAsync(fileStream);
                    }

                    var attachment = new TicketAttachment
                    {
                        SupportTicketsId = supporttickets.Id,
                        FileName = attachmentFile.FileName,
                        FilePath = "/uploads/" + uniqueFileName,
                        UploadedDate = DateTime.Now
                    };

                    _context.TicketAttachments.Add(attachment);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            var createLog = new ActivityLog
            {
                Action = "Ticket Created",
                UserName = User.Identity != null && User.Identity.IsAuthenticated
        ? User.Identity.Name
        : "Guest",
                Details = "Created ticket: " + supporttickets.Title,
                CreatedDate = DateTime.Now
            };

            _context.ActivityLogs.Add(createLog);
            await _context.SaveChangesAsync();

            return View(supporttickets);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supporttickets = await _context.SupportTickets.FindAsync(id);

            if (supporttickets == null)
            {
                return NotFound();
            }

            return View(supporttickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int? id,
            [Bind("Id,Title,Description,Category,Priority,Status,CreatedDate,AssignedTo")] SupportTickets supporttickets)
        {
            if (id != supporttickets.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supporttickets);
                    await _context.SaveChangesAsync();
                    var editLog = new ActivityLog
                    {
                        Action = "Ticket Updated",
                        UserName = User.Identity != null && User.Identity.IsAuthenticated
        ? User.Identity.Name
        : "Guest",
                        Details = "Updated ticket: " + supporttickets.Title,
                        CreatedDate = DateTime.Now
                    };

                    _context.ActivityLogs.Add(editLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupportTicketsExists(supporttickets.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(supporttickets);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supporttickets = await _context.SupportTickets
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supporttickets == null)
            {
                return NotFound();
            }

            return View(supporttickets);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var supporttickets = await _context.SupportTickets.FindAsync(id);
            var deleteLog = new ActivityLog
            {
                Action = "Ticket Deleted",
                UserName = User.Identity != null && User.Identity.IsAuthenticated
      ? User.Identity.Name
      : "Guest",
                Details = "Deleted ticket: " + supporttickets.Title,
                CreatedDate = DateTime.Now
            };

            _context.ActivityLogs.Add(deleteLog);
            if (supporttickets != null)
            {
                _context.SupportTickets.Remove(supporttickets);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SupportTicketsExists(int? id)
        {
            return _context.SupportTickets.Any(e => e.Id == id);
        }
    }
}
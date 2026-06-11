using System;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Models
{
    public class TicketStatusTimeline
    {
        public int Id { get; set; }

        [Required]
        public int SupportTicketsId { get; set; }

        public SupportTickets? SupportTicket { get; set; }

        public string? OldStatus { get; set; }

        [Required]
        public string NewStatus { get; set; }

        public string? ChangedBy { get; set; }

        public DateTime ChangedDate { get; set; } = DateTime.Now;
    }
}
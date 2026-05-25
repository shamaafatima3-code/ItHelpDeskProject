using System;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        public int SupportTicketsId { get; set; }

        public SupportTickets? SupportTicket { get; set; }

        [Required]
        [StringLength(1000)]
        public string CommentText { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
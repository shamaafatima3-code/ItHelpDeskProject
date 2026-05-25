using System;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Models
{
    public class AIRecommendation
    {
        public int Id { get; set; }

        [Required]
        public int SupportTicketsId { get; set; }

        public SupportTickets? SupportTicket { get; set; }

        public string? SuggestedCategory { get; set; }

        public string? SuggestedPriority { get; set; }

        public string? SuggestedReply { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
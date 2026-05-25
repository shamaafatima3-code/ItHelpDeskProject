using System;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [Required]
        public int SupportTicketsId { get; set; }

        public SupportTickets? SupportTicket { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.Now;
    }
}
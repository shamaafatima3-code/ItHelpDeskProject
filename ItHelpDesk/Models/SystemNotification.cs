using System;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Models
{
    public class SystemNotification
    {
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        public string? Type { get; set; }

        public string? RecipientEmail { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
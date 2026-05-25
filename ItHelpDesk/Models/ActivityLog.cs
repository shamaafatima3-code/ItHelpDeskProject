using System;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        public string Action { get; set; }

        public string? UserName { get; set; }

        public string? Details { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
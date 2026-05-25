using System;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Models
{
    public class SupportTickets
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Priority { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? AssignedTo { get; set; }
    }
}
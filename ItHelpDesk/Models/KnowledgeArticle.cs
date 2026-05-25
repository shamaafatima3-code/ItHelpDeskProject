using System;
using System.ComponentModel.DataAnnotations;

namespace ItHelpDesk.Models
{
    public class KnowledgeArticle
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Category { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
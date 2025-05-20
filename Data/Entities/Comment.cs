using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public Guid ClassRoomId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        [ForeignKey("ClassRoomId")]
        public virtual ClassRoom? ClassRoom { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}
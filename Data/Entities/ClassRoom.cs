using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Data.Entities;

public enum ClassRoomStatus
{
    Pending,    // Chờ Duyệt
    Approved    // Đã Duyệt
}
public class ClassRoom : IDateTracking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public int TopicId { get; set; }
    public Topic? Topic { get; set; }
    public string? Introduction { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Code { get; set; }
    [Column(TypeName = "decimal(18,0)")]
    public double Price { get; set; }
    public int Students { get; set; }
    public string? UserId { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public ClassRoomStatus Status { get; set; } = ClassRoomStatus.Pending;
    public ICollection<Post>? Posts { get; set; }
    public ICollection<ClassDetail>? ClassDetails { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    // public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
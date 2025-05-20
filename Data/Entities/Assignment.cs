namespace LMS.Data.Entities;

public class Assignment : IDateTracking
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? FileUrl { get; set; }
    public DateTime DueDate { get; set; }
    public Guid ClassRoomId { get; set; }
    public ClassRoom? ClassRoom { get; set; }
    public User? User { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? MaxScore { get; set; } // Điểm tối đa cho bài tập, mặc định là 10
    // Danh sách bài nộp của học viên
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
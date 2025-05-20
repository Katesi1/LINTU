using LMS.Data.Entities;

namespace LMS.ViewModels;

public class ClassRoomViewModel
{
    public ClassRoom? ClassRoom { get; set; }
    public IEnumerable<Post>? Posts { get; set; } = new List<Post>();
    public int MembersCount { get; set; }
    public List<Assignment>? Assignments { get; set; } = new List<Assignment>();
    public List<User>? Participants { get; set; } = new List<User>();
    public List<Lesson>? Lessons { get; set; } = new List<Lesson>();
}
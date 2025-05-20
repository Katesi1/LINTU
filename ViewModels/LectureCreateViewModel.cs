using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using LMS.Data.Entities;

namespace LMS.ViewModels;

public class LectureCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài giảng")]
    [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại nội dung")]
    public LectureContentType ContentType { get; set; } = LectureContentType.TextContent;

    public string? VideoUrl { get; set; }

    public IFormFile? UploadedFile { get; set; }

    public string? TextContent { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thứ tự bài giảng")]
    [Range(1, 1000, ErrorMessage = "Thứ tự phải từ 1 đến 1000")]
    public int Order { get; set; } = 1;

    [Required(ErrorMessage = "Vui lòng nhập thời lượng bài giảng")]
    [Range(1, 1000, ErrorMessage = "Thời lượng phải từ 1 đến 1000 phút")]
    public int DurationMinutes { get; set; } = 10;

    [Required(ErrorMessage = "Vui lòng chọn bài học")]
    public int LessonId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn lớp học")]
    public Guid ClassRoomId { get; set; }

    // For dropdown list of lessons
    public SelectList? LessonList { get; set; }
}
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using LMS.Data;
using LMS.Data.Entities;
using LMS.ViewModels;
using Microsoft.Extensions.Logging;

namespace LMS.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để xem bài giảng
    public class LecturesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LecturesController> _logger;

        public LecturesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<User> userManager, ILogger<LecturesController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Lectures/Watch/5
        public async Task<IActionResult> Watch(int id)
        {
            // Lấy thông tin về lecture hiện tại
            var lecture = await _context.Lectures
                .Include(l => l.Lesson)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            // Lấy thông tin về classroom
            var classroom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Id == lecture.ClassRoomId);

            if (classroom == null)
            {
                return NotFound();
            }

            // Đảm bảo sử dụng _CourseLayout 
            ViewBag._Layout = "_CourseLayout";
            ViewBag.ClassRoomId = classroom.Id;

            // Lấy tất cả các lesson trong khóa học, bao gồm các lecture
            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == lecture.ClassRoomId)
                .OrderBy(l => l.Order)
                .Include(l => l.Lectures.OrderBy(lec => lec.Order))
                .ToListAsync();

            // Lấy danh sách các lecture đã hoàn thành
            var userId = User.Identity?.Name;
            var completedLectures = await _context.CompletedLectures
                .Where(cl => cl.UserId == userId && cl.ClassRoomId == lecture.ClassRoomId)
                .Select(cl => cl.LectureId)
                .ToListAsync();

            // Tính tiến độ hoàn thành khóa học
            int totalLectures = lessons.Sum(l => l.Lectures.Count);
            int progress = totalLectures > 0
                ? (completedLectures.Count * 100) / totalLectures
                : 0;

            // Xác định lecture trước và lecture sau
            Lecture? previousLecture = null;
            Lecture? nextLecture = null;

            // Logic để xác định lecture trước và sau
            bool foundCurrent = false;
            foreach (var lesson in lessons.OrderBy(l => l.Order))
            {
                foreach (var lec in lesson.Lectures.OrderBy(l => l.Order))
                {
                    if (foundCurrent)
                    {
                        nextLecture = lec;
                        break;
                    }

                    if (lec.Id == id)
                    {
                        foundCurrent = true;
                    }
                    else
                    {
                        previousLecture = lec;
                    }
                }

                if (nextLecture != null)
                {
                    break;
                }
            }

            // Tạo viewmodel
            var viewModel = new LectureViewModel
            {
                Lecture = lecture,
                Lesson = lecture.Lesson!,
                ClassRoom = classroom,
                Lessons = lessons,
                CompletedLectures = completedLectures,
                PreviousLecture = previousLecture,
                NextLecture = nextLecture,
                CourseProgress = progress,
                // Example resources
                Resources = new List<ResourceViewModel>
                {
                    new ResourceViewModel
                    {
                        Title = "Lecture Slides",
                        Url = "/files/slides.pdf"
                    },
                    new ResourceViewModel
                    {
                        Title = "Exercise Files",
                        Url = "/files/exercises.zip"
                    }
                }
            };

            // Ghi lại việc xem bài giảng này (có thể làm thông qua AJAX)
            // await LogLectureView(id, userId);

            return View(viewModel);
        }

        // POST: Lectures/MarkAsCompleted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCompleted(int lectureId)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lecture = await _context.Lectures.FindAsync(lectureId);
            if (lecture == null)
            {
                return NotFound();
            }

            // Kiểm tra xem đã đánh dấu hoàn thành chưa
            var completed = await _context.CompletedLectures
                .AnyAsync(cl => cl.LectureId == lectureId && cl.UserId == userId);

            if (!completed)
            {
                // Thêm mới nếu chưa hoàn thành
                _context.CompletedLectures.Add(new CompletedLecture
                {
                    LectureId = lectureId,
                    UserId = userId,
                    ClassRoomId = lecture.ClassRoomId,
                    CompletedDate = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: Lectures/MarkAsNotCompleted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsNotCompleted(int lectureId)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var completedLecture = await _context.CompletedLectures
                .FirstOrDefaultAsync(cl => cl.LectureId == lectureId && cl.UserId == userId);

            if (completedLecture != null)
            {
                _context.CompletedLectures.Remove(completedLecture);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: Lectures/SaveNotes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNotes(int lectureId, string notes)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var lectureNote = await _context.LectureNotes
                .FirstOrDefaultAsync(ln => ln.LectureId == lectureId && ln.UserId == userId);

            if (lectureNote == null)
            {
                // Thêm mới nếu chưa có ghi chú
                var lecture = await _context.Lectures.FindAsync(lectureId);
                if (lecture == null)
                {
                    return NotFound();
                }

                _context.LectureNotes.Add(new LectureNote
                {
                    LectureId = lectureId,
                    UserId = userId,
                    ClassRoomId = lecture.ClassRoomId,
                    Notes = notes,
                    UpdatedDate = DateTime.Now
                });
            }
            else
            {
                // Cập nhật ghi chú nếu đã có
                lectureNote.Notes = notes;
                lectureNote.UpdatedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Helper method to check if user is authorized to modify content
        private async Task<bool> IsAuthorizedToModify(Guid classRoomId)
        {
            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            // Check if user is admin or manager
            if (User.IsInRole("Administrator") || User.IsInRole("Manager"))
            {
                return true;
            }

            // Check if user is the creator of the classroom
            var classroom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Id == classRoomId);

            return classroom != null && classroom.UserId == userId;
        }

        // GET: Lectures/Create
        public async Task<IActionResult> Create(int? lessonId, Guid? classRoomId)
        {
            if (lessonId == null && classRoomId == null)
            {
                return NotFound();
            }

            // Get ClassRoomId if only lessonId is provided
            if (lessonId.HasValue && !classRoomId.HasValue)
            {
                var lesson = await _context.Lessons.FindAsync(lessonId.Value);
                if (lesson != null)
                {
                    classRoomId = lesson.ClassRoomId;
                }
            }

            // Check if user is authorized to create content
            if (!classRoomId.HasValue || !await IsAuthorizedToModify(classRoomId.Value))
            {
                return Forbid();
            }

            var viewModel = new LectureCreateViewModel();

            if (classRoomId.HasValue)
            {
                var classRoom = await _context.ClassRooms.FindAsync(classRoomId);
                if (classRoom == null)
                {
                    return NotFound();
                }

                viewModel.ClassRoomId = classRoomId.Value;

                // Get lessons for dropdown
                var lessons = await _context.Lessons
                    .Where(l => l.ClassRoomId == classRoomId)
                    .OrderBy(l => l.Order)
                    .ToListAsync();

                viewModel.LessonList = new SelectList(lessons, "Id", "Title");

                if (lessonId.HasValue)
                {
                    viewModel.LessonId = lessonId.Value;
                }
                else if (lessons.Any())
                {
                    viewModel.LessonId = lessons.First().Id;
                }
            }

            return View(viewModel);
        }

        // POST: Lectures/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LectureCreateViewModel viewModel)
        {
            // Get the lesson to find ClassRoomId
            var lesson = await _context.Lessons.FindAsync(viewModel.LessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            // Check if user is authorized to create content
            if (!await IsAuthorizedToModify(lesson.ClassRoomId))
            {
                return Forbid();
            }

            // Validate video URL if content type is VideoUrl
            if (viewModel.ContentType == LectureContentType.VideoUrl && string.IsNullOrWhiteSpace(viewModel.VideoUrl))
            {
                ModelState.AddModelError("VideoUrl", "Video URL is required when Video URL content type is selected");
            }

            // Validate uploaded file if content type is UploadedFile
            if (viewModel.ContentType == LectureContentType.UploadedFile && viewModel.UploadedFile == null)
            {
                ModelState.AddModelError("UploadedFile", "File upload is required when Upload File content type is selected");
            }

            if (ModelState.IsValid)
            {
                var lecture = new Lecture
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Order = viewModel.Order,
                    LessonId = viewModel.LessonId,
                    ClassRoomId = lesson.ClassRoomId,
                    ContentType = viewModel.ContentType,
                    DurationMinutes = viewModel.DurationMinutes,
                    CreateDate = DateTime.Now
                };

                // Handle content based on type
                if (viewModel.ContentType == LectureContentType.VideoUrl)
                {
                    lecture.VideoUrl = viewModel.VideoUrl?.Trim();
                    lecture.FileUrl = null;
                    lecture.TextContent = null;
                }
                else if (viewModel.ContentType == LectureContentType.UploadedFile && viewModel.UploadedFile != null)
                {
                    // Save the uploaded file
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.UploadedFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewModel.UploadedFile.CopyToAsync(fileStream);
                    }

                    lecture.FileUrl = "/uploads/lectures/" + uniqueFileName;
                    lecture.VideoUrl = null;
                    lecture.TextContent = null;
                }
                else if (viewModel.ContentType == LectureContentType.TextContent)
                {
                    lecture.TextContent = viewModel.TextContent;
                    lecture.VideoUrl = null;
                    lecture.FileUrl = null;
                }

                _context.Add(lecture);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ClassRooms", new { id = lesson.ClassRoomId });
            }

            // If we got this far, something failed, redisplay form
            if (viewModel.ClassRoomId != Guid.Empty)
            {
                var lessons = await _context.Lessons
                    .Where(l => l.ClassRoomId == viewModel.ClassRoomId)
                    .OrderBy(l => l.Order)
                    .ToListAsync();

                viewModel.LessonList = new SelectList(lessons, "Id", "Title", viewModel.LessonId);
            }

            return View(viewModel);
        }

        // GET: Lectures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures
                .Include(l => l.Lesson)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            // Check if user is authorized to edit content
            if (!await IsAuthorizedToModify(lecture.ClassRoomId))
            {
                return Forbid();
            }

            var viewModel = new LectureEditViewModel
            {
                Id = lecture.Id,
                Title = lecture.Title,
                Description = lecture.Description,
                Order = lecture.Order,
                LessonId = lecture.LessonId,
                ClassRoomId = lecture.ClassRoomId,
                ContentType = lecture.ContentType,
                VideoUrl = lecture.VideoUrl,
                FileUrl = lecture.FileUrl,
                TextContent = lecture.TextContent,
                DurationMinutes = lecture.DurationMinutes
            };

            // Get lessons for dropdown
            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == lecture.ClassRoomId)
                .OrderBy(l => l.Order)
                .ToListAsync();

            viewModel.LessonList = new SelectList(lessons, "Id", "Title", lecture.LessonId);

            return View(viewModel);
        }

        // POST: Lectures/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LectureEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null)
            {
                return NotFound();
            }

            // Check if user is authorized to edit content
            if (!await IsAuthorizedToModify(lecture.ClassRoomId))
            {
                return Forbid();
            }

            // Debug info
            Console.WriteLine($"Content Type: {viewModel.ContentType}");
            Console.WriteLine($"Video URL: {viewModel.VideoUrl}");

            // Validate video URL if content type is VideoUrl
            if (viewModel.ContentType == LectureContentType.VideoUrl && string.IsNullOrWhiteSpace(viewModel.VideoUrl))
            {
                ModelState.AddModelError("VideoUrl", "Video URL is required when Video URL content type is selected");
            }

            // Validate uploaded file if content type is UploadedFile and no existing file
            if (viewModel.ContentType == LectureContentType.UploadedFile &&
                viewModel.UploadedFile == null && string.IsNullOrEmpty(viewModel.FileUrl))
            {
                ModelState.AddModelError("UploadedFile", "File upload is required when Upload File content type is selected and no existing file");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    lecture.Title = viewModel.Title;
                    lecture.Description = viewModel.Description;
                    lecture.Order = viewModel.Order;
                    lecture.LessonId = viewModel.LessonId;
                    lecture.ContentType = viewModel.ContentType;
                    lecture.DurationMinutes = viewModel.DurationMinutes;
                    lecture.LastModifiedDate = DateTime.Now;

                    // Handle content based on type
                    if (viewModel.ContentType == LectureContentType.VideoUrl)
                    {
                        // Ensure the video URL is saved
                        lecture.VideoUrl = viewModel.VideoUrl?.Trim();
                        lecture.FileUrl = null;
                        lecture.TextContent = null;

                        Console.WriteLine($"Saving video URL: {lecture.VideoUrl}");
                    }
                    else if (viewModel.ContentType == LectureContentType.UploadedFile && viewModel.UploadedFile != null)
                    {
                        // Delete old file if exists
                        if (!string.IsNullOrEmpty(lecture.FileUrl))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, lecture.FileUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save the new uploaded file
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "lectures");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.UploadedFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await viewModel.UploadedFile.CopyToAsync(fileStream);
                        }

                        lecture.FileUrl = "/uploads/lectures/" + uniqueFileName;
                        lecture.VideoUrl = null;
                        lecture.TextContent = null;
                    }
                    else if (viewModel.ContentType == LectureContentType.TextContent)
                    {
                        lecture.TextContent = viewModel.TextContent;
                        lecture.VideoUrl = null;
                        lecture.FileUrl = null;
                    }

                    _context.Update(lecture);
                    await _context.SaveChangesAsync();

                    // Get the lesson to redirect back to the lessons page
                    var lesson = await _context.Lessons.FindAsync(lecture.LessonId);
                    if (lesson == null)
                    {
                        return NotFound();
                    }

                    return RedirectToAction("Details", "ClassRooms", new { id = lesson.ClassRoomId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LectureExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == viewModel.ClassRoomId)
                .OrderBy(l => l.Order)
                .ToListAsync();

            viewModel.LessonList = new SelectList(lessons, "Id", "Title", viewModel.LessonId);

            return View(viewModel);
        }

        // GET: Lectures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures
                .Include(l => l.Lesson)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            return View(lecture);
        }

        // GET: Lectures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecture = await _context.Lectures
                .Include(l => l.Lesson)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lecture == null)
            {
                return NotFound();
            }

            // Check if user is authorized to delete content
            if (!await IsAuthorizedToModify(lecture.ClassRoomId))
            {
                return Forbid();
            }

            return View(lecture);
        }

        // POST: Lectures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture == null)
            {
                return NotFound();
            }

            // Check if user is authorized to delete content
            if (!await IsAuthorizedToModify(lecture.ClassRoomId))
            {
                return Forbid();
            }

            // Delete file if exists
            if (!string.IsNullOrEmpty(lecture.FileUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, lecture.FileUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            // Get the lesson to redirect back to the lessons page
            var lesson = await _context.Lessons.FindAsync(lecture.LessonId);
            if (lesson == null)
            {
                return NotFound();
            }

            var classRoomId = lesson.ClassRoomId;

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ClassRooms", new { id = classRoomId });
        }

        private bool LectureExists(int id)
        {
            return _context.Lectures.Any(e => e.Id == id);
        }

        // GET: Lectures/GetLectures
        [HttpGet]
        public async Task<IActionResult> GetLectures(int lessonId)
        {
            try
            {
                if (lessonId <= 0)
                {
                    return Json(new { success = false, message = "ID bài học không hợp lệ" });
                }

                // Kiểm tra bài học tồn tại
                var lesson = await _context.Lessons.FindAsync(lessonId);
                if (lesson == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy bài học" });
                }

                // Lấy danh sách bài giảng của bài học
                var lectures = await _context.Lectures
                    .Where(l => l.LessonId == lessonId)
                    .OrderBy(l => l.Order)
                    .Select(l => new
                    {
                        id = l.Id,
                        title = l.Title,
                        description = l.Description,
                        videoUrl = l.VideoUrl,
                        fileUrl = l.FileUrl,
                        contentType = l.ContentType,
                        durationMinutes = l.DurationMinutes,
                        order = l.Order,
                        lessonId = l.LessonId
                    })
                    .ToListAsync();

                return Json(new { success = true, data = lectures });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bài giảng cho bài học {LessonId}", lessonId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải bài giảng" });
            }
        }
    }
}
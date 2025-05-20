using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using LMS.Data;
using LMS.Data.Entities;
using LMS.ViewModels;
using Microsoft.Extensions.Logging;

namespace LMS.Controllers
{
    [Authorize]
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LessonsController> _logger;

        public LessonsController(ApplicationDbContext context, UserManager<User> userManager, ILogger<LessonsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
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

        // GET: Lessons
        public async Task<IActionResult> Index(Guid? classRoomId)
        {
            if (classRoomId == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRooms
                .FirstOrDefaultAsync(c => c.Id == classRoomId);

            if (classRoom == null)
            {
                return NotFound();
            }

            var lessons = await _context.Lessons
                .Where(l => l.ClassRoomId == classRoomId)
                .OrderBy(l => l.Order)
                .Include(l => l.Lectures.OrderBy(lec => lec.Order))
                .ToListAsync();

            var viewModel = new LessonsIndexViewModel
            {
                ClassRoom = classRoom,
                Lessons = lessons
            };

            return View(viewModel);
        }

        // GET: Lessons/Create
        public async Task<IActionResult> Create(Guid classRoomId)
        {
            // Check if user is authorized to create content
            if (!await IsAuthorizedToModify(classRoomId))
            {
                return Forbid();
            }

            var viewModel = new LessonCreateViewModel
            {
                ClassRoomId = classRoomId
            };
            return View(viewModel);
        }

        // POST: Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonCreateViewModel viewModel)
        {
            // Check if user is authorized to create content
            if (!await IsAuthorizedToModify(viewModel.ClassRoomId))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                var lesson = new Lesson
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    Order = viewModel.Order,
                    ClassRoomId = viewModel.ClassRoomId,
                    CreateDate = DateTime.Now
                };

                _context.Add(lesson);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ClassRooms", new { id = viewModel.ClassRoomId });
            }
            return View(viewModel);
        }

        // GET: Lessons/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            // Check if user is authorized to edit content
            if (!await IsAuthorizedToModify(lesson.ClassRoomId))
            {
                return Forbid();
            }

            var viewModel = new LessonCreateViewModel
            {
                Title = lesson.Title,
                Description = lesson.Description,
                Order = lesson.Order,
                ClassRoomId = lesson.ClassRoomId
            };

            return View(viewModel);
        }

        // POST: Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LessonCreateViewModel viewModel)
        {
            // Check if user is authorized to edit content
            if (!await IsAuthorizedToModify(viewModel.ClassRoomId))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var lesson = await _context.Lessons.FindAsync(id);
                    if (lesson == null)
                    {
                        return NotFound();
                    }

                    lesson.Title = viewModel.Title;
                    lesson.Description = viewModel.Description;
                    lesson.Order = viewModel.Order;
                    lesson.LastModifiedDate = DateTime.Now;

                    _context.Update(lesson);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "ClassRooms", new { id = viewModel.ClassRoomId });
            }
            return View(viewModel);
        }

        // GET: Lessons/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.ClassRoom)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            // Check if user is authorized to delete content
            if (!await IsAuthorizedToModify(lesson.ClassRoomId))
            {
                return Forbid();
            }

            return View(lesson);
        }

        // POST: Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            // Check if user is authorized to delete content
            if (!await IsAuthorizedToModify(lesson.ClassRoomId))
            {
                return Forbid();
            }

            var classRoomId = lesson.ClassRoomId;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ClassRooms", new { id = classRoomId });
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.Id == id);
        }

        // POST: Lessons/CreateAjax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax(LessonCreateViewModel viewModel)
        {
            // Check if user is authorized to create content
            if (!await IsAuthorizedToModify(viewModel.ClassRoomId))
            {
                return Json(new { success = false, message = "You are not authorized to create lessons in this classroom." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tìm thứ tự cao nhất hiện tại để thêm bài học mới vào cuối
                    int maxOrder = 0;
                    var lessons = await _context.Lessons
                        .Where(l => l.ClassRoomId == viewModel.ClassRoomId)
                        .ToListAsync();

                    if (lessons.Any())
                    {
                        maxOrder = lessons.Max(l => l.Order);
                    }

                    var lesson = new Lesson
                    {
                        Title = viewModel.Title,
                        Description = viewModel.Description,
                        Order = maxOrder + 1, // Thêm vào cuối
                        ClassRoomId = viewModel.ClassRoomId,
                        CreateDate = DateTime.Now
                    };

                    _context.Add(lesson);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Bài học đã được tạo thành công!", lessonId = lesson.Id });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi tạo bài học: " + ex.Message });
                }
            }

            // Nếu ModelState không hợp lệ, trả về lỗi
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Dữ liệu không hợp lệ", errors = errors });
        }

        // GET: Lessons/GetLessons
        [HttpGet]
        public async Task<IActionResult> GetLessons(Guid classRoomId)
        {
            try
            {
                if (classRoomId == Guid.Empty)
                {
                    return Json(new { success = false, message = "ID lớp học không hợp lệ" });
                }

                // Kiểm tra lớp học tồn tại
                var classRoom = await _context.ClassRooms.FindAsync(classRoomId);
                if (classRoom == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lớp học" });
                }

                // Lấy danh sách bài học và số lượng bài giảng của mỗi bài học
                var lessons = await _context.Lessons
                    .Where(l => l.ClassRoomId == classRoomId)
                    .OrderBy(l => l.Order)
                    .Select(l => new
                    {
                        id = l.Id,
                        title = l.Title,
                        description = l.Description,
                        order = l.Order,
                        lectureCount = _context.Lectures.Count(lc => lc.LessonId == l.Id),
                        totalDuration = _context.Lectures.Where(lc => lc.LessonId == l.Id).Sum(lc => (int?)lc.DurationMinutes) ?? 0
                    })
                    .ToListAsync();

                return Json(new { success = true, data = lessons });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bài học cho lớp {ClassRoomId}", classRoomId);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải bài học" });
            }
        }

        // GET: Lessons/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lesson = await _context.Lessons
                .Include(l => l.ClassRoom)
                .Include(l => l.Lectures.OrderBy(lec => lec.Order))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (lesson == null)
            {
                return NotFound();
            }

            // Check if user has access to this classroom (is enrolled or is creator)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEnrolled = await _context.ClassDetails
                .AnyAsync(cd => cd.ClassRoomId == lesson.ClassRoomId && cd.UserId == userId);

            var isCreator = lesson.ClassRoom?.UserId == userId;
            var isAdminOrManager = User.IsInRole("Administrator") || User.IsInRole("Manager");

            if (!isEnrolled && !isCreator && !isAdminOrManager)
            {
                return Forbid();
            }

            return View(lesson);
        }
    }
}

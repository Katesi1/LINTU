using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Data.Entities;
using System.Security.Claims;

namespace LMS.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AssignmentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Assignment
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Assignments.Include(a => a.ClassRoom).Include(a => a.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Assignment/Create
        public IActionResult Create()
        {
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRooms, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Assignment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignmentCreateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            var assignment = new Assignment
            {
                ClassRoomId = Guid.Parse(request.ClassRoomId!),  // Convert Guid từ chuỗi
                Title = request.Title,
                Description = request.Description,
                FileUrl = request.FileUrl,  // Lưu đường dẫn file nếu có
                DueDate = DateTime.Parse(request.DueDate!),  // Parse ngày hết hạn
                CreateDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Bài tập đã được tạo thành công!" });
        }

        // GET: Assignment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRooms, "Id", "Id", assignment.ClassRoomId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", assignment.UserId);
            return View(assignment);
        }

        // POST: Assignment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Title,Description,FileUrl,DueDate,ClassRoomId,CreateDate,LastModifiedDate")] Assignment assignment)
        {
            if (id != assignment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRooms, "Id", "Id", assignment.ClassRoomId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", assignment.UserId);
            return View(assignment);
        }

        // GET: Assignment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(a => a.ClassRoom)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment != null)
            {
                _context.Assignments.Remove(assignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.ClassRoom) // Include ClassRoom để tránh lỗi null reference
                .Include(a => a.Submissions) // Lấy danh sách nộp bài
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View("Details", assignment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int AssignmentId, string AnswerText, IFormFile? File)
        {
            var assignment = await _context.Assignments.FindAsync(AssignmentId);
            if (assignment == null)
            {
                return NotFound();
            }

            string filePath = null!;
            if (File != null && File.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "submissions");
                Directory.CreateDirectory(uploadsFolder);
                filePath = Path.Combine(uploadsFolder, File.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await File.CopyToAsync(fileStream);
                }
                filePath = "/submissions/" + File.FileName;
            }

            var submission = new Submission
            {
                AssignmentId = AssignmentId,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                AnswerText = AnswerText,
                FileUrl = filePath,
                SubmitDate = DateTime.UtcNow
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = AssignmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int SubmissionId, int Score, string? Feedback)
        {
            // Kiểm tra quyền truy cập (chỉ giáo viên hoặc admin mới được chấm điểm)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || (!User.IsInRole("Administrator") && !User.IsInRole("Manager")))
            {
                bool isTeacher = false;

                // Kiểm tra xem người dùng có phải là người tạo lớp không
                var submission = await _context.Submissions
                    .Include(s => s.Assignment)
                    .ThenInclude(a => a.ClassRoom)
                    .FirstOrDefaultAsync(s => s.Id == SubmissionId);

                if (submission != null && submission.Assignment != null &&
                    submission.Assignment.ClassRoom != null &&
                    submission.Assignment.ClassRoom.UserId == userId)
                {
                    isTeacher = true;
                }

                if (!isTeacher)
                {
                    return Json(new { success = false, message = "Bạn không có quyền chấm điểm bài tập này." });
                }
            }

            // Tìm bài làm
            var submissionToGrade = await _context.Submissions.FindAsync(SubmissionId);
            if (submissionToGrade == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài làm." });
            }

            // Cập nhật điểm và nhận xét
            submissionToGrade.Score = Score;
            submissionToGrade.Feedback = Feedback;
            submissionToGrade.IsGraded = true;
            submissionToGrade.GradedDate = DateTime.UtcNow;
            submissionToGrade.GradedBy = userId;

            try
            {
                _context.Update(submissionToGrade);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Chấm điểm thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi chấm điểm: {ex.Message}" });
            }
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.Id == id);
        }
    }
}
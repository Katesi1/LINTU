using LMS.Data;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS.Controllers
{
    [Authorize]
    public class LectureNotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public LectureNotesController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LectureNotes/Create
        [HttpPost]
        public async Task<IActionResult> Create(int lectureId, string notes, Guid classRoomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if note already exists
            var existingNote = await _context.LectureNotes
                .FirstOrDefaultAsync(ln => ln.LectureId == lectureId && ln.UserId == userId);

            if (existingNote != null)
            {
                // Update existing note
                existingNote.Notes = notes;
                existingNote.UpdatedDate = DateTime.Now;
                _context.Update(existingNote);
            }
            else
            {
                // Create new note
                var note = new LectureNote
                {
                    LectureId = lectureId,
                    UserId = userId,
                    ClassRoomId = classRoomId,
                    Notes = notes,
                    UpdatedDate = DateTime.Now
                };
                _context.Add(note);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // GET: LectureNotes/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int lectureId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var note = await _context.LectureNotes
                .FirstOrDefaultAsync(ln => ln.LectureId == lectureId && ln.UserId == userId);

            if (note == null)
            {
                return Json(new { exists = false });
            }

            return Json(new { exists = true, notes = note.Notes });
        }

        // POST: LectureNotes/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int lectureId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var note = await _context.LectureNotes
                .FirstOrDefaultAsync(ln => ln.LectureId == lectureId && ln.UserId == userId);

            if (note != null)
            {
                _context.LectureNotes.Remove(note);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Data;
using LMS.Data.Entities;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace LMS.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, Guid classRoomId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return Json(new { success = false, message = "Comment cannot be empty" });
            }

            // Verify the post exists
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            // Verify the classroom exists
            var classroom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Id == classRoomId);
            if (classroom == null)
            {
                return Json(new { success = false, message = "Classroom not found" });
            }

            // Get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            // Create and save comment
            var comment = new Comment
            {
                PostId = postId,
                ClassRoomId = classRoomId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Return comment data for display
            return Json(new
            {
                success = true,
                userImage = user.ImageUrl,
                userName = user.FullName,
                content = comment.Content,
                createdAt = comment.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int postId, Guid classRoomId)
        {
            // Verify the post exists
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return Json(new { success = false, message = "Post not found" });
            }

            // Get all comments for this post
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    id = c.Id,
                    userImage = c.User!.ImageUrl,
                    userName = c.User.FullName,
                    content = c.Content,
                    createdAt = c.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Json(new { success = true, comments });
        }
    }
}
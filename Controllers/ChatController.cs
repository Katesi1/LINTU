using LMS.Data;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rooms = await _context.ChatRooms.ToListAsync();
            return View(rooms);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                TempData["Error"] = "Tên phòng không được để trống!";
                return RedirectToAction("Create");
            }

            var room = new ChatRoom { Name = name };
            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();

            return RedirectToAction("Room", new { id = room.Id });
        }

        public async Task<IActionResult> Room(int id)
        {
            var room = await _context.ChatRooms
                .Include(r => r.Messages)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null) return NotFound();

            return View(room);
        }
    }
}

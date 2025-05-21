using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LMS.Models;
using LMS.Data;
using LMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET: ClassRooms
    public async Task<IActionResult> Index(string searchString, string sortBy, string sortOrder, int page = 1)
    {
        // Define the number of items per page
        int pageSize = 6;

        // Lấy tất cả các lớp học được phê duyệt để tính toán số liệu thống kê
        var allApprovedClassRooms = await _context.ClassRooms
            .Include(c => c.Topic)
            .Where(c => c.Status == ClassRoomStatus.Approved)
            .ToListAsync();

        // Start with the queryable for ClassRooms
        var classRoomsQuery = _context.ClassRooms
            .Include(c => c.Topic)
            .Where(c => c.Status == ClassRoomStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            string searchLower = searchString.ToLower();
            classRoomsQuery = classRoomsQuery.Where(c => c.Name!.ToLower().Contains(searchLower));
        }

        // Lấy số lượng học viên cho tất cả các lớp học
        var studentCounts = await _context.ClassDetails
            .GroupBy(cd => cd.ClassRoomId!)
            .Select(g => new { ClassRoomId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClassRoomId, x => x.Count);

        // Cập nhật số lượng học viên cho tất cả các lớp học
        foreach (var classRoom in allApprovedClassRooms)
        {
            classRoom.Students = studentCounts.ContainsKey(classRoom.Id!) ? studentCounts[classRoom.Id!] : 0;
        }

        // Apply pagination for displayed classes
        var totalItems = await classRoomsQuery.CountAsync();
        if (totalItems == 0)
        {
            ViewBag.NoClassMessage = "Không tìm thấy lớp học nào phù hợp.";
            ViewBag.TotalClasses = allApprovedClassRooms.Count;
            ViewBag.FreeClasses = allApprovedClassRooms.Count(c => c.Price == 0);
            ViewBag.NewClasses = allApprovedClassRooms.Count(c => (DateTime.UtcNow - c.CreateDate).TotalDays <= 7);
            ViewBag.TopicCount = allApprovedClassRooms.Select(c => c.Topic?.Name).Distinct().Count();
            ViewBag.MembersCount = studentCounts;
            return View(new List<ClassRoom>());
        }
        var classRooms = await classRoomsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Update students count for displayed classes
        foreach (var classRoom in classRooms)
        {
            classRoom.Students = studentCounts.ContainsKey(classRoom.Id!) ? studentCounts[classRoom.Id!] : 0;
        }

        classRooms = sortBy switch
        {
            "name" => sortOrder == "asc" ? classRooms.OrderBy(c => c.Name).ToList() : classRooms.OrderByDescending(c => c.Name).ToList(),
            "price" => sortOrder == "asc" ? classRooms.OrderBy(c => c.Price).ToList() : classRooms.OrderByDescending(c => c.Price).ToList(),
            "students" => sortOrder == "asc" ? classRooms.OrderBy(c => c.Students).ToList() : classRooms.OrderByDescending(c => c.Students).ToList(),
            _ => classRooms.OrderByDescending(c => c.CreateDate).ToList(), // Mặc định: Lớp mới nhất lên đầu
        };

        // Truyền tất cả các lớp học cho view để hiển thị thống kê
        ViewBag.TotalClasses = allApprovedClassRooms.Count;
        ViewBag.FreeClasses = allApprovedClassRooms.Count(c => c.Price == 0);
        ViewBag.NewClasses = allApprovedClassRooms.Count(c => (DateTime.UtcNow - c.CreateDate).TotalDays <= 7);
        ViewBag.TopicCount = allApprovedClassRooms.Select(c => c.Topic?.Name).Distinct().Count();
        ViewBag.MembersCount = studentCounts;

        // Tính tổng số học viên trong toàn bộ hệ thống
        ViewBag.TotalStudents = studentCounts.Values.Sum();

        // Create a ViewModel or ViewData for pagination
        ViewBag.PageNumber = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        return View(classRooms);
    }

    [HttpPost]
    public async Task<IActionResult> JoinClass(string code)
    {
        var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(x => x.Code == code);
        if (classRoom != null)
        {
            return RedirectToAction("Introduction", "ClassRooms", new { id = classRoom!.Id });
        }
        return RedirectToAction(nameof(Index), "Home");
    }

    [Route("template")]
    public IActionResult Template()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

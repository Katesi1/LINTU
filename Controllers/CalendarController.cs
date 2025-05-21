using Microsoft.AspNetCore.Mvc;
using LMS.Data.Entities;
using System;
using System.Collections.Generic;
using LMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Globalization;

namespace LMS.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // API để trả về sự kiện
        public JsonResult GetEvents()
        {
            // Lấy các sự kiện từ bảng Events
            var events = _context.Events.Select(e => new
            {
                id = e.Id,
                title = e.Title,
                start = e.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = e.End.ToString("yyyy-MM-ddTHH:mm:ss"),
                color = "#4e73df", // Màu mặc định cho sự kiện chung
                type = "event"
            }).ToList();

            // Lấy các bài giảng (lecture) từ cơ sở dữ liệu
            var lectures = _context.Lectures
                .Include(l => l.Lesson)
                .Select(l => new
                {
                    id = "lecture_" + l.Id,
                    title = l.Title,
                    // Sử dụng ngày tạo làm ngày bắt đầu
                    start = l.CreateDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    // Sử dụng thời lượng để tính thời gian kết thúc
                    end = l.CreateDate.AddMinutes(l.DurationMinutes).ToString("yyyy-MM-ddTHH:mm:ss"),
                    className = l.Lesson.ClassRoomId.ToString(),
                    color = "#36b9cc", // Màu xanh cho bài giảng
                    type = "lecture",
                    description = l.Description ?? "Bài giảng",
                    url = "/Lectures/Details/" + l.Id
                }).ToList();

            // Lấy các bài tập (assignment) từ cơ sở dữ liệu
            var assignments = _context.Assignments
                .Select(a => new
                {
                    id = "assignment_" + a.Id,
                    title = a.Title ?? "Bài tập",
                    // Sử dụng ngày tạo làm ngày bắt đầu
                    start = a.CreateDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    // Sử dụng hạn nộp làm ngày kết thúc
                    end = a.DueDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    className = a.ClassRoomId.ToString(),
                    color = "#1cc88a", // Màu xanh lá nhạt cho bài tập
                    type = "assignment",
                    description = a.Description ?? "Hạn nộp: " + a.DueDate.ToString("dd/MM/yyyy HH:mm"),
                    url = "/Assignments/Details/" + a.Id
                }).ToList();

            // Thêm các ngày lễ trong năm
            var holidays = GetHolidays(DateTime.Now.Year);

            // Kết hợp tất cả các sự kiện
            var allEvents = events
                .Concat(lectures.Cast<object>())
                .Concat(assignments.Cast<object>())
                .Concat(holidays.Cast<object>())
                .ToList();

            return Json(allEvents);
        }

        // API để lưu sự kiện khi kéo và thả
        [HttpPost]
        public JsonResult SaveEvent([FromBody] Event eventData)
        {
            if (eventData != null)
            {
                _context.Events.Add(eventData);
                _context.SaveChanges();
            }

            return Json(new { success = true });
        }

        // Phương thức để lấy các ngày lễ trong năm
        private List<object> GetHolidays(int year)
        {
            var holidays = new List<object>();

            // Các ngày lễ cố định theo dương lịch
            // Tết Dương lịch
            AddHoliday(holidays, year, 1, 1, "Tết Dương lịch", "#e74a3b");

            // Ngày Giải phóng miền Nam và Quốc tế Lao động
            AddHoliday(holidays, year, 4, 30, "Ngày Giải phóng miền Nam", "#e74a3b");
            AddHoliday(holidays, year, 5, 1, "Quốc tế Lao động", "#e74a3b");

            // Quốc khánh
            AddHoliday(holidays, year, 9, 2, "Quốc khánh", "#e74a3b");

            // Giáng sinh
            AddHoliday(holidays, year, 12, 24, "Đêm Giáng sinh", "#e95f71");
            AddHoliday(holidays, year, 12, 25, "Giáng sinh", "#e95f71");

            // Các ngày lễ theo âm lịch (sử dụng lịch dương tương ứng cho năm hiện tại)

            // Tết Nguyên Đán (ước lượng - cần tính toán chính xác theo lịch âm)
            DateTime tetNguyenDan = GetLunarNewYearDate(year);
            for (int i = 0; i < 5; i++) // Nghỉ khoảng 5 ngày Tết
            {
                AddHoliday(holidays, tetNguyenDan.AddDays(i).Year, tetNguyenDan.AddDays(i).Month, tetNguyenDan.AddDays(i).Day,
                    i == 0 ? "Tết Nguyên Đán" : "Nghỉ Tết Nguyên Đán (ngày " + (i + 1) + ")", "#e74a3b");
            }

            // Lễ Giỗ Tổ Hùng Vương (mùng 10 tháng 3 âm lịch - ước lượng)
            DateTime gioToHungVuong = GetApproximateLunarDate(year, 3, 10);
            AddHoliday(holidays, gioToHungVuong.Year, gioToHungVuong.Month, gioToHungVuong.Day, "Giỗ Tổ Hùng Vương", "#e74a3b");

            // Tết Đoan Ngọ (mùng 5 tháng 5 âm lịch)
            DateTime tetDoanNgo = GetApproximateLunarDate(year, 5, 5);
            AddHoliday(holidays, tetDoanNgo.Year, tetDoanNgo.Month, tetDoanNgo.Day, "Tết Đoan Ngọ", "#fd7e14");

            // Lễ Vu Lan (rằm tháng 7 âm lịch)
            DateTime leVuLan = GetApproximateLunarDate(year, 7, 15);
            AddHoliday(holidays, leVuLan.Year, leVuLan.Month, leVuLan.Day, "Lễ Vu Lan", "#fd7e14");

            // Tết Trung Thu (rằm tháng 8 âm lịch)
            DateTime tetTrungThu = GetApproximateLunarDate(year, 8, 15);
            AddHoliday(holidays, tetTrungThu.Year, tetTrungThu.Month, tetTrungThu.Day, "Tết Trung Thu", "#fd7e14");

            // Các ngày lễ khác
            // Ngày Quốc tế Phụ nữ
            AddHoliday(holidays, year, 3, 8, "Quốc tế Phụ nữ", "#fd7e14");

            // Ngày Phụ nữ Việt Nam
            AddHoliday(holidays, year, 10, 20, "Ngày Phụ nữ Việt Nam", "#fd7e14");

            // Ngày Nhà giáo Việt Nam
            AddHoliday(holidays, year, 11, 20, "Ngày Nhà giáo Việt Nam", "#fd7e14");

            // Valentine
            AddHoliday(holidays, year, 2, 14, "Valentine", "#e95f71");

            // Halloween
            AddHoliday(holidays, year, 10, 31, "Halloween", "#e95f71");

            return holidays;
        }

        // Helper để thêm ngày lễ vào danh sách
        private void AddHoliday(List<object> holidays, int year, int month, int day, string name, string color)
        {
            var date = new DateTime(year, month, day);
            holidays.Add(new
            {
                id = "holiday_" + Guid.NewGuid().ToString(),
                title = name,
                start = date.ToString("yyyy-MM-dd"),
                end = date.ToString("yyyy-MM-dd"),
                allDay = true,
                color = color,
                type = "holiday",
                description = "Ngày lễ: " + name,
                display = "background",
                classNames = "holiday-event"
            });

            // Thêm cả sự kiện hiển thị tên ngày lễ
            holidays.Add(new
            {
                id = "holiday_name_" + Guid.NewGuid().ToString(),
                title = name,
                start = date.ToString("yyyy-MM-dd"),
                end = date.ToString("yyyy-MM-dd"),
                allDay = true,
                color = "transparent",
                textColor = color,
                type = "holiday",
                description = "Ngày lễ: " + name
            });
        }

        // Helper để ước tính ngày âm lịch (chỉ là ước lượng, không chính xác hoàn toàn)
        private DateTime GetApproximateLunarDate(int year, int lunarMonth, int lunarDay)
        {
            // Đây chỉ là ước lượng đơn giản - trong thực tế cần sử dụng thư viện lịch âm
            // Các ngày lễ âm lịch thường rơi vào khoảng 1 tháng trước so với dương lịch
            // Lịch âm lịch thường đi sau dương lịch khoảng 1 tháng
            return new DateTime(year, lunarMonth + 1 > 12 ? 1 : lunarMonth + 1, Math.Min(lunarDay, DateTime.DaysInMonth(year, lunarMonth + 1 > 12 ? 1 : lunarMonth + 1)));
        }

        // Helper để ước tính ngày Tết Nguyên Đán (thường rơi vào cuối tháng 1 hoặc tháng 2 dương lịch)
        private DateTime GetLunarNewYearDate(int year)
        {
            // Đây là ước lượng đơn giản - cần thư viện lịch âm chính xác trong thực tế
            if (year == 2024) return new DateTime(2024, 2, 10); // Tết 2024: Mùng 1 âm lịch là 10/02/2024
            if (year == 2025) return new DateTime(2025, 1, 29); // Tết 2025: Mùng 1 âm lịch là 29/01/2025

            // Ước lượng thô cho các năm khác (không chính xác)
            // Khoảng cuối tháng 1 đến giữa tháng 2
            return new DateTime(year, 2, 5);
        }
    }
}

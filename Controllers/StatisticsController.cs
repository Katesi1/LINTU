using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Data;
using LMS.Data.Entities;
using LMS.Data.Entities.Enums;
using LMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[Authorize(Roles = "Administrator")]
public class StatisticsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public StatisticsController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new StatisticsViewModel();

        // Lấy dữ liệu doanh thu theo tháng trong năm hiện tại
        var currentYear = DateTime.Now.Year;
        var revenueData = await GetMonthlyRevenueData(currentYear);
        viewModel.MonthlyRevenue = revenueData;
        viewModel.TotalRevenue = revenueData.Sum(r => r.Revenue);

        // Lấy dữ liệu người dùng theo tháng
        var userData = await GetMonthlyUserData(currentYear);
        viewModel.MonthlyUsers = userData;
        viewModel.TotalUsers = await _context.Users.CountAsync();
        viewModel.NewUsersThisMonth = userData.Last().Count;

        // Lấy dữ liệu lớp học có nhiều người tham gia nhất
        viewModel.TopClassrooms = await GetTopClassrooms(10);

        // Lấy tổng số lớp học
        viewModel.TotalClassrooms = await _context.ClassRooms.CountAsync();

        // Lấy tổng số khóa học và bài giảng
        viewModel.TotalLessons = await _context.Lessons.CountAsync();
        viewModel.TotalLectures = await _context.Lectures.CountAsync();

        return View(viewModel);
    }

    private async Task<List<MonthlyRevenueData>> GetMonthlyRevenueData(int year)
    {
        var depositTransactions = await _context.Transactions
            .Where(t => t.TransactionType == TransactionType.Deposit && t.CreateDate.Year == year)
            .GroupBy(t => new { t.CreateDate.Year, t.CreateDate.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Total = g.Sum(t => t.Amount)
            })
            .ToListAsync();

        var result = new List<MonthlyRevenueData>();

        // Tạo dữ liệu cho 12 tháng
        for (int month = 1; month <= 12; month++)
        {
            var monthData = depositTransactions.FirstOrDefault(t => t.Month == month);
            result.Add(new MonthlyRevenueData
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMM"),
                Revenue = monthData?.Total ?? 0
            });
        }

        return result;
    }

    private async Task<List<MonthlyUserData>> GetMonthlyUserData(int year)
    {
        // Lấy tất cả người dùng và đếm số lượng theo tháng đăng ký
        var users = await _userManager.Users.ToListAsync();

        // Tính toán dựa trên ngày đăng ký (dựa vào các thông tin liên quan)
        var usersData = users
            .GroupBy(u => new
            {
                Year = u.DateOfBirth?.Year ?? DateTime.Now.Year,
                Month = u.DateOfBirth?.Month ?? DateTime.Now.Month
            })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .Where(d => d.Year == year)
            .ToList();

        var result = new List<MonthlyUserData>();

        // Tạo dữ liệu cho 12 tháng
        for (int month = 1; month <= 12; month++)
        {
            var monthData = usersData.FirstOrDefault(t => t.Month == month);
            result.Add(new MonthlyUserData
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMM"),
                Count = monthData?.Count ?? 0
            });
        }

        return result;
    }

    private async Task<List<ClassroomStatData>> GetTopClassrooms(int count)
    {
        return await _context.ClassRooms
            .Select(c => new ClassroomStatData
            {
                Id = c.Id,
                Name = c.Name,
                ParticipantCount = c.ClassDetails.Count,
                Price = c.Price
            })
            .OrderByDescending(c => c.ParticipantCount)
            .Take(count)
            .ToListAsync();
    }
}
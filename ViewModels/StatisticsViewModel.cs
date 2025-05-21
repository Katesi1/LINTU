using System;
using System.Collections.Generic;
using LMS.Data.Entities;

namespace LMS.ViewModels
{
    public class StatisticsViewModel
    {
        public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new List<MonthlyRevenueData>();
        public decimal TotalRevenue { get; set; }

        public List<MonthlyUserData> MonthlyUsers { get; set; } = new List<MonthlyUserData>();
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }

        public List<ClassroomStatData> TopClassrooms { get; set; } = new List<ClassroomStatData>();
        public int TotalClassrooms { get; set; }

        public int TotalLessons { get; set; }
        public int TotalLectures { get; set; }
    }

    public class MonthlyRevenueData
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MonthlyUserData
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Count { get; set; }
    }

    public class ClassroomStatData
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int ParticipantCount { get; set; }
        public double Price { get; set; }
    }
}
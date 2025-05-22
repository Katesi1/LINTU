using LMS.Data;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace LMS.Data
{
    public class TestDataSeeder : DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private const string TestPassword = "Matkhau123@";

        public TestDataSeeder(
            ApplicationDbContext context,
            UserManager<User> userManager) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task SeedTestData()
        {
            // Seed test users
            await SeedTestUsers();

            // Seed additional topics - first make sure we have topics
            await SeedAdditionalTopics();

            // Seed courses by Test1 and Test2
            await SeedTestCourses();
        }

        private async Task SeedTestUsers()
        {
            // Create 10 test users if they don't exist
            for (int i = 1; i <= 10; i++)
            {
                string email = $"Test{i}@gmail.com";

                if (await _userManager.FindByEmailAsync(email) == null)
                {
                    var user = new User
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = $"Test User {i}",
                        DateOfBirth = new DateTime(1990, 1, 1).AddMonths(i),
                        LockoutEnabled = false,
                        PhoneNumber = $"09{i.ToString().PadLeft(8, '0')}"
                    };

                    await _userManager.CreateAsync(user, TestPassword);
                    await _userManager.AddToRoleAsync(user, "User");

                    Console.WriteLine($"Added test user: {email}");
                }
            }
        }

        private async Task SeedAdditionalTopics()
        {
            // First ensure the base topics exist (run parent Seed method if needed)
            if (!_context.Topics.Any())
            {
                Console.WriteLine("No topics found. Running base seed method first...");
                await base.Seed();
            }

            // Only add if the additional topics don't already exist
            var topicNames = new[]
            {
                "PHP",
                "Swift",
                "Kotlin",
                "TypeScript",
                "Rust"
            };

            var existingNames = await _context.Topics.Select(t => t.Name).ToListAsync();

            var topicsToAdd = new List<Topic>();
            int nextId = await _context.Topics.AnyAsync() ?
                         await _context.Topics.MaxAsync(t => t.Id) + 1 : 1;

            for (int i = 0; i < topicNames.Length; i++)
            {
                if (!existingNames.Contains(topicNames[i]))
                {
                    topicsToAdd.Add(new Topic
                    {
                        Id = nextId++,  // Explicitly set ID to make sure it's sequential
                        Name = topicNames[i],
                        Alias = topicNames[i].ToLower(),
                        ImageUrl = $"/images/{topicNames[i].ToLower()}.png",
                        ParentTopicId = 0
                    });
                }
            }

            if (topicsToAdd.Any())
            {
                _context.Topics.AddRange(topicsToAdd);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Added {topicsToAdd.Count} additional topics");
            }

            // Verify all topics
            var allTopics = await _context.Topics.ToListAsync();
            Console.WriteLine($"Total topics in database: {allTopics.Count}");
            foreach (var topic in allTopics)
            {
                Console.WriteLine($"Topic ID: {topic.Id.ToString()}, Name: {topic.Name}");
            }
        }

        private async Task SeedTestCourses()
        {
            var test1User = await _userManager.FindByEmailAsync("Test1@gmail.com");
            var test2User = await _userManager.FindByEmailAsync("Test2@gmail.com");

            if (test1User == null || test2User == null)
            {
                Console.WriteLine("Test1 or Test2 user not found! Please run SeedTestUsers first.");
                return;
            }

            // Check if courses already exist for these users
            var existingCourses = await _context.ClassRooms
                .Where(c => c.UserId == test1User.Id || c.UserId == test2User.Id)
                .ToListAsync();

            if (existingCourses.Count >= 10)
            {
                Console.WriteLine("Test courses already exist.");
                return;
            }

            // Get valid topic IDs from the database
            var validTopicIds = await _context.Topics.Select(t => t.Id).ToListAsync();
            if (!validTopicIds.Any())
            {
                Console.WriteLine("No topics found in the database. Cannot create courses without topics.");
                return;
            }

            Console.WriteLine($"Found {validTopicIds.Count} topics with IDs: {string.Join(", ", validTopicIds.Select(id => id.ToString()))}");

            // Course names
            string[] courseNames = {
                "Modern Web Development",
                "Mobile App Development",
                "Cloud Computing Essentials",
                "Database Management Systems",
                "Artificial Intelligence Basics",
                "DevOps Practices",
                "Blockchain Fundamentals",
                "UI/UX Design Principles",
                "Cybersecurity Fundamentals",
                "IoT Development"
            };

            // Create 5 courses for Test1
            for (int i = 0; i < 5; i++)
            {
                // Pick a valid topic ID from the database
                int topicIndex = i % validTopicIds.Count;
                int topicId = validTopicIds[topicIndex];

                var classroom = new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    Name = courseNames[i],
                    TopicId = topicId,  // Use valid topic ID
                    Introduction = $"Introduction to {courseNames[i]}",
                    Description = $"Comprehensive course on {courseNames[i]} covering all essential aspects and practical applications.",
                    ImageUrl = $"/images/{courseNames[i].ToLower().Replace(" ", "_")}.png",
                    Code = GenerateRandomCode(),
                    Price = (i % 2 == 0) ? 0 : 100000 + (i * 15000), // Some free, some paid
                    Students = 0,
                    Status = ClassRoomStatus.Approved,
                    UserId = test1User.Id,
                    CreateDate = DateTime.Now.AddDays(-i * 2),
                    LastModifiedDate = DateTime.Now
                };

                _context.ClassRooms.Add(classroom);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Added first 5 test courses for Test1");

            // Create 5 courses for Test2
            for (int i = 5; i < 10; i++)
            {
                // Pick a valid topic ID from the database
                int topicIndex = i % validTopicIds.Count;
                int topicId = validTopicIds[topicIndex];

                var classroom = new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    Name = courseNames[i],
                    TopicId = topicId,  // Use valid topic ID
                    Introduction = $"Introduction to {courseNames[i]}",
                    Description = $"Comprehensive course on {courseNames[i]} covering all essential aspects and practical applications.",
                    ImageUrl = $"/images/{courseNames[i].ToLower().Replace(" ", "_")}.png",
                    Code = GenerateRandomCode(),
                    Price = (i % 2 == 0) ? 0 : 120000 + ((i - 5) * 18000), // Some free, some paid
                    Students = 0,
                    Status = ClassRoomStatus.Approved,
                    UserId = test2User.Id,
                    CreateDate = DateTime.Now.AddDays(-i * 2),
                    LastModifiedDate = DateTime.Now
                };

                _context.ClassRooms.Add(classroom);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Added second 5 test courses for Test2");
        }

        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
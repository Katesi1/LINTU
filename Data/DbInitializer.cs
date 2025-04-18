using LMS.Data;
using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public DbInitializer(ApplicationDbContext context,
      UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public static async Task CreateRoles(IServiceProvider serviceProvider, UserManager<User> userManager)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Administrator", "Manager", "User" };

        // Tạo các vai trò nếu chưa tồn tại
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        var user = await userManager.FindByEmailAsync("trungnguyen7358@gmail.com");
        if (user != null)
        {
            await userManager.AddToRoleAsync(user, "Administrator");
        }
    }
    public async Task Seed()
    {
        if (!_userManager.Users.Any())
        {
            await _userManager.CreateAsync(new User
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "trungnguyen7358@gmail.com",
                FullName = "Quản trị",
                DateOfBirth = DateTime.Now,
                Email = "trungnguyen7358@gmail.com",
                LockoutEnabled = false,
                PhoneNumber = "0964732231",
                EmailConfirmed = true
            }, "Admin@123");
        }

        var topics = new List<Topic>
            {
                new Topic
                {
                    Name = "C#",
                    Alias = "csharp",
                    ImageUrl = "/images/csharp.png",
                    ParentTopicId = 0
                },
                new Topic
                {
                    Name = "Java",
                    Alias = "java",
                    ImageUrl = "/images/java.png",
                    ParentTopicId = 0
                },
                new Topic
                {
                    Name = "Python",
                    Alias = "python",
                    ImageUrl = "/images/python.png",
                    ParentTopicId = 0
                },
                new Topic
                {
                    Name = "JavaScript",
                    Alias = "javascript",
                    ImageUrl = "/images/javascript.png",
                    ParentTopicId = 0
                },
                new Topic
                {
                    Name = "Ruby",
                    Alias = "ruby",
                    ImageUrl = "/images/ruby.png",
                    ParentTopicId = 0
                },
                new Topic
                {
                    Name = "Go",
                    Alias = "go",
                    ImageUrl = "/images/go.png",
                    ParentTopicId = 0
                }
            };
        // Look for any products.
        if (_context.Topics.Any())
        {
            return;   // DB has been seeded
        }

        _context.Topics.AddRange(topics);
        await _context.SaveChangesAsync();

        var classRooms = new List<ClassRoom>
            {
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 1,
                    Name = ".NET Programming",
                    Introduction = "This class is about .NET Programming",
                    Description = ".NET is a free, cross-platform, open-source developer platform for building many different types of applications. With .NET, you can use multiple languages, editors, and libraries to build web, mobile, desktop, games, IoT, and AI apps.",
                    ImageUrl = "/images/dotnet_programming.png",
                    Code = "DFGVIL",
                    Price = 0,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 2,
                    Name = "Java Basics",
                    Introduction = "This class is about Java Basics",
                    Description = "Java is a high-level, class-based, object-oriented programming language that is designed to have as few implementation dependencies as possible.",
                    ImageUrl = "/images/java_basics.png",
                    Code = "HJKLOP",
                    Price = 90000,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 3,
                    Name = "Python for Data Science",
                    Introduction = "This class is about Python for Data Science",
                    Description = "Python is an interpreted, high-level and general-purpose programming language. Its design philosophy emphasizes code readability with its use of significant indentation.",
                    ImageUrl = "/images/python_data_science.png",
                    Code = "QWERTY",
                    Price = 120000,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 4,
                    Name = "JavaScript Essentials",
                    Introduction = "This class is about JavaScript Essentials",
                    Description = "JavaScript, often abbreviated as JS, is a programming language that conforms to the ECMAScript specification. JavaScript is high-level, often just-in-time compiled, and multi-paradigm.",
                    ImageUrl = "/images/javascript_essentials.png",
                    Code = "ZXCVBN",
                    Price = 85000,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 5,
                    Name = "Ruby on Rails",
                    Introduction = "This class is about Ruby on Rails",
                    Description = "Ruby on Rails is a server-side web application framework written in Ruby under the MIT License, and is a model-view-controller (MVC) framework.",
                    ImageUrl = "/images/ruby_on_rails.png",
                    Code = "ASDFGH",
                    Price = 95000,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 6,
                    Name = "Go Programming",
                    Introduction = "This class is about Go Programming",
                    Description = "Go is a statically typed, compiled programming language designed at Google. It is syntactically similar to C, but with memory safety, garbage collection, structural typing, and CSP-style concurrency.",
                    ImageUrl = "/images/go_programming.png",
                    Code = "TYUIOP",
                    Price = 110000,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 1,
                    Name = "Advanced .NET",
                    Introduction = "This class is about Advanced .NET",
                    Description = "Dive deeper into .NET to build more complex applications and understand the advanced features of the platform.",
                    ImageUrl = "/images/advanced_dotnet.png",
                    Code = "GFDSEA",
                    Price = 0,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 2,
                    Name = "Spring Framework with Java",
                    Introduction = "This class is about Spring Framework with Java",
                    Description = "Learn the Spring Framework to create robust Java applications with features like dependency injection, aspect-oriented programming, and more.",
                    ImageUrl = "/images/spring_framework.png",
                    Code = "LKJHGF",
                    Price = 140000,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 3,
                    Name = "Machine Learning with Python",
                    Introduction = "This class is about Machine Learning with Python",
                    Description = "Explore the world of machine learning using Python and libraries such as TensorFlow and scikit-learn.",
                    ImageUrl = "/images/ml_with_python.png",
                    Code = "POIUYT",
                    Price = 0,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                },
                new ClassRoom
                {
                    Id = Guid.NewGuid(),
                    TopicId = 4,
                    Name = "Full-Stack JavaScript",
                    Introduction = "This class is about Full-Stack JavaScript",
                    Description = "Become a full-stack JavaScript developer by learning both front-end and back-end technologies.",
                    ImageUrl = "/images/full_stack_js.png",
                    Code = "QAZXSW",
                    Price = 150000,
                    Students = 0,
                    Status = ClassRoomStatus.Approved
                }
            };
        // Look for any products.
        if (_context.ClassRooms.Any())
        {
            return;   // DB has been seeded
        }

        _context.ClassRooms.AddRange(classRooms);

        await _context.SaveChangesAsync();
    }
}


using LMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LMS.Data
{
    public static class SeedTestData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                // Get the required services
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

                // Ensure database is created and migrated
                context.Database.Migrate();

                // Create the test data seeder
                var seeder = new TestDataSeeder(context, userManager);

                // Seed the test data
                await seeder.SeedTestData();

                Console.WriteLine("Test data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while seeding the test data: {ex.Message}");
                throw;
            }
        }

        // Example of how to call this from Program.cs:
        /*
        // Add at the end of the Program.cs file, in a scope with service provider:
        if (args.Length > 0 && args[0].ToLower() == "seedtestdata")
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await SeedTestData.Initialize(services);
            }
            return;
        }
        */
    }
}
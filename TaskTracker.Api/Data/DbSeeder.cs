using Microsoft.AspNetCore.Identity;
using TaskTracker.Api.Auth;
using TaskTracker.Api.Domain.Entities;
using TaskTracker.Api.Domain.Enums;

namespace TaskTracker.Api.Data;

using TaskStatus = Domain.Enums.TaskStatus;

public static  class DbSeeder
{
        public static async Task SeedAsync(AppDbContext dbContext, PasswordHasher passwordHasher)
    {
        if (dbContext.Users.Any())
            return;

        var admin = new User
        {
            Login = "admin",
            Email = "admin@tasktracker.local",
            FullName = "System Administrator",
            Department = "IT",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, "admin123");

        var manager = new User
        {
            Login = "manager",
            Email = "manager@tasktracker.local",
            FullName = "Project Manager",
            Department = "IT",
            Role = UserRole.Manager,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        manager.PasswordHash = passwordHasher.HashPassword(manager, "manager123");

        var user = new User
        {
            Login = "user",
            Email = "user@tasktracker.local",
            FullName = "Regular User",
            Department = "Sales",
            Role = UserRole.User,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "user123");

        var project1 = new Project
        {
            Name = "Education Platform",
            Code = "EDU",
            CreatedAtUtc = DateTime.UtcNow
        };

        var project2 = new Project
        {
            Name = "CRM System",
            Code = "CRM",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Users.AddRange(admin, manager, user);
        dbContext.Projects.AddRange(project1, project2);

        dbContext.Tasks.AddRange(
            new TaskItem
            {
                Title = "Add login endpoint",
                Description = "Implement JWT login endpoint",
                Status = TaskStatus.New,
                Priority = 1,
                DueDateUtc = DateTime.UtcNow.AddDays(3),
                CreatedAtUtc = DateTime.UtcNow,
                Project = project1,
                CreatedByUser = admin
            },
            new TaskItem
            {
                Title = "Write validator for task creation",
                Description = "FluentValidation rules",
                Status = TaskStatus.InProgress,
                Priority = 2,
                DueDateUtc = DateTime.UtcNow.AddDays(2),
                CreatedAtUtc = DateTime.UtcNow,
                Project = project2,
                CreatedByUser = manager
            });

        await dbContext.SaveChangesAsync();
    }
}
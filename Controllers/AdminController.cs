using DemoWeb.Data;
using DemoWeb.DTOs;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            int totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
            int totalTeachers = await _context.Users.CountAsync(u => u.Role == UserRole.Teacher);
            int totalCourses = await _context.Subjects.CountAsync();
            int totalLessons = await _context.Lessons.CountAsync();
            int totalQuestions = await _context.Questions.CountAsync();
            int totalExamsTaken = await _context.StudentExams.CountAsync();

            double passRate = 0;
            if (totalExamsTaken > 0)
            {
                int passed = await _context.StudentExams.CountAsync(se => se.IsPassed);
                passRate = Math.Round((double)passed / totalExamsTaken * 100, 1);
            }

            var stats = new AdminDashboardStatsDto
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalCourses = totalCourses,
                TotalLessons = totalLessons,
                TotalQuestions = totalQuestions,
                TotalExamsTaken = totalExamsTaken,
                AveragePassRate = passRate
            };

            return Ok(stats);
        }
    }
}

using DemoWeb.Data;
using DemoWeb.DTOs;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProgressController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("toggle-lesson")]
        public async Task<IActionResult> ToggleLessonProgress([FromBody] LearningProgress progress)
        {
            var existing = await _context.LearningProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == progress.StudentId && lp.LessonId == progress.LessonId);

            if (existing == null)
            {
                existing = new LearningProgress
                {
                    StudentId = progress.StudentId,
                    LessonId = progress.LessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };
                _context.LearningProgresses.Add(existing);
            }
            else
            {
                existing.IsCompleted = !existing.IsCompleted;
                existing.CompletedAt = existing.IsCompleted ? DateTime.UtcNow : null;
            }

            await _context.SaveChangesAsync();
            return Ok(new { isCompleted = existing.IsCompleted });
        }

        [HttpGet("student/{studentId}/summary")]
        public async Task<IActionResult> GetStudentProgressSummary(int studentId)
        {
            var user = await _context.Users.FindAsync(studentId);
            if (user == null) return NotFound();

            var query = _context.Subjects
                .Include(s => s.Chapters)
                    .ThenInclude(c => c.Lessons)
                .Include(s => s.Exams)
                .AsQueryable();

            if (user.GradeClassId.HasValue)
            {
                query = query.Where(s => s.GradeClassId == user.GradeClassId.Value);
            }

            var subjects = await query.ToListAsync();

            var completedLessonIds = await _context.LearningProgresses
                .Where(lp => lp.StudentId == studentId && lp.IsCompleted)
                .Select(lp => lp.LessonId)
                .ToListAsync();

            var studentExams = await _context.StudentExams
                .Where(se => se.StudentId == studentId)
                .ToListAsync();

            var subjectProgresses = new List<SubjectProgressDto>();

            foreach (var sub in subjects)
            {
                var allLessons = sub.Chapters.SelectMany(c => c.Lessons).ToList();
                int totalLessons = allLessons.Count;
                int completedCount = allLessons.Count(l => completedLessonIds.Contains(l.Id));

                var examIds = sub.Exams.Select(e => e.Id).ToList();
                var subExams = studentExams.Where(se => examIds.Contains(se.ExamId)).ToList();
                double avgScore = subExams.Any() ? Math.Round(subExams.Average(se => se.Score), 1) : 0;

                subjectProgresses.Add(new SubjectProgressDto
                {
                    SubjectId = sub.Id,
                    SubjectName = sub.Name,
                    CompletedLessons = completedCount,
                    TotalLessons = totalLessons,
                    CompletionPercentage = totalLessons > 0 ? Math.Round((double)completedCount / totalLessons * 100, 1) : 0,
                    AverageExamScore = avgScore
                });
            }

            return Ok(new
            {
                StudentId = studentId,
                StudentName = user.FullName,
                TotalCompletedLessons = completedLessonIds.Count,
                TotalExamsTaken = studentExams.Count,
                AverageOverallScore = studentExams.Any() ? Math.Round(studentExams.Average(se => se.Score), 1) : 0,
                Subjects = subjectProgresses
            });
        }
    }
}

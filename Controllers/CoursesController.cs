using DemoWeb.Data;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoursesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _context.GradeClasses
                .Include(c => c.Subjects)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    SubjectCount = c.Subjects.Count
                })
                .ToListAsync();

            return Ok(classes);
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjects([FromQuery] int? classId)
        {
            var query = _context.Subjects.AsQueryable();
            if (classId.HasValue)
            {
                query = query.Where(s => s.GradeClassId == classId.Value);
            }

            var subjects = await query
                .Include(s => s.Chapters)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.GradeClassId,
                    s.Icon,
                    s.Color,
                    s.Description,
                    ChapterCount = s.Chapters.Count
                })
                .ToListAsync();

            return Ok(subjects);
        }

        [HttpGet("subjects/{subjectId}/tree")]
        public async Task<IActionResult> GetSubjectTree(int subjectId, [FromQuery] int? studentId)
        {
            var subject = await _context.Subjects
                .Include(s => s.Chapters)
                    .ThenInclude(c => c.Lessons)
                .Include(s => s.Exams)
                .FirstOrDefaultAsync(s => s.Id == subjectId);

            if (subject == null) return NotFound(new { message = "Môn học không tồn tại." });

            List<int> completedLessonIds = new();
            if (studentId.HasValue)
            {
                completedLessonIds = await _context.LearningProgresses
                    .Where(lp => lp.StudentId == studentId.Value && lp.IsCompleted)
                    .Select(lp => lp.LessonId)
                    .ToListAsync();
            }

            var result = new
            {
                subject.Id,
                subject.Name,
                subject.Color,
                subject.Icon,
                subject.Description,
                Chapters = subject.Chapters.OrderBy(c => c.OrderIndex).Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.OrderIndex,
                    Lessons = c.Lessons.OrderBy(l => l.OrderIndex).Select(l => new
                    {
                        l.Id,
                        l.Title,
                        l.ContentType,
                        l.DurationMinutes,
                        l.OrderIndex,
                        IsCompleted = completedLessonIds.Contains(l.Id)
                    })
                }),
                Exams = subject.Exams.Where(e => e.IsPublished).Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.DurationMinutes,
                    e.PassScore,
                    Type = e.Type.ToString()
                })
            };

            return Ok(result);
        }

        [HttpGet("lessons/{lessonId}")]
        public async Task<IActionResult> GetLessonDetail(int lessonId, [FromQuery] int? studentId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(c => c!.Subject)
                .Include(l => l.PracticeQuestions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return NotFound(new { message = "Bài học không tồn tại." });

            bool isCompleted = false;
            if (studentId.HasValue)
            {
                isCompleted = await _context.LearningProgresses
                    .AnyAsync(lp => lp.StudentId == studentId.Value && lp.LessonId == lessonId && lp.IsCompleted);
            }

            return Ok(new
            {
                lesson.Id,
                lesson.Title,
                lesson.ChapterId,
                ChapterTitle = lesson.Chapter?.Title,
                SubjectId = lesson.Chapter?.SubjectId,
                SubjectName = lesson.Chapter?.Subject?.Name,
                lesson.ContentType,
                lesson.VideoUrl,
                lesson.ContentText,
                lesson.DurationMinutes,
                IsCompleted = isCompleted,
                PracticeQuestions = lesson.PracticeQuestions.Select(q => new
                {
                    q.Id,
                    q.Content,
                    q.Difficulty,
                    q.Explanation,
                    Options = q.Options.Select(o => new
                    {
                        o.Id,
                        o.OptionText,
                        o.IsCorrect
                    })
                })
            });
        }
    }
}

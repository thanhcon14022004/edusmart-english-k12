using DemoWeb.Data;
using DemoWeb.DTOs;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExamsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetExams([FromQuery] int? subjectId)
        {
            var query = _context.Exams.Include(e => e.Subject).AsQueryable();
            if (subjectId.HasValue)
            {
                query = query.Where(e => e.SubjectId == subjectId.Value);
            }

            var exams = await query
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.SubjectId,
                    SubjectName = e.Subject != null ? e.Subject.Name : "",
                    e.DurationMinutes,
                    e.PassScore,
                    Type = e.Type.ToString(),
                    QuestionCount = e.ExamQuestions.Count,
                    TotalPoints = e.ExamQuestions.Sum(eq => eq.Points)
                })
                .ToListAsync();

            return Ok(exams);
        }

        [HttpGet("{examId}")]
        public async Task<IActionResult> GetExamDetail(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.Subject)
                .Include(e => e.ExamQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q!.Options)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) return NotFound(new { message = "Đề thi không tồn tại." });

            return Ok(new
            {
                exam.Id,
                exam.Title,
                exam.SubjectId,
                SubjectName = exam.Subject?.Name,
                exam.DurationMinutes,
                exam.PassScore,
                Type = exam.Type.ToString(),
                Questions = exam.ExamQuestions.Select(eq => new
                {
                    eq.QuestionId,
                    Content = eq.Question?.Content,
                    Difficulty = eq.Question?.Difficulty.ToString(),
                    Points = eq.Points,
                    Options = eq.Question?.Options.Select(o => new
                    {
                        o.Id,
                        o.OptionText
                        // Hide IsCorrect during test taking
                    })
                })
            });
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitExam([FromBody] SubmitExamRequest request)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q!.Options)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId);

            if (exam == null) return NotFound(new { message = "Đề thi không tồn tại." });

            double totalScoreObtained = 0;
            double maxScore = exam.ExamQuestions.Sum(eq => eq.Points);
            if (maxScore <= 0) maxScore = 10.0;

            int correctCount = 0;
            var questionReviews = new List<QuestionReviewDto>();
            var studentAnswers = new List<StudentAnswer>();

            foreach (var eq in exam.ExamQuestions)
            {
                var question = eq.Question;
                if (question == null) continue;

                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                int correctOptionId = correctOption?.Id ?? 0;

                var userAns = request.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                int selectedOptionId = userAns?.SelectedOptionId ?? 0;
                bool isCorrect = (selectedOptionId != 0 && selectedOptionId == correctOptionId);

                double pointsEarned = isCorrect ? eq.Points : 0;
                if (isCorrect)
                {
                    correctCount++;
                    totalScoreObtained += pointsEarned;
                }

                studentAnswers.Add(new StudentAnswer
                {
                    QuestionId = question.Id,
                    SelectedOptionId = selectedOptionId,
                    IsCorrect = isCorrect,
                    PointsEarned = pointsEarned
                });

                questionReviews.Add(new QuestionReviewDto
                {
                    QuestionId = question.Id,
                    QuestionContent = question.Content,
                    SelectedOptionId = selectedOptionId,
                    CorrectOptionId = correctOptionId,
                    IsCorrect = isCorrect,
                    Explanation = question.Explanation,
                    Options = question.Options.Select(o => new OptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                });
            }

            // Normalise score out of 10.0
            double normalizedScore = Math.Round((totalScoreObtained / maxScore) * 10.0, 1);
            bool isPassed = normalizedScore >= exam.PassScore;

            var studentExam = new StudentExam
            {
                StudentId = request.StudentId,
                ExamId = request.ExamId,
                Score = normalizedScore,
                MaxScore = 10.0,
                IsPassed = isPassed,
                StartedAt = DateTime.UtcNow.AddMinutes(-exam.DurationMinutes),
                SubmittedAt = DateTime.UtcNow,
                Status = "Completed",
                Answers = studentAnswers
            };

            _context.StudentExams.Add(studentExam);
            await _context.SaveChangesAsync();

            var resultDto = new ExamResultDto
            {
                StudentExamId = studentExam.Id,
                ExamId = exam.Id,
                ExamTitle = exam.Title,
                Score = normalizedScore,
                MaxScore = 10.0,
                Percentage = Math.Round((double)correctCount / exam.ExamQuestions.Count * 100, 1),
                IsPassed = isPassed,
                CorrectCount = correctCount,
                TotalQuestions = exam.ExamQuestions.Count,
                SubmittedAt = studentExam.SubmittedAt.Value,
                QuestionReviews = questionReviews
            };

            return Ok(resultDto);
        }

        [HttpGet("results/student/{studentId}")]
        public async Task<IActionResult> GetStudentExamResults(int studentId)
        {
            var results = await _context.StudentExams
                .Include(se => se.Exam)
                .Where(se => se.StudentId == studentId)
                .OrderByDescending(se => se.SubmittedAt)
                .Select(se => new
                {
                    se.Id,
                    se.ExamId,
                    ExamTitle = se.Exam != null ? se.Exam.Title : "",
                    se.Score,
                    se.MaxScore,
                    se.IsPassed,
                    se.SubmittedAt
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}

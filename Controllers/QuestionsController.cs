using DemoWeb.Data;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuestionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetQuestions([FromQuery] int? chapterId, [FromQuery] string? difficulty)
        {
            var query = _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Chapter)
                .AsQueryable();

            if (chapterId.HasValue)
            {
                query = query.Where(q => q.ChapterId == chapterId.Value);
            }

            if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<DifficultyLevel>(difficulty, true, out var diffEnum))
            {
                query = query.Where(q => q.Difficulty == diffEnum);
            }

            var list = await query.Select(q => new
            {
                q.Id,
                q.ChapterId,
                ChapterTitle = q.Chapter != null ? q.Chapter.Title : "",
                q.Content,
                Type = q.Type.ToString(),
                Difficulty = q.Difficulty.ToString(),
                q.Explanation,
                q.DefaultPoints,
                Options = q.Options.Select(o => new
                {
                    o.Id,
                    o.OptionText,
                    o.IsCorrect
                })
            }).ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromBody] Question question)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetQuestions), new { id = question.Id }, question);
        }
    }
}

using DemoWeb.Data;
using DemoWeb.DTOs;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() || u.Username.ToLower() == request.Email.ToLower());

            if (user == null || user.PasswordHash != request.Password)
            {
                return Unauthorized(new { message = "Tên đăng nhập / Email hoặc mật khẩu không chính xác." });
            }

            string className = "";
            if (user.GradeClassId.HasValue)
            {
                var gradeClass = await _context.GradeClasses.FindAsync(user.GradeClassId.Value);
                className = gradeClass?.Name ?? "";
            }

            var response = new LoginResponse
            {
                Token = $"mock-jwt-token-{user.Id}-{Guid.NewGuid()}",
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                GradeClassId = user.GradeClassId,
                GradeClassName = className,
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=200&q=80" : user.AvatarUrl
            };

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Vui lòng nhập tên đăng nhập và mật khẩu." });
            }

            bool exists = await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower() || (!string.IsNullOrEmpty(request.Email) && u.Email.ToLower() == request.Email.ToLower()));
            if (exists)
            {
                return BadRequest(new { message = "Tên đăng nhập hoặc Email này đã tồn tại trong hệ thống." });
            }

            var newUser = new User
            {
                FullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Username : request.FullName,
                Username = request.Username,
                Email = string.IsNullOrWhiteSpace(request.Email) ? $"{request.Username}@student.edu.vn" : request.Email,
                PasswordHash = request.Password,
                Role = UserRole.Student,
                GradeClassId = request.GradeClassId > 0 ? request.GradeClassId : 1,
                AvatarUrl = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=200&q=80",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            string className = "";
            var gradeClass = await _context.GradeClasses.FindAsync(newUser.GradeClassId);
            className = gradeClass?.Name ?? "";

            var response = new LoginResponse
            {
                Token = $"mock-jwt-token-{newUser.Id}-{Guid.NewGuid()}",
                UserId = newUser.Id,
                FullName = newUser.FullName,
                Email = newUser.Email,
                Role = newUser.Role.ToString(),
                GradeClassId = newUser.GradeClassId,
                GradeClassName = className,
                AvatarUrl = newUser.AvatarUrl
            };

            return Ok(response);
        }

        [HttpGet("me/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            string className = "";
            if (user.GradeClassId.HasValue)
            {
                var gradeClass = await _context.GradeClasses.FindAsync(user.GradeClassId.Value);
                className = gradeClass?.Name ?? "";
            }

            return Ok(new
            {
                user.Id,
                user.Username,
                user.FullName,
                user.Email,
                Role = user.Role.ToString(),
                user.GradeClassId,
                GradeClassName = className,
                user.AvatarUrl
            });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.FullName,
                    u.Email,
                    Role = u.Role.ToString(),
                    u.GradeClassId,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}

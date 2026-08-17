using System.ComponentModel.DataAnnotations;

namespace DemoWeb.Models
{
    public enum UserRole
    {
        Student,
        Teacher,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Student;
        public int? GradeClassId { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class GradeClass
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty; // e.g. "Lớp 6", "Lớp 7"
        public string Description { get; set; } = string.Empty;
        public List<Subject> Subjects { get; set; } = new();
    }

    public class Subject
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty; // e.g. "Toán Học", "Ngữ Văn"
        public int GradeClassId { get; set; }
        public GradeClass? GradeClass { get; set; }
        public string Icon { get; set; } = "book";
        public string Color { get; set; } = "#4f46e5";
        public string Description { get; set; } = string.Empty;
        public List<Chapter> Chapters { get; set; } = new();
        public List<Exam> Exams { get; set; } = new();
    }

    public class Chapter
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty; // e.g. "Chương 1: Tập hợp các số tự nhiên"
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }
        public int OrderIndex { get; set; }
        public List<Lesson> Lessons { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
    }

    public class Lesson
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public int ChapterId { get; set; }
        public Chapter? Chapter { get; set; }
        public string ContentType { get; set; } = "video"; // "video", "article", "document"
        public string VideoUrl { get; set; } = string.Empty;
        public string ContentText { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } = 15;
        public int OrderIndex { get; set; }
        public List<Question> PracticeQuestions { get; set; } = new();
    }

    public enum QuestionType
    {
        SingleChoice,
        MultipleChoice,
        TrueFalse
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public class Question
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public Chapter? Chapter { get; set; }
        public int? LessonId { get; set; }
        public Lesson? Lesson { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
        public QuestionType Type { get; set; } = QuestionType.SingleChoice;
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
        public string Explanation { get; set; } = string.Empty;
        public double DefaultPoints { get; set; } = 1.0;
        public List<QuestionOption> Options { get; set; } = new();
    }

    public class QuestionOption
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public Question? Question { get; set; }
        [Required]
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public enum ExamType
    {
        Practice,
        Midterm,
        Final
    }

    public class Exam
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }
        public int DurationMinutes { get; set; } = 45;
        public double PassScore { get; set; } = 5.0;
        public ExamType Type { get; set; } = ExamType.Practice;
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ExamQuestion> ExamQuestions { get; set; } = new();
    }

    public class ExamQuestion
    {
        public int ExamId { get; set; }
        public Exam? Exam { get; set; }
        public int QuestionId { get; set; }
        public Question? Question { get; set; }
        public double Points { get; set; } = 1.0;
    }

    public class StudentExam
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public User? Student { get; set; }
        public int ExamId { get; set; }
        public Exam? Exam { get; set; }
        public double Score { get; set; }
        public double MaxScore { get; set; }
        public bool IsPassed { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }
        public string Status { get; set; } = "Completed"; // "InProcess", "Submitted", "Graded"
        public List<StudentAnswer> Answers { get; set; } = new();
    }

    public class StudentAnswer
    {
        public int Id { get; set; }
        public int StudentExamId { get; set; }
        public StudentExam? StudentExam { get; set; }
        public int QuestionId { get; set; }
        public Question? Question { get; set; }
        public int SelectedOptionId { get; set; }
        public bool IsCorrect { get; set; }
        public double PointsEarned { get; set; }
    }

    public class LearningProgress
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public User? Student { get; set; }
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}

-- ============================================================================
-- EDUSMART ONLINE LEARNING PLATFORM - FULL DATABASE SCRIPT (MS SQL SERVER)
-- Project: PRN222 DemoWeb
-- Compatible with SQL Server 2016+, LocalDB, and SSMS (SQL Server Management Studio)
-- ============================================================================

CREATE DATABASE [EduSmartDb];
GO

USE [EduSmartDb];
GO

-- 1. Create GradeClasses Table
CREATE TABLE [GradeClasses] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL
);
GO

-- 2. Create Users Table
CREATE TABLE [Users] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [FullName] NVARCHAR(150) NOT NULL,
    [Email] NVARCHAR(150) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [Role] INT NOT NULL DEFAULT 0, -- 0: Student, 1: Teacher, 2: Admin
    [GradeClassId] INT NULL FOREIGN KEY REFERENCES [GradeClasses]([Id]),
    [AvatarUrl] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- 3. Create Subjects Table
CREATE TABLE [Subjects] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(150) NOT NULL,
    [GradeClassId] INT NOT NULL FOREIGN KEY REFERENCES [GradeClasses]([Id]),
    [Icon] NVARCHAR(50) NULL,
    [Color] NVARCHAR(50) NULL,
    [Description] NVARCHAR(500) NULL
);
GO

-- 4. Create Chapters Table
CREATE TABLE [Chapters] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SubjectId] INT NOT NULL FOREIGN KEY REFERENCES [Subjects]([Id]) ON DELETE CASCADE,
    [Title] NVARCHAR(200) NOT NULL,
    [Order] INT NOT NULL DEFAULT 1
);
GO

-- 5. Create Lessons Table
CREATE TABLE [Lessons] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ChapterId] INT NOT NULL FOREIGN KEY REFERENCES [Chapters]([Id]) ON DELETE CASCADE,
    [Title] NVARCHAR(200) NOT NULL,
    [VideoUrl] NVARCHAR(500) NULL,
    [TheoryText] NVARCHAR(MAX) NULL,
    [Order] INT NOT NULL DEFAULT 1
);
GO

-- 6. Create Questions Table
CREATE TABLE [Questions] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ChapterId] INT NOT NULL FOREIGN KEY REFERENCES [Chapters]([Id]),
    [Content] NVARCHAR(MAX) NOT NULL,
    [Explanation] NVARCHAR(MAX) NULL,
    [Difficulty] INT NOT NULL DEFAULT 0 -- 0: Easy, 1: Medium, 2: Hard
);
GO

-- 7. Create QuestionOptions Table
CREATE TABLE [QuestionOptions] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [QuestionId] INT NOT NULL FOREIGN KEY REFERENCES [Questions]([Id]) ON DELETE CASCADE,
    [OptionText] NVARCHAR(MAX) NOT NULL,
    [IsCorrect] BIT NOT NULL DEFAULT 0
);
GO

-- 8. Create Exams Table
CREATE TABLE [Exams] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SubjectId] INT NOT NULL FOREIGN KEY REFERENCES [Subjects]([Id]),
    [Title] NVARCHAR(200) NOT NULL,
    [Type] INT NOT NULL DEFAULT 0, -- 0: Practice15m, 1: Midterm45m, 2: Final90m
    [DurationMinutes] INT NOT NULL DEFAULT 15,
    [PassScore] FLOAT NOT NULL DEFAULT 5.0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- 9. Create ExamQuestions Join Table
CREATE TABLE [ExamQuestions] (
    [ExamId] INT NOT NULL FOREIGN KEY REFERENCES [Exams]([Id]) ON DELETE CASCADE,
    [QuestionId] INT NOT NULL FOREIGN KEY REFERENCES [Questions]([Id]),
    PRIMARY KEY ([ExamId], [QuestionId])
);
GO

-- 10. Create StudentExams Table
CREATE TABLE [StudentExams] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StudentId] INT NOT NULL FOREIGN KEY REFERENCES [Users]([Id]),
    [ExamId] INT NOT NULL FOREIGN KEY REFERENCES [Exams]([Id]),
    [Score] FLOAT NOT NULL,
    [MaxScore] FLOAT NOT NULL DEFAULT 10.0,
    [IsPassed] BIT NOT NULL DEFAULT 0,
    [SubmittedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- 11. Create StudentAnswers Table
CREATE TABLE [StudentAnswers] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StudentExamId] INT NOT NULL FOREIGN KEY REFERENCES [StudentExams]([Id]) ON DELETE CASCADE,
    [QuestionId] INT NOT NULL FOREIGN KEY REFERENCES [Questions]([Id]),
    [SelectedOptionId] INT NOT NULL FOREIGN KEY REFERENCES [QuestionOptions]([Id])
);
GO

-- 12. Create LearningProgresses Table
CREATE TABLE [LearningProgresses] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StudentId] INT NOT NULL FOREIGN KEY REFERENCES [Users]([Id]),
    [LessonId] INT NOT NULL FOREIGN KEY REFERENCES [Lessons]([Id]),
    [IsCompleted] BIT NOT NULL DEFAULT 0,
    [CompletedAt] DATETIME2 NULL
);
GO

-- ============================================================================
-- SEED INITIAL DATA
-- ============================================================================

INSERT INTO [GradeClasses] ([Name], [Description]) VALUES
(N'Lớp 1', N'Chương trình Tiểu học Khối 1'),
(N'Lớp 2', N'Chương trình Tiểu học Khối 2'),
(N'Lớp 3', N'Chương trình Tiểu học Khối 3'),
(N'Lớp 4', N'Chương trình Tiểu học Khối 4'),
(N'Lớp 5', N'Chương trình Tiểu học Khối 5'),
(N'Lớp 6', N'Chương trình THCS Khối 6'),
(N'Lớp 7', N'Chương trình THCS Khối 7'),
(N'Lớp 8', N'Chương trình THCS Khối 8'),
(N'Lớp 9', N'Chương trình THCS Khối 9'),
(N'Lớp 10', N'Chương trình THPT Khối 10'),
(N'Lớp 11', N'Chương trình THPT Khối 11'),
(N'Lớp 12', N'Chương trình THPT Khối 12');
GO

INSERT INTO [Users] ([Username], [FullName], [Email], [PasswordHash], [Role], [GradeClassId], [AvatarUrl]) VALUES
(N'student1', N'Trần Minh Quân', N'hocsinh.quan@edu.vn', N'student123', 0, 6, N'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?auto=format&fit=crop&w=200&q=80'),
(N'student7', N'Lê Ngọc Anh', N'ngocanh7@student.edu.vn', N'student123', 0, 7, N'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=200&q=80'),
(N'student8', N'Nguyễn Hoàng Nam', N'hoangnam8@student.edu.vn', N'student123', 0, 8, N'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=200&q=80');
GO

PRINT N'EduSmartDb database script executed successfully!';
GO

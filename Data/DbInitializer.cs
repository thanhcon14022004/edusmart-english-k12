using DemoWeb.Models;

namespace DemoWeb.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return; // DB has already been initialized
            }

            // 1. Seed Grade Classes for Cấp 1 (Tiểu học), Cấp 2 (THCS), Cấp 3 (THPT)
            var class1 = new GradeClass { Name = "Lớp 1", Description = "Chương trình Tiếng Anh Tiểu học Lớp 1 - Phonics & Từ vựng cơ bản" };
            var class2 = new GradeClass { Name = "Lớp 2", Description = "Chương trình Tiếng Anh Tiểu học Lớp 2 - Giao tiếp gia đình & bạn bè" };
            var class3 = new GradeClass { Name = "Lớp 3", Description = "Chương trình Tiếng Anh Tiểu học Lớp 3 - Cambridge Starters" };
            var class4 = new GradeClass { Name = "Lớp 4", Description = "Chương trình Tiếng Anh Tiểu học Lớp 4 - Cambridge Movers" };
            var class5 = new GradeClass { Name = "Lớp 5", Description = "Chương trình Tiếng Anh Tiểu học Lớp 5 - Cambridge Flyers & Chuẩn bị THCS" };

            var class6 = new GradeClass { Name = "Lớp 6", Description = "Chương trình Tiếng Anh THCS Lớp 6 - Global Success & Chuyên Đề" };
            var class7 = new GradeClass { Name = "Lớp 7", Description = "Chương trình Tiếng Anh THCS Lớp 7 - Ngữ pháp & Đọc hiểu nâng cao" };
            var class8 = new GradeClass { Name = "Lớp 8", Description = "Chương trình Tiếng Anh THCS Lớp 8 - Cambridge KET & PET" };
            var class9 = new GradeClass { Name = "Lớp 9", Description = "Chương trình Tiếng Anh THCS Lớp 9 - Ôn thi Vào 10 Môn Tiếng Anh" };

            var class10 = new GradeClass { Name = "Lớp 10", Description = "Chương trình Tiếng Anh THPT Lớp 10 - Nền tảng Giao tiếp & Grammar" };
            var class11 = new GradeClass { Name = "Lớp 11", Description = "Chương trình Tiếng Anh THPT Lớp 11 - IELTS Foundation & Communication" };
            var class12 = new GradeClass { Name = "Lớp 12", Description = "Chương trình Tiếng Anh THPT Lớp 12 - Ôn thi Tốt nghiệp THPT Quốc Gia Môn Tiếng Anh" };

            context.GradeClasses.AddRange(
                class1, class2, class3, class4, class5,
                class6, class7, class8, class9,
                class10, class11, class12
            );
            context.SaveChanges();

            // 2. Seed Users
            var student = new User
            {
                Username = "student1",
                FullName = "Trần Minh Quân",
                Email = "hocsinh.quan@edu.vn",
                PasswordHash = "student123",
                Role = UserRole.Student,
                GradeClassId = class6.Id,
                AvatarUrl = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?auto=format&fit=crop&w=200&q=80"
            };
            context.Users.Add(student);
            context.SaveChanges();

            // 3. Seed Rich Selection of English Subjects for each K-12 Grade Class
            // --- LỚP 1 ---
            var eng1_1 = new Subject { Name = "Tiếng Anh 1 - English Phonics & ABC", GradeClassId = class1.Id, Icon = "icons", Color = "#06b6d4", Description = "Vui học phát âm Phonics A-B-C, từ vựng màu sắc & chữ số qua bài hát" };
            var eng1_2 = new Subject { Name = "Tiếng Anh 1 - English Vocabulary & Songs", GradeClassId = class1.Id, Icon = "music", Color = "#ec4899", Description = "Học từ vựng hoa quả, con vật qua bài hát Tiếng Anh sinh động" };
            var eng1_3 = new Subject { Name = "Tiếng Anh 1 - Kids Communication", GradeClassId = class1.Id, Icon = "comments", Color = "#3b82f6", Description = "Rèn luyện phản xạ chào hỏi & giới thiệu bản thân đơn giản" };

            // --- LỚP 2 ---
            var eng2_1 = new Subject { Name = "Tiếng Anh 2 - Fun with English", GradeClassId = class2.Id, Icon = "gamepad", Color = "#3b82f6", Description = "Giao tiếp câu đơn giản chủ đề Gia đình, Đồ chơi & Thú cưng" };
            var eng2_2 = new Subject { Name = "Tiếng Anh 2 - Speaking & Listening", GradeClassId = class2.Id, Icon = "volume-high", Color = "#10b981", Description = "Luyện nghe nói Tiếng Anh qua các đoạn hội thoại đời sống" };
            var eng2_3 = new Subject { Name = "Tiếng Anh 2 - Picture Storytelling", GradeClassId = class2.Id, Icon = "book-open-reader", Color = "#f59e0b", Description = "Đọc truyện tranh Tiếng Anh ngắn & trả lời câu hỏi" };

            // --- LỚP 3 ---
            var eng3_1 = new Subject { Name = "Tiếng Anh 3 - Starters Grammar & Vocab", GradeClassId = class3.Id, Icon = "star", Color = "#eab308", Description = "Từ vựng & cấu trúc ngữ pháp chuẩn Cambridge Starters" };
            var eng3_2 = new Subject { Name = "Tiếng Anh 3 - Cambridge Starters Exam Prep", GradeClassId = class3.Id, Icon = "award", Color = "#8b5cf6", Description = "Luyện đề thi chứng chỉ Cambridge Starters 15 khiên" };

            // --- LỚP 4 ---
            var eng4_1 = new Subject { Name = "Tiếng Anh 4 - Cambridge Movers Prep", GradeClassId = class4.Id, Icon = "bolt", Color = "#f59e0b", Description = "Luyện thi Cambridge Movers: Mở rộng từ vựng & cấu trúc câu" };
            var eng4_2 = new Subject { Name = "Tiếng Anh 4 - Reading & Writing Skills", GradeClassId = class4.Id, Icon = "pen-nib", Color = "#06b6d4", Description = "Rèn kỹ năng đọc hiểu văn bản & viết đoạn văn ngắn Tiếng Anh" };

            // --- LỚP 5 ---
            var eng5_1 = new Subject { Name = "Tiếng Anh 5 - Cambridge Flyers Prep", GradeClassId = class5.Id, Icon = "award", Color = "#ec4899", Description = "Luyện thi Cambridge Flyers: Đạt chuẩn đầu ra Tiểu học" };
            var eng5_2 = new Subject { Name = "Tiếng Anh 5 - Primary Grammar Mastery", GradeClassId = class5.Id, Icon = "spell-check", Color = "#4f46e5", Description = "Tổng ôn ngữ pháp Tiếng Anh Tiểu học & chuẩn bị vào Lớp 6" };

            // --- LỚP 6 (DANH SÁCH KHÓA HỌC PHONG PHÚ CHO LỚP 6) ---
            var eng6_1 = new Subject { Name = "Tiếng Anh 6 - Global Success (Bộ GD&ĐT)", GradeClassId = class6.Id, Icon = "language", Color = "#4f46e5", Description = "Chương trình SGK Tiếng Anh 6 Mới: Unit 1-12, Từ vựng & Bài tập bám sát" };
            var eng6_2 = new Subject { Name = "Tiếng Anh 6 - Master Grammar & Tenses", GradeClassId = class6.Id, Icon = "layer-group", Color = "#06b6d4", Description = "Chuyên đề Ngữ pháp Lớp 6: Thì Hiện tại đơn, Hiện tại tiếp diễn, So sánh" };
            var eng6_3 = new Subject { Name = "Tiếng Anh 6 - Listening & Speaking Workshop", GradeClassId = class6.Id, Icon = "headphones", Color = "#10b981", Description = "Luyện nghe nói giao tiếp Tiếng Anh theo chủ đề trường học & cuộc sống" };
            var eng6_4 = new Subject { Name = "Tiếng Anh 6 - Academic Reading & Vocab", GradeClassId = class6.Id, Icon = "book-bookmark", Color = "#f97316", Description = "Mở rộng từ vựng nâng cao & rèn phương pháp làm bài đọc hiểu Tiếng Anh 6" };

            // --- LỚP 7 ---
            var eng7_1 = new Subject { Name = "Tiếng Anh 7 - Global Success (Bộ GD&ĐT)", GradeClassId = class7.Id, Icon = "globe", Color = "#0284c7", Description = "Chương trình Lớp 7: Hobby, Health, Community Service & Past Simple" };
            var eng7_2 = new Subject { Name = "Tiếng Anh 7 - Advanced Grammar & Writing", GradeClassId = class7.Id, Icon = "file-signature", Color = "#8b5cf6", Description = "Chuyên đề ngữ pháp nâng cao & kỹ năng viết đoạn văn Tiếng Anh 7" };

            // --- LỚP 8 ---
            var eng8_1 = new Subject { Name = "Tiếng Anh 8 - Global Success (Bộ GD&ĐT)", GradeClassId = class8.Id, Icon = "graduation-cap", Color = "#10b981", Description = "Tiếng Anh Lớp 8: Unit 1-12, Passive Voice, Conditional Sentences" };
            var eng8_2 = new Subject { Name = "Tiếng Anh 8 - Cambridge KET & PET Prep", GradeClassId = class8.Id, Icon = "award", Color = "#f59e0b", Description = "Luyện thi chứng chỉ Cambridge A2 Key (KET) & B1 Preliminary (PET)" };

            // --- LỚP 9 ---
            var eng9_1 = new Subject { Name = "Tiếng Anh 9 - Global Success (Bộ GD&ĐT)", GradeClassId = class9.Id, Icon = "book-open", Color = "#3b82f6", Description = "Chương trình SGK Tiếng Anh Lớp 9 bám sát Bộ Giáo dục" };
            var eng9_2 = new Subject { Name = "Ôn Thi Vào 10 Môn Tiếng Anh", GradeClassId = class9.Id, Icon = "trophy", Color = "#f97316", Description = "Tổng hợp 30 chuyên đề Ngữ pháp & Đề thi tuyển sinh vào Lớp 10 các Tỉnh/TP" };

            // --- LỚP 10 ---
            var eng10_1 = new Subject { Name = "Tiếng Anh 10 - High School Mastery", GradeClassId = class10.Id, Icon = "book-open", Color = "#6366f1", Description = "Tiếng Anh THPT Lớp 10: Family Life, Humans & Environment, Gender Equality" };
            var eng10_2 = new Subject { Name = "Tiếng Anh 10 - IELTS Foundation 4.5 - 5.5", GradeClassId = class10.Id, Icon = "chart-line", Color = "#ec4899", Description = "Khởi động lộ trình luyện thi IELTS 4 Kỹ Năng dành cho học sinh THPT" };

            // --- LỚP 11 ---
            var eng11_1 = new Subject { Name = "Tiếng Anh 11 - Academic English", GradeClassId = class11.Id, Icon = "spell-check", Color = "#8b5cf6", Description = "Tiếng Anh Lớp 11: Global Cities, Heritage, Ecosystems" };
            var eng11_2 = new Subject { Name = "Tiếng Anh 11 - IELTS Intensive 6.0+", GradeClassId = class11.Id, Icon = "fire-flame-curved", Color = "#ef4444", Description = "Tăng tốc kỹ năng IELTS Academic Reading & Writing Task 1/2" };

            // --- LỚP 12 ---
            var eng12_1 = new Subject { Name = "Ôn Thi THPTQG Môn Tiếng Anh 12", GradeClassId = class12.Id, Icon = "trophy", Color = "#ef4444", Description = "Luyện đề thi Tốt nghiệp THPT Quốc Gia Môn Tiếng Anh chuẩn cấu trúc Bộ" };
            var eng12_2 = new Subject { Name = "Tiếng Anh 12 - High School Graduation Mastery", GradeClassId = class12.Id, Icon = "graduation-cap", Color = "#3b82f6", Description = "Tổng ôn toàn bộ ngữ pháp, trọng âm, từ vựng 12 khối THPT" };

            context.Subjects.AddRange(
                eng1_1, eng1_2, eng1_3,
                eng2_1, eng2_2, eng2_3,
                eng3_1, eng3_2,
                eng4_1, eng4_2,
                eng5_1, eng5_2,
                eng6_1, eng6_2, eng6_3, eng6_4,
                eng7_1, eng7_2,
                eng8_1, eng8_2,
                eng9_1, eng9_2,
                eng10_1, eng10_2,
                eng11_1, eng11_2,
                eng12_1, eng12_2
            );
            context.SaveChanges();

            // 4. Seed Chapters & Lessons for Tiếng Anh 6 Courses
            // Course 1: Global Success
            var chap1 = new Chapter { SubjectId = eng6_1.Id, Title = "Unit 1: My New School (Trường học mới của tôi)", OrderIndex = 1 };
            var chap2 = new Chapter { SubjectId = eng6_1.Id, Title = "Unit 2: My House (Ngôi nhà của tôi)", OrderIndex = 2 };

            // Course 2: Master Grammar & Tenses
            var chap3 = new Chapter { SubjectId = eng6_2.Id, Title = "Chuyên đề 1: Thì Hiện Tại Đơn (Present Simple Tense)", OrderIndex = 1 };
            var chap4 = new Chapter { SubjectId = eng6_2.Id, Title = "Chuyên đề 2: Thì Hiện Tại Tiếp Diễn (Present Continuous Tense)", OrderIndex = 2 };

            // Course 3: Listening & Speaking
            var chap5 = new Chapter { SubjectId = eng6_3.Id, Title = "Workshop 1: Everyday Greetings & Self-Introduction", OrderIndex = 1 };

            // Course 4: Academic Reading
            var chap6 = new Chapter { SubjectId = eng6_4.Id, Title = "Reading Skill 1: Main Idea & Context Clues", OrderIndex = 1 };

            context.Chapters.AddRange(chap1, chap2, chap3, chap4, chap5, chap6);
            context.SaveChanges();

            // Lessons for Course 1
            var lesson1 = new Lesson
            {
                ChapterId = chap1.Id,
                Title = "Lesson 1: Vocabulary & Listening - School Subjects & Items",
                ContentType = "video",
                VideoUrl = "https://www.youtube.com/embed/juKd26qkNAw",
                ContentText = "Key Vocabulary:\n- School bag: Cặp sách\n- Calculator: Máy tính bỏ túi\n- Pencil case: Hộp bút\n- Compass: Combo vẽ đường tròn\n- Uniform: Đồng phục học sinh\n\nListening Practice:\nNghe bài hội thoại giữa Phong và Vy về ngày đầu tiên đi học tại trường mới.",
                DurationMinutes = 20,
                OrderIndex = 1
            };

            var lesson2 = new Lesson
            {
                ChapterId = chap1.Id,
                Title = "Lesson 2: Grammar Focus - The Present Simple Tense",
                ContentType = "article",
                VideoUrl = "",
                ContentText = "Thì Hiện Tại Đơn (The Present Simple Tense):\n\n1. Cách dùng:\n- Diễn tả hành động lặp đi lặp lại hoặc thói quen hàng ngày.\n- Diễn tả một sự thật hiển nhiên, chân lý.\n\n2. Cấu trúc:\n- Thể khẳng định (+): S + V(s/es) (Ví dụ: She goes to school every day).\n- Thể phủ định (-): S + do/does + not + V_bare (Ví dụ: He does not play football).\n- Thể nghi vấn (?): Do/Does + S + V_bare? (Ví dụ: Do you study English?).",
                DurationMinutes = 25,
                OrderIndex = 2
            };

            var lesson3 = new Lesson
            {
                ChapterId = chap2.Id,
                Title = "Lesson 3: Reading & Writing - Describing Rooms in My House",
                ContentType = "video",
                VideoUrl = "https://www.youtube.com/embed/juKd26qkNAw",
                ContentText = "Vocabulary for Rooms:\n- Living room: Phòng khách\n- Bedroom: Phòng ngủ\n- Kitchen: Phòng bếp\n- Bathroom: Phòng tắm\n\nGrammar Structure:\n- There is + A/An + Danh từ số ít (There is a sofa in the living room).\n- There are + Danh từ số nhiều (There are two pillows on the bed).",
                DurationMinutes = 18,
                OrderIndex = 1
            };

            // Lessons for Course 2 (Master Grammar)
            var lesson4 = new Lesson
            {
                ChapterId = chap3.Id,
                Title = "Chuyên đề 1.1: Quy tắc thêm s/es vào sau Động từ số ít",
                ContentType = "article",
                VideoUrl = "",
                ContentText = "Quy tắc thêm 's' hoặc 'es' trong thì Hiện tại đơn:\n1. Thêm 'es' sau các động từ tận cùng bằng: o, s, z, ch, x, sh (Ví dụ: watch -> watches, go -> goes).\n2. Động từ tận cùng là 'y' sau phụ âm -> đổi 'y' thành 'i' rồi thêm 'es' (Ví dụ: study -> studies).\n3. Các trường hợp còn lại: Thêm 's' (Ví dụ: play -> plays, read -> reads).",
                DurationMinutes = 20,
                OrderIndex = 1
            };

            context.Lessons.AddRange(lesson1, lesson2, lesson3, lesson4);
            context.SaveChanges();

            // 5. Seed English Questions & Options
            var q1 = new Question
            {
                ChapterId = chap1.Id,
                LessonId = lesson2.Id,
                Content = "Choose the correct verb form: 'She _____ to school by bus every morning.'",
                Difficulty = DifficultyLevel.Easy,
                Explanation = "Chủ ngữ 'She' (số ít) trong thì Hiện tại đơn đi với động từ thêm 'es' -> 'goes'.",
                DefaultPoints = 2.5
            };
            var q1Options = new List<QuestionOption>
            {
                new QuestionOption { Question = q1, OptionText = "goes", IsCorrect = true },
                new QuestionOption { Question = q1, OptionText = "go", IsCorrect = false },
                new QuestionOption { Question = q1, OptionText = "going", IsCorrect = false },
                new QuestionOption { Question = q1, OptionText = "gone", IsCorrect = false }
            };

            var q2 = new Question
            {
                ChapterId = chap1.Id,
                LessonId = lesson1.Id,
                Content = "What is the correct English word for 'Cặp sách / Balo học sinh'?",
                Difficulty = DifficultyLevel.Easy,
                Explanation = "'School bag' có nghĩa là cặp sách/balo đi học.",
                DefaultPoints = 2.5
            };
            var q2Options = new List<QuestionOption>
            {
                new QuestionOption { Question = q2, OptionText = "School bag", IsCorrect = true },
                new QuestionOption { Question = q2, OptionText = "Pencil case", IsCorrect = false },
                new QuestionOption { Question = q2, OptionText = "Notebook", IsCorrect = false },
                new QuestionOption { Question = q2, OptionText = "Textbook", IsCorrect = false }
            };

            var q3 = new Question
            {
                ChapterId = chap1.Id,
                LessonId = lesson2.Id,
                Content = "Find the word with a different underlined sound: A. cat, B. hat, C. car, D. bat",
                Difficulty = DifficultyLevel.Medium,
                Explanation = "Từ 'car' phát âm là /kɑː/, trong khi các từ 'cat', 'hat', 'bat' phát âm là /æ/.",
                DefaultPoints = 2.5
            };
            var q3Options = new List<QuestionOption>
            {
                new QuestionOption { Question = q3, OptionText = "C. car", IsCorrect = true },
                new QuestionOption { Question = q3, OptionText = "A. cat", IsCorrect = false },
                new QuestionOption { Question = q3, OptionText = "B. hat", IsCorrect = false },
                new QuestionOption { Question = q3, OptionText = "D. bat", IsCorrect = false }
            };

            var q4 = new Question
            {
                ChapterId = chap2.Id,
                LessonId = lesson3.Id,
                Content = "Complete the sentence: 'There _____ three chairs in the dining room.'",
                Difficulty = DifficultyLevel.Easy,
                Explanation = "'three chairs' là danh từ số nhiều nên dùng động từ tobe 'are' -> 'There are'.",
                DefaultPoints = 2.5
            };
            var q4Options = new List<QuestionOption>
            {
                new QuestionOption { Question = q4, OptionText = "are", IsCorrect = true },
                new QuestionOption { Question = q4, OptionText = "is", IsCorrect = false },
                new QuestionOption { Question = q4, OptionText = "am", IsCorrect = false },
                new QuestionOption { Question = q4, OptionText = "be", IsCorrect = false }
            };

            var q5 = new Question
            {
                ChapterId = chap2.Id,
                LessonId = lesson3.Id,
                Content = "Select the correct synonym (từ đồng nghĩa) for 'Big':",
                Difficulty = DifficultyLevel.Easy,
                Explanation = "'Large' có nghĩa là to/lớn, đồng nghĩa với 'Big'.",
                DefaultPoints = 2.5
            };
            var q5Options = new List<QuestionOption>
            {
                new QuestionOption { Question = q5, OptionText = "Large", IsCorrect = true },
                new QuestionOption { Question = q5, OptionText = "Small", IsCorrect = false },
                new QuestionOption { Question = q5, OptionText = "Tiny", IsCorrect = false },
                new QuestionOption { Question = q5, OptionText = "Short", IsCorrect = false }
            };

            context.Questions.AddRange(q1, q2, q3, q4, q5);
            context.QuestionOptions.AddRange(q1Options);
            context.QuestionOptions.AddRange(q2Options);
            context.QuestionOptions.AddRange(q3Options);
            context.QuestionOptions.AddRange(q4Options);
            context.QuestionOptions.AddRange(q5Options);
            context.SaveChanges();

            // 6. Seed Exclusively English Online Exams
            var exam1 = new Exam
            {
                Title = "Bài kiểm tra 15 phút: Present Simple & School Vocabulary",
                SubjectId = eng6_1.Id,
                DurationMinutes = 15,
                PassScore = 5.0,
                Type = ExamType.Practice,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var exam2 = new Exam
            {
                Title = "Đề thi Giữa Kỳ 1 - Tiếng Anh 6 (Global Success)",
                SubjectId = eng6_1.Id,
                DurationMinutes = 45,
                PassScore = 5.0,
                Type = ExamType.Midterm,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var exam3 = new Exam
            {
                Title = "Đề thi Ôn THPT Quốc Gia Môn Tiếng Anh (Cấu trúc Bộ)",
                SubjectId = eng12_1.Id,
                DurationMinutes = 60,
                PassScore = 6.0,
                Type = ExamType.Final,
                IsPublished = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Exams.AddRange(exam1, exam2, exam3);
            context.SaveChanges();

            context.ExamQuestions.AddRange(
                new ExamQuestion { ExamId = exam1.Id, QuestionId = q1.Id, Points = 2.5 },
                new ExamQuestion { ExamId = exam1.Id, QuestionId = q2.Id, Points = 2.5 },
                new ExamQuestion { ExamId = exam1.Id, QuestionId = q3.Id, Points = 2.5 },
                new ExamQuestion { ExamId = exam1.Id, QuestionId = q4.Id, Points = 2.5 },

                new ExamQuestion { ExamId = exam2.Id, QuestionId = q1.Id, Points = 2.0 },
                new ExamQuestion { ExamId = exam2.Id, QuestionId = q2.Id, Points = 2.0 },
                new ExamQuestion { ExamId = exam2.Id, QuestionId = q3.Id, Points = 2.0 },
                new ExamQuestion { ExamId = exam2.Id, QuestionId = q4.Id, Points = 2.0 },
                new ExamQuestion { ExamId = exam2.Id, QuestionId = q5.Id, Points = 2.0 },

                new ExamQuestion { ExamId = exam3.Id, QuestionId = q1.Id, Points = 2.0 },
                new ExamQuestion { ExamId = exam3.Id, QuestionId = q2.Id, Points = 2.0 },
                new ExamQuestion { ExamId = exam3.Id, QuestionId = q3.Id, Points = 2.0 },
                new ExamQuestion { ExamId = exam3.Id, QuestionId = q4.Id, Points = 2.0 },
                new ExamQuestion { ExamId = exam3.Id, QuestionId = q5.Id, Points = 2.0 }
            );

            // 7. Seed Student Learning Progress & Past Exam Attempts
            context.LearningProgresses.Add(new LearningProgress
            {
                StudentId = student.Id,
                LessonId = lesson1.Id,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            });

            var pastExam = new StudentExam
            {
                StudentId = student.Id,
                ExamId = exam1.Id,
                Score = 10.0,
                MaxScore = 10.0,
                IsPassed = true,
                StartedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(-12),
                SubmittedAt = DateTime.UtcNow.AddDays(-2),
                Status = "Completed",
                Answers = new List<StudentAnswer>
                {
                    new StudentAnswer { QuestionId = q1.Id, SelectedOptionId = q1Options.First(o => o.IsCorrect).Id, IsCorrect = true, PointsEarned = 2.5 },
                    new StudentAnswer { QuestionId = q2.Id, SelectedOptionId = q2Options.First(o => o.IsCorrect).Id, IsCorrect = true, PointsEarned = 2.5 },
                    new StudentAnswer { QuestionId = q3.Id, SelectedOptionId = q3Options.First(o => o.IsCorrect).Id, IsCorrect = true, PointsEarned = 2.5 },
                    new StudentAnswer { QuestionId = q4.Id, SelectedOptionId = q4Options.First(o => o.IsCorrect).Id, IsCorrect = true, PointsEarned = 2.5 }
                }
            };
            context.StudentExams.Add(pastExam);

            context.SaveChanges();
        }
    }
}

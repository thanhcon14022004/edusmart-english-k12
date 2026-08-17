// EDUSMART PLATFORM CORE JAVASCRIPT

const API_BASE = '/api';

// Application Global State
let currentUser = JSON.parse(localStorage.getItem('edusmart_user')) || {
    id: 3,
    name: 'Trần Minh Quân',
    role: 'Student',
    classId: 6,
    className: 'Lớp 6',
    avatarUrl: 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?auto=format&fit=crop&w=200&q=80'
};

let allGradeClasses = [];
let currentLevel = 2; // Default Cấp 2 (THCS)
let currentClassId = currentUser ? (currentUser.classId || 6) : 6;
let currentSubjectId = 1;
let currentLessonId = 1;
let currentExam = null;
let userAnswers = {};
let examTimerInterval = null;
let examTimeRemaining = 0;

// Global Keyboard Shortcut (Ctrl + K) for Search
document.addEventListener('keydown', (e) => {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        const searchInput = document.getElementById('global-search-input');
        if (searchInput) {
            searchInput.focus();
            searchInput.select();
        }
    }
});

// Global Search Handler
function handleGlobalSearch(e) {
    const query = e.target.value.toLowerCase().trim();
    if (!query) return;

    if (e.key === 'Enter') {
        navTo('exams-list-view');
        const examSearchInput = document.getElementById('exam-search-input');
        if (examSearchInput) {
            examSearchInput.value = query;
            searchExams();
        }
    }
}

// Resume Last Active Lesson from Sidebar Widget
async function resumeLastLesson() {
    navTo('courses-view');
}

// Initialize App on DOM Load
document.addEventListener('DOMContentLoaded', async () => {
    console.log("EduSmart K-12 Online Learning Portal Initializing...");
    
    // Restore Saved Theme if any
    const savedTheme = localStorage.getItem('edusmart_theme') || 'light';
    document.body.setAttribute('data-theme', savedTheme);
    const themeIcon = document.getElementById('theme-icon');
    if (themeIcon) {
        themeIcon.className = savedTheme === 'dark' ? 'fa-solid fa-moon' : 'fa-solid fa-sun';
    }

    updateUserProfileHeader();
    await loadGradeClassesForRegister();
    await loadClassTabs();
    await loadQuickExams();
    await loadStudentProgress();
});

// Theme Switcher Toggle
function toggleAppTheme() {
    const isDark = document.body.getAttribute('data-theme') === 'dark';
    const newTheme = isDark ? 'light' : 'dark';
    document.body.setAttribute('data-theme', newTheme);
    localStorage.setItem('edusmart_theme', newTheme);

    const themeIcon = document.getElementById('theme-icon');
    if (themeIcon) {
        themeIcon.className = isDark ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
    }
}

// Pause all playing videos when switching tabs or views
function pauseAllVideos() {
    // 1. Pause HTML5 Video elements
    document.querySelectorAll('video').forEach(v => {
        if (!v.paused) v.pause();
    });

    // 2. Pause YouTube IFrames via PostMessage API & src reset
    document.querySelectorAll('iframe').forEach(iframe => {
        try {
            iframe.contentWindow.postMessage('{"event":"command","func":"pauseVideo","args":""}', '*');
        } catch (e) {}
        
        // Backup reset src to ensure audio stops if postMessage is blocked
        const src = iframe.src;
        if (src && (src.includes('youtube.com') || src.includes('youtu.be'))) {
            iframe.src = src;
        }
    });
}

// Pause video if user switches browser tab or minimizes window
document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
        pauseAllVideos();
    }
});

// Auto-scroll to top helper when switching view tabs/courses
function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
    const mainContent = document.getElementById('main-content');
    if (mainContent) mainContent.scrollTop = 0;
}

// Navigation Helper
function navTo(viewId, event) {
    if (event) event.preventDefault();

    // Auto pause any running video when switching view tabs
    pauseAllVideos();

    // Always scroll smoothly to top when opening a new view tab
    scrollToTop();

    document.querySelectorAll('.page-view').forEach(view => view.classList.remove('active'));
    document.querySelectorAll('.nav-item').forEach(item => item.classList.remove('active'));

    const targetView = document.getElementById(`view-${viewId}`);
    if (targetView) targetView.classList.add('active');

    // Update active nav link if clicked
    if (event && event.currentTarget) {
        event.currentTarget.classList.add('active');
    }

    if (viewId === 'courses-view') {
        showCourseCatalog();
    } else if (viewId === 'progress-view') {
        loadStudentProgress();
    } else if (viewId === 'exams-list-view') {
        loadAllExams();
    }
}

function showCourseCatalog(ev) {
    if (ev) ev.preventDefault();
    pauseAllVideos();

    if (currentUser && currentUser.classId) {
        currentClassId = currentUser.classId;
    }
    loadSubjects(currentClassId);
}

// Load Class Tabs & Filter by Educational Level (Cấp 1, Cấp 2, Cấp 3)
async function loadClassTabs() {
    try {
        const res = await fetch(`${API_BASE}/courses/classes`);
        allGradeClasses = await res.json();
        renderClassTabsForLevel(currentLevel);
    } catch (err) {
        console.error("Failed to load classes:", err);
    }
}

function filterByLevel(levelNum) {
    currentLevel = levelNum;
    document.querySelectorAll('.level-tab-btn').forEach(btn => btn.classList.remove('active'));
    const activeLevelBtn = document.getElementById(`level-btn-${levelNum}`);
    if (activeLevelBtn) activeLevelBtn.classList.add('active');

    renderClassTabsForLevel(levelNum);
}

function renderClassTabsForLevel(levelNum) {
    const container = document.getElementById('class-tabs');
    if (!container) return;

    let filtered = [];
    if (levelNum === 1) { // Cấp 1 (Tiểu học)
        filtered = allGradeClasses.filter(c => ['Lớp 1', 'Lớp 2', 'Lớp 3', 'Lớp 4', 'Lớp 5'].includes(c.name));
    } else if (levelNum === 2) { // Cấp 2 (THCS)
        filtered = allGradeClasses.filter(c => ['Lớp 6', 'Lớp 7', 'Lớp 8', 'Lớp 9'].includes(c.name));
    } else if (levelNum === 3) { // Cấp 3 (THPT)
        filtered = allGradeClasses.filter(c => ['Lớp 10', 'Lớp 11', 'Lớp 12'].includes(c.name));
    }

    if (!filtered || filtered.length === 0) filtered = allGradeClasses;

    container.innerHTML = filtered.map(c => `
        <button class="tab-btn ${c.id === currentClassId ? 'active' : ''}" onclick="selectClass(${c.id}, event)">
            ${c.name}
        </button>
    `).join('');

    // Check if current class ID belongs to filtered list
    const match = filtered.find(c => c.id === currentClassId);
    if (!match && filtered.length > 0) {
        selectClass(filtered[0].id);
    } else {
        loadSubjects(currentClassId);
    }
}

// Select Class
async function selectClass(classId, ev) {
    currentClassId = classId;
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    if (ev && ev.currentTarget) {
        ev.currentTarget.classList.add('active');
    } else {
        // Find tab-btn matching onclick and set active
        const tabs = document.querySelectorAll('#class-tabs .tab-btn');
        tabs.forEach(btn => {
            if (btn.getAttribute('onclick') && btn.getAttribute('onclick').includes(`selectClass(${classId}`)) {
                btn.classList.add('active');
            }
        });
    }
    await loadSubjects(classId);
}

// Load Subjects Grid
async function loadSubjects(classId) {
    try {
        const res = await fetch(`${API_BASE}/courses/subjects?classId=${classId}`);
        const subjects = await res.json();
        const container = document.getElementById('subjects-container');

        if (!subjects || subjects.length === 0) {
            container.innerHTML = `
                <div class="glass-card text-center" style="grid-column: 1 / -1; padding: 2rem;">
                    <i class="fa-solid fa-shapes" style="font-size: 2.5rem; color: var(--accent-primary); margin-bottom: 1rem;"></i>
                    <h4 style="margin-bottom: 0.5rem;">Đang cập nhật nội dung môn học</h4>
                    <p class="text-muted">Chương trình môn học cho lớp này đang được các thầy cô cập nhật liên tục.</p>
                </div>
            `;
            return;
        }

        container.innerHTML = subjects.map(s => `
            <div class="subject-card" style="--card-color: ${s.color};" onclick="openSubject(${s.id}, '${s.name}')">
                <div class="subject-card-header">
                    <div class="subject-icon-box" style="color: ${s.color}; background: ${s.color}15;">
                        <i class="fa-solid fa-${s.icon}"></i>
                    </div>
                    <div>
                        <h4>${s.name}</h4>
                        <span class="badge badge-purple"><i class="fa-solid fa-book-open"></i> ${s.chapterCount || 2} Chương học</span>
                    </div>
                </div>
                <p>${s.description}</p>
                <div class="progress-bar-container">
                    <div class="progress-bar" style="width: 60%; background: ${s.color};"></div>
                </div>
                <span style="font-size: 0.78rem; font-weight: 600; color: var(--text-muted); margin-top: 0.5rem; display: block;">
                    <i class="fa-solid fa-circle-check" style="color: var(--accent-emerald);"></i> Tiến độ học tập: 60%
                </span>
            </div>
        `).join('');
    } catch (err) {
        console.error("Failed to load subjects:", err);
    }
}

// Open Subject & Load Lesson Tree Player (Switches to Dedicated Lesson Player View Page)
async function openSubject(subjectId, subjectName) {
    pauseAllVideos();
    currentSubjectId = subjectId;
    document.getElementById('breadcrumb-subject').innerText = subjectName;
    document.getElementById('course-title-sidebar').innerText = subjectName;

    navTo('lesson-player-view');

    try {
        const res = await fetch(`${API_BASE}/courses/subjects/${subjectId}/tree?studentId=${currentUser.id}`);
        const data = await res.json();

        const accordionContainer = document.getElementById('chapters-accordion-container');
        
        let html = '';
        let firstLessonId = null;

        data.chapters.forEach(chap => {
            html += `
                <div class="chapter-item">
                    <h4>${chap.title}</h4>
                    <div class="lessons-list">
            `;
            chap.lessons.forEach(l => {
                if (!firstLessonId) firstLessonId = l.id;
                html += `
                    <button class="lesson-item-btn ${l.id === currentLessonId ? 'active' : ''}" onclick="selectLesson(${l.id})">
                        <span><i class="fa-regular fa-${l.contentType === 'video' ? 'circle-play' : 'file-lines'}"></i> ${l.title}</span>
                        ${l.isCompleted ? '<i class="fa-solid fa-circle-check color-emerald"></i>' : ''}
                    </button>
                `;
            });
            html += `</div></div>`;
        });

        accordionContainer.innerHTML = html;

        if (firstLessonId) {
            selectLesson(firstLessonId);
        }
    } catch (err) {
        console.error("Failed to load course tree:", err);
    }
}

// Select & Render Lesson Detail
async function selectLesson(lessonId) {
    currentLessonId = lessonId;
    scrollToTop();

    const lessonBox = document.getElementById('lesson-display-box');
    if (lessonBox) lessonBox.scrollTop = 0;

    document.querySelectorAll('.lesson-item-btn').forEach(btn => btn.classList.remove('active'));

    try {
        const res = await fetch(`${API_BASE}/courses/lessons/${lessonId}?studentId=${currentUser.id}`);
        const lesson = await res.json();

        document.getElementById('lesson-active-title').innerText = lesson.title;
        document.getElementById('lesson-article-content').innerText = lesson.contentText || "Nội dung bài học dạng tài liệu trực quan.";

        const videoWrapper = document.getElementById('video-wrapper');
        if (lesson.videoUrl) {
            videoWrapper.style.display = 'block';
            document.getElementById('lesson-video-iframe').src = lesson.videoUrl;
        } else {
            videoWrapper.style.display = 'none';
        }

        const completeBtn = document.getElementById('btn-complete-lesson');
        const completeText = document.getElementById('complete-btn-text');

        if (lesson.isCompleted) {
            completeBtn.className = 'btn btn-outline';
            completeText.innerText = 'Đã Hoàn Thành ✓';
        } else {
            completeBtn.className = 'btn btn-success';
            completeText.innerText = 'Đánh Dấu Đã Học';
        }

        // Render In-Lesson Practice Questions
        const quizContainer = document.getElementById('lesson-quiz-container');
        if (quizContainer) {
            if (!lesson.practiceQuestions || lesson.practiceQuestions.length === 0) {
                quizContainer.innerHTML = '<p class="text-muted" style="font-size: 0.9rem;">Bài học này không có câu hỏi trắc nghiệm trực tiếp.</p>';
            } else {
                quizContainer.innerHTML = lesson.practiceQuestions.map((q, idx) => `
                    <div class="quiz-question-card" id="quiz-card-${q.id}">
                        <h4>Câu ${idx + 1}: ${q.content}</h4>
                        <div class="options-list">
                            ${q.options.map(opt => `
                                <div class="option-item" id="lesson-opt-${q.id}-${opt.id}" onclick="checkPracticeAnswer(${q.id}, ${opt.id}, ${q.options.find(o => o.isCorrect)?.id || 0}, '${(q.explanation || '').replace(/'/g, "\\'")}')">
                                    <div class="option-radio"></div>
                                    <span>${opt.optionText}</span>
                                </div>
                            `).join('')}
                        </div>
                        <div class="explanation-box hidden" id="lesson-exp-${q.id}" style="display: none;"></div>
                    </div>
                `).join('');
            }
        }

    } catch (err) {
        console.error("Failed to load lesson detail:", err);
    }
}

// Check Practice Question Answer in Lesson
function checkPracticeAnswer(questionId, selectedOptId, correctOptId, explanationText) {
    const card = document.getElementById(`quiz-card-${questionId}`);
    if (!card) return;

    card.querySelectorAll('.option-item').forEach(opt => {
        opt.classList.remove('selected', 'correct', 'wrong');
        opt.style.borderColor = 'var(--border-color)';
        opt.style.background = 'transparent';
    });

    const selectedEl = document.getElementById(`lesson-opt-${questionId}-${selectedOptId}`);
    const expBox = document.getElementById(`lesson-exp-${questionId}`);

    if (selectedOptId === correctOptId) {
        if (selectedEl) {
            selectedEl.style.borderColor = 'var(--accent-emerald)';
            selectedEl.style.background = 'rgba(16, 185, 129, 0.2)';
        }
        if (expBox) {
            expBox.style.display = 'block';
            expBox.innerHTML = `<i class="fa-solid fa-circle-check color-emerald"></i> <strong>Chính xác!</strong> ${explanationText || ''}`;
        }
    } else {
        if (selectedEl) {
            selectedEl.style.borderColor = 'var(--accent-rose)';
            selectedEl.style.background = 'rgba(244, 63, 94, 0.2)';
        }
        const correctEl = document.getElementById(`lesson-opt-${questionId}-${correctOptId}`);
        if (correctEl) {
            correctEl.style.borderColor = 'var(--accent-emerald)';
            correctEl.style.background = 'rgba(16, 185, 129, 0.15)';
        }
        if (expBox) {
            expBox.style.display = 'block';
            expBox.innerHTML = `<i class="fa-solid fa-circle-xmark color-pink"></i> <strong>Chưa chính xác.</strong> ${explanationText || 'Hãy thử lại câu khác nhé!'}`;
        }
    }
}

// Toggle Complete Current Lesson
async function toggleCompleteCurrentLesson() {
    try {
        const res = await fetch(`${API_BASE}/progress/toggle-lesson`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ studentId: currentUser.id, lessonId: currentLessonId })
        });
        const result = await res.json();
        
        await selectLesson(currentLessonId);
        await openSubject(currentSubjectId, document.getElementById('breadcrumb-subject').innerText);
    } catch (err) {
        console.error("Failed to toggle lesson progress:", err);
    }
}

// Load Quick Exams for Home Screen
async function loadQuickExams() {
    try {
        const res = await fetch(`${API_BASE}/exams`);
        const exams = await res.json();
        const container = document.getElementById('quick-exams-container');

        if (!exams || exams.length === 0) {
            container.innerHTML = '<p class="text-muted">Chưa có bài thi nào.</p>';
            return;
        }

        container.innerHTML = exams.map(e => `
            <div class="exam-card">
                <div class="exam-info">
                    <h4>${e.title}</h4>
                    <span><i class="fa-regular fa-clock"></i> ${e.durationMinutes} Phút | ${e.questionCount} Câu trắc nghiệm</span>
                </div>
                <button class="btn btn-primary btn-sm" onclick="startExam(${e.id})">
                    <i class="fa-solid fa-play"></i> Làm Bài
                </button>
            </div>
        `).join('');
    } catch (err) {
        console.error("Failed to load exams:", err);
    }
}

let currentQuestionIndex = 0;

// Start Online Exam with Countdown Timer & Single-Question Step-by-Step TTS Wizard
async function startExam(examId) {
    try {
        const res = await fetch(`${API_BASE}/exams/${examId}`);
        currentExam = await res.json();
        userAnswers = {};
        currentQuestionIndex = 0;

        navTo('take-exam-view');

        document.getElementById('exam-active-title').innerText = currentExam.title;
        document.getElementById('exam-tag-type').innerText = currentExam.type || 'Bài Thi Luyện Tập';

        // Render Question Palette
        renderQuestionPalette();

        // Render Initial Active Question & Read Aloud
        renderActiveQuestion();

        // Start Timer
        examTimeRemaining = currentExam.durationMinutes * 60;
        updateTimerDisplay();

        if (examTimerInterval) clearInterval(examTimerInterval);
        examTimerInterval = setInterval(() => {
            examTimeRemaining--;
            updateTimerDisplay();
            if (examTimeRemaining <= 0) {
                clearInterval(examTimerInterval);
                alert("Đã hết thời gian làm bài! Hệ thống tự động nộp bài.");
                confirmSubmitExam();
            }
        }, 1000);

    } catch (err) {
        console.error("Failed to start exam:", err);
    }
}

// Render Single Active Question Card with Audio Reader
function renderActiveQuestion() {
    if (!currentExam || !currentExam.questions || currentExam.questions.length === 0) return;

    const q = currentExam.questions[currentQuestionIndex];
    const totalQ = currentExam.questions.length;
    const qContainer = document.getElementById('exam-questions-container');

    qContainer.innerHTML = `
        <div class="question-box single-question-card" id="q-box-${q.questionId}">
            <div class="question-header-row" style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 1.25rem;">
                <span class="badge badge-purple" style="font-size: 0.9rem; padding: 0.5rem 1rem;">
                    <i class="fa-solid fa-layer-group"></i> Câu ${currentQuestionIndex + 1} / ${totalQ} (${q.points} Điểm)
                </span>
                <button class="btn btn-sm btn-outline" onclick="speakQuestionText()" title="Nghe đọc câu hỏi Tiếng Anh" style="display: inline-flex; align-items: center; gap: 0.5rem; color: var(--accent-primary); border-color: var(--accent-primary); font-weight: 600;">
                    <i class="fa-solid fa-volume-high"></i> 🔊 Đọc Câu Hỏi
                </button>
            </div>

            <h3 class="question-title-text" style="font-size: 1.2rem; font-weight: 700; line-height: 1.6; margin-bottom: 1.5rem; color: var(--text-main);">
                ${q.content}
            </h3>

            <div class="options-list">
                ${q.options.map(opt => `
                    <div class="option-item ${userAnswers[q.questionId] === opt.id ? 'selected' : ''}" id="opt-${q.questionId}-${opt.id}" onclick="selectOption(${q.questionId}, ${opt.id})">
                        <div class="option-radio"></div>
                        <span>${opt.optionText}</span>
                    </div>
                `).join('')}
            </div>

            <!-- Single Question Step-by-Step Nav Buttons -->
            <div class="wizard-nav-bar" style="display: flex; align-items: center; justify-content: space-between; margin-top: 2rem; padding-top: 1.25rem; border-top: 1px solid var(--border-color);">
                <button class="btn btn-outline" onclick="prevQuestion()" ${currentQuestionIndex === 0 ? 'disabled style="opacity:0.4; cursor:not-allowed;"' : ''}>
                    <i class="fa-solid fa-arrow-left"></i> Câu Trước
                </button>
                
                ${currentQuestionIndex === totalQ - 1 ? `
                    <button class="btn btn-success btn-lg" onclick="confirmSubmitExam()">
                        <i class="fa-solid fa-paper-plane"></i> Nộp Bài Thi
                    </button>
                ` : `
                    <button class="btn btn-primary btn-lg" onclick="nextQuestion()">
                        Câu Tiếp <i class="fa-solid fa-arrow-right"></i>
                    </button>
                `}
            </div>
        </div>
    `;

    // Highlight Active Button in Palette
    updatePaletteActiveState();

    // Auto-read question text aloud in English
    speakQuestionText();
}

// Text-To-Speech (TTS) Voice Reader for Question
function speakQuestionText() {
    if (!('speechSynthesis' in window)) return;

    window.speechSynthesis.cancel(); // Cancel any ongoing speech

    if (!currentExam || !currentExam.questions || !currentExam.questions[currentQuestionIndex]) return;

    const q = currentExam.questions[currentQuestionIndex];
    const textToSpeak = q.content;

    const utterance = new SpeechSynthesisUtterance(textToSpeak);
    utterance.lang = 'en-US'; // English accent
    utterance.rate = 0.9; // Natural pace
    utterance.pitch = 1.0;

    window.speechSynthesis.speak(utterance);
}

// Step-by-step Navigation Methods
function goToQuestion(idx) {
    if (idx >= 0 && idx < currentExam.questions.length) {
        currentQuestionIndex = idx;
        renderActiveQuestion();
    }
}

function nextQuestion() {
    if (currentQuestionIndex < currentExam.questions.length - 1) {
        currentQuestionIndex++;
        renderActiveQuestion();
    }
}

function prevQuestion() {
    if (currentQuestionIndex > 0) {
        currentQuestionIndex--;
        renderActiveQuestion();
    }
}

function renderQuestionPalette() {
    const paletteGrid = document.getElementById('palette-grid');
    if (!paletteGrid || !currentExam) return;

    paletteGrid.innerHTML = currentExam.questions.map((q, idx) => `
        <button class="palette-btn ${userAnswers[q.questionId] ? 'answered' : ''} ${idx === currentQuestionIndex ? 'active' : ''}" id="palette-btn-${q.questionId}" onclick="goToQuestion(${idx})">
            ${idx + 1}
        </button>
    `).join('');
}

function updatePaletteActiveState() {
    if (!currentExam || !currentExam.questions) return;
    currentExam.questions.forEach((q, idx) => {
        const btn = document.getElementById(`palette-btn-${q.questionId}`);
        if (btn) {
            if (idx === currentQuestionIndex) btn.classList.add('active');
            else btn.classList.remove('active');
            
            if (userAnswers[q.questionId]) btn.classList.add('answered');
        }
    });
}

// Option selection handler
function selectOption(questionId, optionId) {
    userAnswers[questionId] = optionId;

    // Highlight selected option UI
    const qBox = document.getElementById(`q-box-${questionId}`);
    if (qBox) {
        qBox.querySelectorAll('.option-item').forEach(opt => opt.classList.remove('selected'));
        const selectedOpt = document.getElementById(`opt-${questionId}-${optionId}`);
        if (selectedOpt) selectedOpt.classList.add('selected');
    }

    // Highlight Palette Button
    const btn = document.getElementById(`palette-btn-${questionId}`);
    if (btn) btn.classList.add('answered');
}

function scrollToQuestion(questionId) {
    const idx = currentExam.questions.findIndex(q => q.questionId === questionId);
    if (idx !== -1) goToQuestion(idx);
}

function updateTimerDisplay() {
    const minutes = Math.floor(examTimeRemaining / 60);
    const seconds = examTimeRemaining % 60;
    document.getElementById('exam-timer-display').innerText = 
        `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
}

// Confirm & Submit Exam -> Trigger Automatic Grading
async function confirmSubmitExam() {
    if (examTimerInterval) clearInterval(examTimerInterval);
    if ('speechSynthesis' in window) window.speechSynthesis.cancel();

    const answersPayload = Object.keys(userAnswers).map(qId => ({
        questionId: parseInt(qId),
        selectedOptionId: userAnswers[qId]
    }));

    try {
        const res = await fetch(`${API_BASE}/exams/submit`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                studentId: currentUser.id,
                examId: currentExam.id,
                answers: answersPayload
            })
        });

        const result = await res.json();
        renderExamResult(result);
    } catch (err) {
        console.error("Failed to submit exam:", err);
    }
}

// Render Result & Explanation Review View
function renderExamResult(result) {
    navTo('exam-result-view');

    document.getElementById('result-score-val').innerText = result.score.toFixed(1);
    document.getElementById('result-exam-title').innerText = result.examTitle;
    document.getElementById('result-correct-count').innerText = `${result.correctCount}/${result.totalQuestions}`;
    document.getElementById('result-percentage').innerText = `${result.percentage}%`;

    const statusEl = document.getElementById('result-status-text');
    if (result.isPassed) {
        statusEl.innerHTML = '<span style="color: var(--accent-emerald);">🎉 Chúc mừng! Bạn đã ĐẠT bài kiểm tra xuất sắc.</span>';
    } else {
        statusEl.innerHTML = '<span style="color: var(--accent-rose);">⚠️ Bạn chưa đạt điểm yêu cầu. Hãy xem lại bài làm nhé!</span>';
    }

    // Render Answer Reviews
    const container = document.getElementById('questions-review-container');
    container.innerHTML = result.questionReviews.map((q, idx) => `
        <div class="review-item ${q.isCorrect ? 'correct' : 'wrong'}">
            <h4>Câu ${idx + 1}: ${q.questionContent} 
                <span class="badge ${q.isCorrect ? 'badge-success' : 'badge-warning'}">
                    ${q.isCorrect ? 'Chính xác +2.5đ' : 'Chưa đúng 0đ'}
                </span>
            </h4>
            <div class="options-list mt-4">
                ${q.options.map(o => {
                    let styleClass = '';
                    if (o.id === q.selectedOptionId && o.isCorrect) styleClass = 'style="border-color: var(--accent-emerald); background: rgba(16,185,129,0.2);"';
                    else if (o.id === q.selectedOptionId && !o.isCorrect) styleClass = 'style="border-color: var(--accent-rose); background: rgba(244,63,94,0.2);"';
                    else if (o.isCorrect) styleClass = 'style="border-color: var(--accent-emerald); background: rgba(16,185,129,0.1);"';

                    return `
                        <div class="option-item" ${styleClass}>
                            <span>${o.optionText} ${o.isCorrect ? '✓ (Đáp án đúng)' : ''}</span>
                        </div>
                    `;
                }).join('')}
            </div>
            ${q.explanation ? `<div class="explanation-box"><i class="fa-solid fa-lightbulb"></i> <strong>Giải thích đáp án:</strong> ${q.explanation}</div>` : ''}
        </div>
    `).join('');
}

// Load Student Progress & Stats
async function loadStudentProgress() {
    try {
        const res = await fetch(`${API_BASE}/progress/student/${currentUser.id}/summary`);
        const data = await res.json();

        const completed = data.totalCompletedLessons ?? data.TotalCompletedLessons ?? 0;
        const taken = data.totalExamsTaken ?? data.TotalExamsTaken ?? 0;
        const avg = data.averageOverallScore ?? data.AverageOverallScore ?? 0;
        const subjects = data.subjects ?? data.Subjects ?? [];

        document.getElementById('stat-completed-lessons').innerText = completed;
        document.getElementById('stat-exams-taken').innerText = taken;
        document.getElementById('stat-avg-score').innerText = typeof avg === 'number' ? avg.toFixed(1) : '0.0';

        const container = document.getElementById('subject-progress-container');
        if (container) {
            if (subjects.length === 0) {
                container.innerHTML = '<p class="text-muted">Chưa có dữ liệu tiến độ môn học.</p>';
            } else {
                container.innerHTML = subjects.map(s => {
                    const name = s.subjectName ?? s.SubjectName ?? '';
                    const avgScore = s.averageExamScore ?? s.AverageExamScore ?? 0;
                    const compPct = s.completionPercentage ?? s.CompletionPercentage ?? 0;
                    const compLessons = s.completedLessons ?? s.CompletedLessons ?? 0;
                    const totLessons = s.totalLessons ?? s.TotalLessons ?? 0;

                    return `
                        <div class="glass-card mb-4" style="margin-bottom: 1rem;">
                            <div class="section-header">
                                <h4>${name}</h4>
                                <span class="badge badge-purple">Điểm TB: ${(typeof avgScore === 'number' ? avgScore : 0).toFixed(1)}/10</span>
                            </div>
                            <div class="progress-bar-container" style="height: 12px;">
                                <div class="progress-bar" style="width: ${compPct}%;"></div>
                            </div>
                            <div style="display: flex; justify-content: space-between; font-size: 0.85rem; color: var(--text-muted); margin-top: 0.5rem;">
                                <span>Hoàn thành ${compLessons}/${totLessons} Bài học</span>
                                <span>${compPct}%</span>
                            </div>
                        </div>
                    `;
                }).join('');
            }
        }

        await loadStudentExamHistory();
    } catch (err) {
        console.error("Failed to load progress summary:", err);
    }
}

// Load Student Exam Attempt History Log
async function loadStudentExamHistory() {
    try {
        const res = await fetch(`${API_BASE}/exams/results/student/${currentUser.id}`);
        const history = await res.json();
        const tbody = document.getElementById('exam-history-tbody');
        if (!tbody) return;

        if (!history || history.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-muted text-center">Chưa có nhật ký nộp bài thi nào.</td></tr>';
            return;
        }

        tbody.innerHTML = history.map(h => {
            const title = h.examTitle ?? h.ExamTitle ?? 'Bài Thi';
            const dateStr = h.submittedAt ?? h.SubmittedAt ?? new Date();
            const score = h.score ?? h.Score ?? 0;
            const maxScore = h.maxScore ?? h.MaxScore ?? 10.0;
            const isPassed = h.isPassed ?? h.IsPassed ?? false;

            return `
                <tr>
                    <td><strong>${title}</strong></td>
                    <td>${new Date(dateStr).toLocaleString('vi-VN')}</td>
                    <td><strong style="color: ${isPassed ? 'var(--accent-emerald)' : 'var(--accent-rose)'}">${(typeof score === 'number' ? score : 0).toFixed(1)} / ${maxScore}</strong></td>
                    <td><span class="badge ${isPassed ? 'badge-success' : 'badge-warning'}">${isPassed ? 'ĐẠT' : 'CHƯA ĐẠT'}</span></td>
                </tr>
            `;
        }).join('');
    } catch (err) {
        console.error("Failed to load exam history:", err);
    }
}

let allExamsList = [];

// Load All Exams for Exams List View
async function loadAllExams() {
    try {
        const res = await fetch(`${API_BASE}/exams`);
        allExamsList = await res.json();
        renderExamsList(allExamsList);
    } catch (err) {
        console.error("Failed to load exams list:", err);
    }
}

// Search Exams by Title Query
function searchExams(query) {
    if (!query) {
        renderExamsList(allExamsList);
        return;
    }
    const q = query.toLowerCase().trim();
    const filtered = allExamsList.filter(e => e.title.toLowerCase().includes(q) || (e.subjectName && e.subjectName.toLowerCase().includes(q)));
    renderExamsList(filtered);
}

// Filter Exams by Type
function filterExams(type) {
    document.querySelectorAll('#exam-filter-tabs .tab-btn').forEach(btn => btn.classList.remove('active'));
    if (event && event.currentTarget) event.currentTarget.classList.add('active');

    if (type === 'all') {
        renderExamsList(allExamsList);
    } else {
        const filtered = allExamsList.filter(e => e.type === type);
        renderExamsList(filtered);
    }
}

// Render Exams Grid
function renderExamsList(exams) {
    const container = document.getElementById('all-exams-container');
    if (!container) return;

    if (!exams || exams.length === 0) {
        container.innerHTML = '<p class="text-muted">Không tìm thấy bài thi nào phù hợp.</p>';
        return;
    }

    container.innerHTML = exams.map(e => `
        <div class="exam-card glass-card">
            <div class="exam-info">
                <span class="badge ${e.type === 'Final' ? 'badge-warning' : e.type === 'Midterm' ? 'badge-purple' : 'badge-pro'} mb-2" style="margin-bottom: 0.5rem; display: inline-block;">
                    ${e.type === 'Final' ? 'Thi Cuối Kỳ' : e.type === 'Midterm' ? 'Kiểm Tra Giữa Kỳ' : 'Bài Thi Luyện Tập'}
                </span>
                <h4>${e.title}</h4>
                <p class="text-muted mb-3" style="margin-bottom: 0.75rem; color: var(--text-muted);"><i class="fa-solid fa-book"></i> Môn: ${e.subjectName || 'Tổng hợp'}</p>
                <div style="font-size: 0.85rem; color: var(--text-muted); display: flex; gap: 1rem; flex-wrap: wrap;">
                    <span><i class="fa-regular fa-clock"></i> ${e.durationMinutes} Phút</span>
                    <span><i class="fa-solid fa-list-check"></i> ${e.questionCount} Câu trắc nghiệm</span>
                    <span><i class="fa-solid fa-star"></i> Đạt: ${e.passScore}/10</span>
                </div>
            </div>
            <div style="margin-top: 1.25rem;">
                <button class="btn btn-primary btn-block" onclick="startExam(${e.id})">
                    <i class="fa-solid fa-play"></i> Bắt Đầu Làm Bài
                </button>
            </div>
        </div>
    `).join('');
}

// AUTHENTICATION & SESSION MANAGEMENT
function updateUserProfileHeader() {
    const badge = document.getElementById('user-profile-badge');
    if (!badge) return;

    const className = currentUser ? (currentUser.className || ('Lớp ' + (currentUser.classId || 6))) : 'Lớp 6';
    const userName = currentUser ? currentUser.name : 'Trần Minh Quân';

    // Update Assigned Class Banner elements ("Đăng ký lớp nào thì học lớp đó")
    const classNameDisplay = document.getElementById('assigned-class-name-display');
    const userNameDisplay = document.getElementById('assigned-user-name-display');
    const classBadge = document.getElementById('assigned-class-badge');

    if (classNameDisplay) classNameDisplay.innerText = className;
    if (userNameDisplay) userNameDisplay.innerText = userName;
    if (classBadge) classBadge.innerText = className;

    if (currentUser && currentUser.id) {
        currentClassId = currentUser.classId || 6;
        badge.innerHTML = `
            <img id="user-avatar" src="${currentUser.avatarUrl || 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=200&q=80'}" alt="Avatar" class="avatar">
            <div class="user-info">
                <span class="user-name" id="user-name">${currentUser.name}</span>
                <span class="user-role-tag" id="user-role-tag">Học sinh - ${className}</span>
            </div>
            <button class="btn btn-sm btn-outline" onclick="handleLogout()" title="Đăng Xuất" style="margin-left: 0.75rem; border-color: rgba(255,255,255,0.2);">
                <i class="fa-solid fa-right-from-bracket"></i> Đăng Xuất
            </button>
        `;
    } else {
        badge.innerHTML = `
            <button class="btn btn-primary btn-sm" onclick="openAuthModal('login')">
                <i class="fa-solid fa-right-to-bracket"></i> Đăng Nhập / Đăng Ký
            </button>
        `;
    }
}

function handleLogout() {
    localStorage.removeItem('edusmart_user');
    currentUser = null;
    updateUserProfileHeader();
    openAuthModal('login');
}

function openAuthModal(tab = 'login') {
    const modal = document.getElementById('auth-modal');
    if (modal) modal.classList.remove('hidden');
    switchAuthTab(tab);
}

function closeAuthModal() {
    const modal = document.getElementById('auth-modal');
    if (modal) modal.classList.add('hidden');
    showAuthAlert(null);
}

function switchAuthTab(tab) {
    const loginForm = document.getElementById('login-form');
    const regForm = document.getElementById('register-form');
    const loginBtn = document.getElementById('tab-login-btn');
    const regBtn = document.getElementById('tab-register-btn');

    showAuthAlert(null);

    if (tab === 'login') {
        if (loginForm) loginForm.classList.remove('hidden');
        if (regForm) regForm.classList.add('hidden');
        if (loginBtn) loginBtn.classList.add('active');
        if (regBtn) regBtn.classList.remove('active');
    } else {
        if (loginForm) loginForm.classList.add('hidden');
        if (regForm) regForm.classList.remove('hidden');
        if (loginBtn) loginBtn.classList.remove('active');
        if (regBtn) regBtn.classList.add('active');
    }
}

function showAuthAlert(msg) {
    const alertBox = document.getElementById('auth-alert-msg');
    if (!alertBox) return;
    if (!msg) {
        alertBox.classList.add('hidden');
        alertBox.innerText = '';
    } else {
        alertBox.classList.remove('hidden');
        alertBox.innerText = msg;
    }
}

async function loadGradeClassesForRegister() {
    const select = document.getElementById('reg-gradeclass');
    if (!select) return;
    try {
        const res = await fetch(`${API_BASE}/courses/classes`);
        const classes = await res.json();
        select.innerHTML = classes.map(c => `<option value="${c.id}">${c.name}</option>`).join('');
    } catch (e) {
        console.error("Failed to load grade classes for register:", e);
        select.innerHTML = `
            <option value="1">Lớp 6</option>
            <option value="2">Lớp 7</option>
            <option value="3">Lớp 8</option>
            <option value="4">Lớp 9</option>
        `;
    }
}

async function submitLogin(e) {
    if (e) e.preventDefault();
    const emailOrUsername = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;

    showAuthAlert(null);

    try {
        const res = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email: emailOrUsername, password: password })
        });

        if (!res.ok) {
            const err = await res.json();
            showAuthAlert(err.message || 'Đăng nhập không thành công.');
            return;
        }

        const data = await res.json();
        setLoggedInUser({
            id: data.userId,
            name: data.fullName,
            role: data.role,
            classId: data.gradeClassId || 1,
            className: data.gradeClassName || 'Lớp 6',
            avatarUrl: data.avatarUrl
        });

        closeAuthModal();
    } catch (err) {
        console.error("Login failed:", err);
        showAuthAlert("Không thể kết nối đến máy chủ.");
    }
}

async function submitRegister(e) {
    if (e) e.preventDefault();
    const fullName = document.getElementById('reg-fullname').value;
    const username = document.getElementById('reg-username').value;
    const email = document.getElementById('reg-email').value;
    const gradeClassId = parseInt(document.getElementById('reg-gradeclass').value, 10);
    const password = document.getElementById('reg-password').value;

    showAuthAlert(null);

    try {
        const res = await fetch(`${API_BASE}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                fullName: fullName,
                username: username,
                email: email,
                gradeClassId: gradeClassId,
                password: password
            })
        });

        if (!res.ok) {
            const err = await res.json();
            showAuthAlert(err.message || 'Đăng ký không thành công.');
            return;
        }

        const data = await res.json();
        setLoggedInUser({
            id: data.userId,
            name: data.fullName,
            role: data.role,
            classId: data.gradeClassId,
            className: data.gradeClassName,
            avatarUrl: data.avatarUrl
        });

        closeAuthModal();
    } catch (err) {
        console.error("Register failed:", err);
        showAuthAlert("Không thể kết nối đến máy chủ.");
    }
}

function quickLogin(username, password) {
    document.getElementById('login-username').value = username;
    document.getElementById('login-password').value = password;
    submitLogin();
}

async function setLoggedInUser(userData) {
    currentUser = userData;
    localStorage.setItem('edusmart_user', JSON.stringify(userData));
    updateUserProfileHeader();

    // AUTO-SWITCH CLASS TO REGISTERED CLASS ("Đăng ký lớp nào thì đăng nhập lớp đó")
    if (userData.classId) {
        currentClassId = userData.classId;
        await loadClassTabs();
        await loadSubjects(currentClassId);
        await loadStudentProgress();
        navTo('student-dashboard');
    }
}

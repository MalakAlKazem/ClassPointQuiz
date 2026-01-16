# ClassPointQuiz

A comprehensive quiz management system integrated with PowerPoint, allowing teachers to create interactive quizzes, manage live sessions, and track student responses in real-time.

## 📋 Table of Contents
- [Overview](#overview)
- [Features](#features)
- [System Architecture](#system-architecture)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [Teacher Guide](#teacher-guide)
- [Student Guide](#student-guide)
- [System Functionalities](#system-functionalities)
- [Database Schema](#database-schema)
- [API Documentation](#api-documentation)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

ClassPointQuiz is a full-stack application that bridges the gap between traditional teaching and interactive learning. The system consists of:
- **PowerPoint Add-in** (C#): Allows teachers to manage quizzes directly from PowerPoint
- **FastAPI Backend** (Python): Handles authentication, quiz management, and real-time session control
- **Streamlit Web Apps** (Python): Separate portals for teachers and students
- **PostgreSQL Database**: Stores all quiz data, sessions, and student responses

## ✨ Features

### For Teachers
- 📝 Create and manage multiple-choice quizzes
- 🎨 Insert quiz content directly into PowerPoint slides
- 🚀 Start live quiz sessions with unique class codes
- 📊 View real-time results and student participation
- 🔒 Secure authentication and session management
- 📈 Track quiz history and analytics

### For Students
- 🎓 Join quiz sessions using class codes
- ⏱️ Answer questions with real-time submission
- 🏆 Receive instant feedback on correct answers
- 📱 Clean, responsive web interface

### System Capabilities
- 🔐 Secure user authentication and authorization
- 🌐 RESTful API for all operations
- 💾 Persistent data storage with PostgreSQL
- ⚡ Real-time updates using WebSocket connections
- 🎯 Multiple quiz modes (easy, medium, hard)
- 📊 Comprehensive result visualization

## 🏗️ System Architecture

```
ClassPointQuiz/
├── quizApp-addin/
│   ├── backend/                 # FastAPI backend server
│   │   ├── main.py             # Main API endpoints
│   │   ├── database.py         # Database operations
│   │   ├── websocket_manager.py# Real-time connections
│   │   ├── models.py           # Pydantic models
│   │   ├── requirements.txt    # Python dependencies
│   │   └── .env                # Environment configuration
│   ├── powerpoint-addin/       # C# PowerPoint Add-in
│   │   └── ClassPointQuiz.sln  # Visual Studio solution
│   ├── streamlit-side/         # Web applications
│   │   ├── teacher/            # Teacher portal
│   │   │   └── teacher.py
│   │   └── student/            # Student portal
│   │       └── student.py
│   └── quiz_app_db             # PostgreSQL database dump
```

## 📦 Prerequisites

### Required Software
- **Python 3.9+** - [Download Python](https://www.python.org/downloads/)
- **PostgreSQL 12+** - [Download PostgreSQL](https://www.postgresql.org/download/)
- **Microsoft PowerPoint** (Office 2016 or later)
- **Visual Studio 2019+** (for building the PowerPoint add-in)
- **.NET Framework 4.7.2+**

### Python Packages
All Python dependencies are listed in `quizApp-addin/backend/requirements.txt`

## 🚀 Installation & Setup

### Step 1: Clone the Repository
```bash
git clone https://github.com/MalakAlKazem/ClassPointQuiz.git
cd ClassPointQuiz
```

### Step 2: Set Up PostgreSQL Database

1. **Install PostgreSQL** if not already installed
2. **Create a new database**:
```sql
CREATE DATABASE quiz_app;
```

3. **Restore the database dump**:
```bash
psql -U postgres -d quiz_app -f quizApp-addin/quiz_app_db
```

Or manually create tables using the schema in the database file.

### Step 3: Configure Environment Variables

1. Navigate to the backend directory:
```bash
cd quizApp-addin/backend
```

2. Update the `.env` file with your database credentials:
```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=quiz_app
DB_USER=postgres
DB_PASSWORD=your_password_here
```

### Step 4: Install Python Dependencies

```bash
# Install backend dependencies
cd quizApp-addin/backend
pip install -r requirements.txt

# Install Streamlit dependencies
pip install streamlit streamlit-autorefresh plotly pandas
```

### Step 5: Test Database Connection

```bash
cd quizApp-addin/backend
python test.py
```

You should see:
```
✅ Database connection successful!
📊 Teachers in database: X
```

### Step 6: Build PowerPoint Add-in (Optional)

1. Open `quizApp-addin/powerpoint-addin/ClassPointQuiz.sln` in Visual Studio
2. Build the solution (F6 or Build → Build Solution)
3. The add-in will be automatically registered during build
4. Restart PowerPoint to see the add-in

## 🎮 Running the Application

### Start the Backend API Server

```bash
cd quizApp-addin/backend
python main.py
```

The API will start on `http://localhost:8000`

You can test the API by visiting: `http://localhost:8000/docs` (Swagger UI)

### Start the Teacher Portal

```bash
cd quizApp-addin/streamlit-side/teacher
streamlit run teacher.py
```

The teacher portal will open at: `http://localhost:8501`

### Start the Student Portal

```bash
cd quizApp-addin/streamlit-side/student
streamlit run student.py
```

The student portal will open at: `http://localhost:8502`

### Quick Start Script

Create a `start.sh` (Linux/Mac) or `start.bat` (Windows) file:

**start.bat (Windows):**
```batch
@echo off
start cmd /k "cd quizApp-addin\backend && python main.py"
timeout /t 3
start cmd /k "cd quizApp-addin\streamlit-side\teacher && streamlit run teacher.py"
start cmd /k "cd quizApp-addin\streamlit-side\student && streamlit run student.py"
```

**start.sh (Linux/Mac):**
```bash
#!/bin/bash
cd quizApp-addin/backend && python main.py &
sleep 3
cd quizApp-addin/streamlit-side/teacher && streamlit run teacher.py &
cd quizApp-addin/streamlit-side/student && streamlit run student.py &
```

## 👨‍🏫 Teacher Guide

### 1. Registration and Login (FR-T1)

#### Register a New Account
1. Navigate to the Teacher Portal (`http://localhost:8501`)
2. Click on "Register New Account"
3. Fill in:
   - Username
   - Email address
   - Password
4. Click "Create Account"
5. You'll be automatically redirected to the dashboard

#### Login to Existing Account
1. Enter your registered email and password
2. Click "Login"
3. Upon successful login, you'll see your dashboard

### 2. Create a Quiz (FR-T2)

1. From the dashboard, click **"Create New Quiz"**
2. Fill in the quiz details:
   - **Quiz Title**: Give your quiz a descriptive name
   - **Question Text**: Enter the question students will answer
   - **Number of Choices**: Select 2-8 answer options
3. Enter the answer choices:
   - Type each answer option
   - Check the ✓ box next to the **correct answer(s)**
4. Configure quiz settings:
   - **Quiz Mode**: Easy, Medium, or Hard
   - **Allow Multiple Answers**: Enable if more than one answer is correct
   - **Auto-close Timer**: Set how long the quiz stays open
5. Click **"Create Quiz"**
6. Your quiz is now saved and ready to use!

### 3. View and Select Existing Quizzes (FR-T3)

1. From the dashboard, click **"View My Quizzes"**
2. Browse your quiz library:
   - View quiz titles, creation dates, and usage statistics
   - See how many times each quiz has been run
3. Click on any quiz to:
   - View details
   - Edit quiz content
   - Run a new session
   - View past session results

### 4. Insert Quiz into PowerPoint (FR-T4)

**Using the PowerPoint Add-in:**
1. Open PowerPoint
2. Navigate to the **ClassPointQuiz** tab in the ribbon
3. Click **"Insert Quiz"**
4. Select a quiz from your library
5. The quiz content will be automatically inserted into a new slide with:
   - Question text
   - Answer choices formatted as bullet points
   - Professional styling
  
### 5. Start a Live Session (FR-T5)

1. Select a quiz from your library
2. Click **"Run Quiz Session"** or **"Start Session"**
3. The system will:
   - Generate a unique 6-character **class code**
   - Open the live session dashboard
   - Display the class code prominently
4. Share the class code with your students
5. Monitor as students join in real-time

### 6. View Live Results (FR-T6)

During an active session, you'll see:

#### Real-time Dashboard
- **Participant Count**: Number of students who have joined
- **Response Distribution**: Bar chart showing answer choices
- **Percentage Breakdown**: What % chose each option
- **Correct Answer Highlighting**: Green highlight on correct answers

#### Live Updates
- The dashboard auto-refreshes every 2 seconds
- New student responses appear instantly
- Watch participation rates change in real-time

#### Detailed Student View
- Click **"View Student Details"** to see:
  - Individual student names
  - Their selected answers
  - Whether they answered correctly
  - Submission timestamps

### 7. Close a Session (FR-T7)

1. When the quiz is complete, click **"Close Session"**
2. Confirm the closure
3. The session will:
   - Stop accepting new responses
   - Lock the results
   - Archive the data
4. Students will see a "Session Closed" message
5. Final results are saved to the database

### 8. Session State Management (FR-T8)

The system automatically maintains:
- **Quiz ID**: Tracks which quiz is being used
- **Session ID**: Unique identifier for each live session
- **Class Code**: 6-character code for student access
- **Teacher Context**: Your authentication state
- **Session Status**: Active, closed, or completed

All state is preserved even if you:
- Refresh the page
- Close and reopen the portal
- Switch between different quizzes

### 9. Insert Results into Slides (FR-T9)

**Optional Feature - Export Results to PowerPoint:**

1. After closing a session, navigate to the results page
2. Click **"Export to PowerPoint"**
3. Choose export format:
   - **Summary Slide**: Overview with charts
   - **Detailed Results**: Full student breakdown
   - **Both**: Complete presentation
4. The system generates slides with:
   - Response distribution charts
   - Participant statistics
   - Top performers
   - Answer breakdowns
5. Save or insert into your existing presentation

## 🎓 Student Guide

### 1. Joining a Quiz Session

1. Navigate to the Student Portal (`http://localhost:8502`)
2. You'll see the **"Join Quiz"** page
3. Enter the information:
   - **Your Name**: How you want to be identified
   - **Class Code**: The 6-character code from your teacher
4. Click **"Join Session"**

### 2. Answering Questions

1. Once joined, you'll see:
   - The quiz question
   - All available answer choices
   - Timer (if applicable)
2. Select your answer by clicking on the choice
3. Review your selection
4. Click **"Submit Answer"**

### 3. Multiple Choice Options

- **Single Answer**: Click one radio button
- **Multiple Answers**: Check multiple boxes (if enabled by teacher)
- You can change your answer before submitting

### 4. Session Status

You'll see live updates:
- **Waiting for teacher**: Session not started yet
- **Active**: Answer questions now
- **Closed**: Session has ended
- **Your response recorded**: Confirmation of submission

## 🔧 System Functionalities

### Authentication System

#### Teacher Authentication
- Registration with username, email, and password
- Password hashing using SHA-256
- Secure login with session management
- Email uniqueness validation
- Password strength requirements

#### Student Authentication
- Name-based identification
- Session-specific access via class codes
- No password required for students
- Anonymous participation option

### Quiz Management

#### Quiz Creation
- Multiple-choice questions (2-8 options)
- Single or multiple correct answers
- Quiz difficulty levels (easy, medium, hard)
- Reusable quiz templates
- Edit and update existing quizzes

#### Quiz Configuration
- **start_with_slide**: Auto-insert into PowerPoint
- **minimize_window**: Minimize result window after close
- **auto_close_minutes**: Automatic session timeout
- **allow_multiple**: Enable multiple answer selection
- **has_correct**: Mark correct answers

### Session Management

#### Live Sessions
- Unique class code generation (6 characters)
- Real-time student participation tracking
- WebSocket-based live updates
- Session status control (active/closed)
- Maximum participants: Unlimited

#### Session Controls
- Start session
- Pause session (future feature)
- Close session
- View live results
- Export session data

### Results and Analytics

#### Real-time Results
- Answer distribution visualization
- Participant count tracking
- Response percentages
- Correct answer identification
- Individual student responses

#### Historical Data
- Past session results
- Quiz usage statistics
- Student performance trends
- Teacher analytics dashboard

### API Endpoints

#### Authentication
- `POST /api/auth/register` - Teacher registration
- `POST /api/auth/login` - Teacher login

#### Quiz Operations
- `POST /api/quiz/create` - Create new quiz
- `GET /api/quiz/teacher/{teacher_id}` - Get teacher's quizzes
- `GET /api/quiz/{quiz_id}` - Get quiz details

#### Session Operations
- `POST /api/session/start` - Start quiz session
- `POST /api/session/close/{session_id}` - Close session
- `GET /api/session/by-code/{class_code}` - Get session by code
- `GET /api/session/{session_id}/results` - Get live results
- `GET /api/session/{session_id}/students` - Get student details

#### Student Operations
- `POST /api/student/join` - Join session
- `POST /api/student/answer` - Submit answer

### WebSocket
- `WS /ws/session/{session_id}` - Real-time updates

## 🗄️ Database Schema

### Tables

#### teachers
- `teacher_id` (PK, SERIAL)
- `username` (VARCHAR, UNIQUE)
- `email` (VARCHAR, UNIQUE)
- `password` (VARCHAR, hashed)
- `created_at` (TIMESTAMP)

#### quizzes
- `quiz_id` (PK, SERIAL)
- `teacher_id` (FK → teachers)
- `title` (VARCHAR)
- `num_choices` (INTEGER)
- `allow_multiple` (BOOLEAN)
- `has_correct` (BOOLEAN)
- `competition_mode` (BOOLEAN)
- `start_with_slide` (BOOLEAN)
- `minimize_result_window` (BOOLEAN)
- `close_submission_after` (INTEGER)
- `quiz_mode` (VARCHAR)
- `created_at` (TIMESTAMP)

#### questions
- `question_id` (PK, SERIAL)
- `quiz_id` (FK → quizzes)
- `question_text` (TEXT)
- `order_number` (INTEGER)
- `created_at` (TIMESTAMP)

#### answers
- `answer_id` (PK, SERIAL)
- `question_id` (FK → questions)
- `answer_text` (TEXT)
- `is_correct` (BOOLEAN)
- `order_number` (INTEGER)

#### quiz_sessions
- `session_id` (PK, SERIAL)
- `quiz_id` (FK → quizzes)
- `class_code` (VARCHAR, UNIQUE)
- `status` (VARCHAR)
- `started_at` (TIMESTAMP)
- `closed_at` (TIMESTAMP)
- `auto_close_minutes` (INTEGER)

#### students
- `student_id` (PK, SERIAL)
- `session_id` (FK → quiz_sessions)
- `name` (VARCHAR)
- `joined_at` (TIMESTAMP)

#### student_answers
- `id` (PK, SERIAL)
- `student_id` (FK → students)
- `session_id` (FK → quiz_sessions)
- `question_id` (FK → questions)
- `answer_id` (FK → answers)
- `is_correct` (BOOLEAN)
- `submitted_at` (TIMESTAMP)
- `time_taken_seconds` (INTEGER)
- `selected_options` (TEXT)

## 📡 API Documentation

### Interactive API Docs
Once the backend is running, visit:
- **Swagger UI**: http://localhost:8000/docs
- **ReDoc**: http://localhost:8000/redoc

### Example API Calls

#### Register Teacher
```bash
curl -X POST "http://localhost:8000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "password": "secure_password"
  }'
```

#### Login
```bash
curl -X POST "http://localhost:8000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "secure_password"
  }'
```

#### Create Quiz
```bash
curl -X POST "http://localhost:8000/api/quiz/create?teacher_id=1&title=Sample%20Quiz&question_text=What%20is%202+2?&num_choices=4&allow_multiple=false&has_correct=true" \
  -H "Content-Type: application/json" \
  -d '{
    "answers": [
      {"text": "3", "order": 0, "is_correct": false},
      {"text": "4", "order": 1, "is_correct": true},
      {"text": "5", "order": 2, "is_correct": false},
      {"text": "6", "order": 3, "is_correct": false}
    ]
  }'
```

#### Start Session
```bash
curl -X POST "http://localhost:8000/api/session/start" \
  -H "Content-Type: application/json" \
  -d '{
    "quiz_id": 1,
    "override_auto_close_minutes": 5
  }'
```

## 🔍 Troubleshooting

### Common Issues

#### Database Connection Failed
**Problem**: Cannot connect to PostgreSQL

**Solutions**:
1. Verify PostgreSQL is running:
   ```bash
   # Windows
   pg_ctl status
   
   # Linux/Mac
   sudo systemctl status postgresql
   ```
2. Check database credentials in `.env` file
3. Ensure database `quiz_app` exists
4. Verify firewall settings

#### Backend Won't Start
**Problem**: `python main.py` fails

**Solutions**:
1. Install missing dependencies:
   ```bash
   pip install -r requirements.txt
   ```
2. Check for port conflicts (port 8000)
3. Review error messages in console
4. Verify Python version (3.9+)

#### Streamlit Apps Won't Load
**Problem**: Streamlit portals show errors

**Solutions**:
1. Ensure backend is running first
2. Check sys.path in student.py and teacher.py
3. Verify Streamlit installation:
   ```bash
   streamlit --version
   ```
4. Clear Streamlit cache:
   ```bash
   streamlit cache clear
   ```

#### PowerPoint Add-in Not Appearing
**Problem**: ClassPointQuiz tab not in PowerPoint

**Solutions**:
1. Rebuild solution in Visual Studio
2. Check if add-in is disabled:
   - File → Options → Add-ins
   - Manage: COM Add-ins → Go
   - Enable ClassPointQuiz
3. Restart PowerPoint
4. Run Visual Studio as Administrator

#### Students Can't Join Session
**Problem**: Invalid class code error

**Solutions**:
1. Verify session is active
2. Check class code spelling (case-sensitive)
3. Ensure backend is running
4. Check session hasn't auto-closed
5. Verify database connection

#### Live Results Not Updating
**Problem**: Dashboard shows stale data

**Solutions**:
1. Check WebSocket connection
2. Refresh the page manually
3. Verify backend WebSocket endpoint is accessible
4. Check browser console for errors
5. Ensure st_autorefresh is installed

### Logs and Debugging

#### Backend Logs
View API logs in the terminal where `main.py` is running

#### Database Logs
```bash
# Check PostgreSQL logs
tail -f /var/log/postgresql/postgresql-12-main.log
```

#### Streamlit Logs
Logs appear in the terminal where Streamlit apps are running

### Getting Help

If issues persist:
1. Check the [GitHub Issues](https://github.com/MalakAlKazem/ClassPointQuiz/issues)
2. Review API documentation at `/docs`
3. Verify all prerequisites are installed
4. Check database connectivity with `test.py`

# Contributing

To contribute to this project, follow these steps:

1. **Fork the repository**
2. **Clone your fork**
   
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes**
   
   ```bash
   git commit -m 'Add: description of your feature'
   ```

4. **Push your changes**
   
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Create a pull request**

Thank you for contributing!

## 📞 Contact

For questions or support:
- GitHub: [@MalakAlKazem](https://github.com/MalakAlKazem)
- Repository: [ClassPointQuiz](https://github.com/MalakAlKazem/ClassPointQuiz)

---

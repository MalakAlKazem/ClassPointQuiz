# ClassPointQuiz

A quiz management system integrated with PowerPoint, allowing teachers to create interactive quizzes, manage live sessions, and track student responses.

## 📋 Table of Contents
- [Overview](#overview)
- [Features](#features)
- [System Architecture](#system-architecture)
- [How the PowerPoint Add-in Works](#how-the-powerpoint-add-in-works)
- [Live Updates Explanation](#live-updates-explanation)
- [Streamlit Dashboard Links](#streamlit-dashboard-links)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [Teacher Guide](#teacher-guide)
- [Student Guide](#student-guide)
- [Database Schema](#database-schema)
- [API Documentation](#api-documentation)
- [Troubleshooting](#troubleshooting)
- [Known Limitations](#known-limitations)
- [Future Improvements](#future-improvements)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

ClassPointQuiz is an application that integrates quiz functionality with PowerPoint presentations. The system consists of:

- **PowerPoint Add-in** (C# / VSTO): Allows teachers to manage quizzes directly from PowerPoint
- **FastAPI Backend** (Python): Handles authentication, quiz management, and session control
- **Streamlit Web Apps** (Python): Separate portals for teachers and students
- **PostgreSQL Database**: Stores all quiz data, sessions, and student responses (hosted on Neon for production)

## ✨ Features

### For Teachers
- 📝 Create and manage multiple-choice quizzes (single question per quiz)
- 🎨 Insert quiz content directly into PowerPoint slides
- 🚀 Start live quiz sessions with unique 6-character class codes
- 📊 View results as students submit (polling-based updates)
- 🔒 Secure authentication with session persistence
- 📈 Track quiz history and basic analytics

### For Students
- 🎓 Join quiz sessions using class codes
- ⏱️ Answer questions with timed submission
- 📱 Clean, responsive web interface via Streamlit

### System Capabilities
- 🔐 Teacher authentication and authorization
- 🌐 RESTful API for all operations
- 💾 Persistent data storage with PostgreSQL (Neon)
- 🔄 Polling-based updates for live results
- 🎯 Quiz difficulty modes (easy, medium, hard)
- 📊 Result visualization with charts

## 🏗️ System Architecture

```
ClassPointQuiz/
├── quizApp-addin/
│   ├── backend/                 # FastAPI backend server
│   │   ├── main.py             # Main API endpoints
│   │   ├── database. py         # Database helper functions
│   │   ├── models.py           # Pydantic models
│   │   ├── requirements.txt    # Python dependencies
│   │   └── .env                # Environment configuration
│   ├── powerpoint-addin/       # C# PowerPoint Add-in (VSTO)
│   │   └── ClassPointQuiz.sln  # Visual Studio solution
│   ├── streamlit-side/         # Web applications
│   │   ├── teacher/            # Teacher portal
│   │   │   └── teacher.py
│   │   └── student/            # Student portal
│   │       └── student.py
│   └── quiz_app_db             # PostgreSQL database dump
```

### Data Flow

```
┌─────────────────┐     Polling (~2s)     ┌─────────────────┐
│  PowerPoint     │◄────────────────────►│  FastAPI        │
│  Add-in (C#)    │     REST API          │  Backend        │
└─────────────────┘                       └────────┬────────┘
                                                   │
┌─────────────────┐                                │
│  Teacher        │     REST API + Direct DB      │
│  Streamlit      │◄───────────────────────────────┤
└─────────────────┘     Polling (~3s)              │
                                                   │
┌─────────────────┐     Direct DB Access           │
│  Student        │◄───────────────────────────────┤
│  Streamlit      │     (database. py helpers)      │
└─────────────────┘     Polling (~5s)              │
                                                   │
                                          ┌────────▼────────┐
                                          │  PostgreSQL     │
                                          │  (Neon Cloud)   │
                                          └─────────────────┘
```

## 🔌 How the PowerPoint Add-in Works

### Add-in Loading
The ClassPointQuiz add-in loads automatically when Microsoft PowerPoint opens. It appears as a panel in the PowerPoint interface. 

### Login Flow
1. **Initial State**: When the add-in loads, it checks for saved login credentials by reading a file (`teacher_login.txt`) stored in the user's home directory. 
2. **Not Logged In**: If no valid credentials are found, the add-in displays a "Login" button. 
3. **Login Process**:
   - Clicking "Login" opens the Streamlit teacher portal in your default web browser
   - Log in through the web portal with your email and password
   - Upon successful login, the portal saves credentials to `~/teacher_login.txt`
4. **Automatic Detection**: The add-in polls every 2 seconds to check for the login file.  Once detected, the UI automatically updates to show the logged-in state with your name and access to quiz features. 
5. **Session Persistence**:  Credentials remain saved until you explicitly log out, so you won't need to log in again when reopening PowerPoint.

### Quiz Management from PowerPoint
Once logged in, you can:
- View and select your quizzes
- Configure quiz settings (auto-close timer, etc.)
- Start a quiz session (generates a class code)
- View live results in a popup dialog
- Insert quiz content or results into slides

### Error Handling
- If the Streamlit portal does not open automatically, check the `app.config` file in the add-in directory for the correct `StreamlitUrl` and `StreamlitAppPath` settings
- You can manually run `streamlit run teacher. py` from the terminal and navigate to the URL
- Use the "Check Login Status (Debug)" button to verify the login file path

## 🔄 Live Updates Explanation

### Polling-Based Architecture
This system uses **polling** (periodic requests) rather than WebSockets for live updates. This design was chosen for simplicity and reliability across different deployment environments.

### Refresh Intervals
| Component | Interval | Purpose |
|-----------|----------|---------|
| PowerPoint Add-in (login check) | 2 seconds | Detects when teacher logs in via browser |
| PowerPoint Live Results Dialog | 2 seconds | Fetches latest student responses |
| Teacher Streamlit Dashboard | 3 seconds | Updates participant count and results |
| Student Streamlit App | 5 seconds | Checks session status (active/closed) |

### How It Works
- **Teacher Dashboard**: Uses `streamlit_autorefresh` to automatically reload the page every 3 seconds during a live session, fetching the latest results from the database. 
- **Student App**: Uses `streamlit_autorefresh` every 5 seconds to check if the session is still active and update the UI accordingly. 
- **PowerPoint Add-in**:  Uses C# `Timer` objects to periodically call the FastAPI endpoints and refresh the results display.

### Why Polling? 
- Simpler deployment (no WebSocket infrastructure needed)
- Works reliably with Streamlit Cloud hosting
- Acceptable latency for classroom quiz scenarios (3-5 seconds)

## 🔗 Streamlit Dashboard Links

### Production (Streamlit Cloud)
- **Teacher Dashboard**: https://quizapp-teacher. streamlit.app
- **Student Dashboard**:  https://quizapp-joinclass.streamlit.app

### Local Development
- **Teacher Dashboard**: http://localhost:8501
- **Student Dashboard**: http://localhost:8502

Both dashboards connect to the PostgreSQL database hosted on Neon.  They support full read/write operations for their respective user roles. 

## 📦 Prerequisites

### Required Software
- **Python 3.9+** - [Download Python](https://www.python.org/downloads/)
- **PostgreSQL 12+** (for local development) - [Download PostgreSQL](https://www.postgresql.org/download/)
- **Microsoft PowerPoint** (Office 2016 or later) - Windows only
- **Visual Studio 2019+** (for building the PowerPoint add-in)
- **.NET Framework 4.7.2+**

### Python Packages
All Python dependencies are listed in `quizApp-addin/backend/requirements. txt`

## 🚀 Installation & Setup

### Step 1: Clone the Repository
```bash
git clone https://github.com/MalakAlKazem/ClassPointQuiz. git
cd ClassPointQuiz
```

### Step 2: Database Setup

#### Option A: Use Neon Cloud Database (Recommended for Production)
The production database is hosted on [Neon](https://neon.tech/). Update the `.env` file with your Neon connection credentials. 

#### Option B:  Local Development with PostgreSQL
1. **Install PostgreSQL** if not already installed
2. **Create a new database**:
```sql
CREATE DATABASE quiz_app;
```
3. **Restore the database dump**:
```bash
psql -U postgres -d quiz_app -f quizApp-addin/quiz_app_db
```
4. Optionally use **pgAdmin** to browse and manage your local database during development. 

### Step 3: Configure Environment Variables

1. Navigate to the backend directory: 
```bash
cd quizApp-addin/backend
```

2. Update the `.env` file with your database credentials: 
```env
# For Neon (production)
DB_HOST=your-neon-host.neon.tech
DB_PORT=5432
DB_NAME=quiz_app
DB_USER=your_username
DB_PASSWORD=your_password

# For local development
# DB_HOST=localhost
# DB_PORT=5432
# DB_NAME=quiz_app
# DB_USER=postgres
# DB_PASSWORD=your_local_password
```

### Step 4: Install Python Dependencies

```bash
cd quizApp-addin/backend
pip install -r requirements. txt

# Additional Streamlit dependencies
pip install streamlit streamlit-autorefresh plotly pandas
```

### Step 5: Test Database Connection

```bash
cd quizApp-addin/backend
python test. py
```

You should see: 
```
✅ Database connection successful! 
📊 Teachers in database: X
```

### Step 6: Build PowerPoint Add-in

1. Open `quizApp-addin/powerpoint-addin/ClassPointQuiz.sln` in Visual Studio
2. Build the solution (F6 or Build → Build Solution)
3. The add-in will be automatically registered during build
4. Restart PowerPoint to see the add-in panel

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
streamlit run student. py
```

The student portal will open at: `http://localhost:8502`

### Quick Start Script (Windows)

```batch
@echo off
start cmd /k "cd quizApp-addin\backend && python main.py"
timeout /t 3
start cmd /k "cd quizApp-addin\streamlit-side\teacher && streamlit run teacher.py"
start cmd /k "cd quizApp-addin\streamlit-side\student && streamlit run student.py"
```

## 👨‍🏫 Teacher Guide

### 1. Registration and Login

#### Via Streamlit Portal
1. Navigate to the Teacher Portal (`http://localhost:8501` or the Streamlit Cloud URL)
2. Click on "Register New Account"
3. Fill in:  Username, Email address, Password
4. Click "Create Account"
5. You'll be logged in and credentials are saved for PowerPoint access

#### Via PowerPoint Add-in
1. Open PowerPoint — the ClassPointQuiz panel appears automatically
2. If not logged in, click the "Login" button
3. Your browser opens to the Streamlit teacher portal
4. Log in with your credentials
5. Return to PowerPoint — the panel will automatically update within a few seconds

### 2. Create a Quiz

1. From the dashboard, click **"Create New Quiz"**
2. Fill in the quiz details:
   - **Quiz Title**:  Descriptive name for your quiz
   - **Question Text**: The question students will answer
   - **Number of Choices**: Select 2-8 answer options
3. Enter the answer choices and mark the correct one(s)
4. Configure quiz settings: 
   - **Quiz Mode**: Easy, Medium, or Hard
   - **Allow Multiple Answers**: Enable if more than one answer is correct
   - **Auto-close Timer**: Set how long the quiz stays open
5. Click **"Create Quiz"**

### 3. Start a Live Session

1. Select a quiz from your library
2. Click **"Run Quiz Session"** or **"Start Session"**
3. The system generates a unique 6-character class code
4. Share the class code with your students
5. Monitor responses as students join and answer

### 4. View Live Results

During an active session, you'll see:
- **Participant Count**: Number of students who have joined
- **Response Distribution**: Bar chart showing answer choices
- **Percentage Breakdown**: What % chose each option
- **Correct Answer Highlighting**: Green highlight on correct answers

Results update automatically every 2-3 seconds via polling.

### 5. Close a Session

1. Click **"Close Session"** when the quiz is complete
2. The session stops accepting new responses
3. Final results are saved to the database

## 🎓 Student Guide

### 1. Joining a Quiz Session

1. Navigate to the Student Portal (local or Streamlit Cloud URL)
2. Enter: 
   - **Your Name**: How you want to be identified
   - **Class Code**: The 6-character code from your teacher
3. Click **"Join Session"**

### 2. Answering Questions

1. Read the quiz question and available choices
2. Select your answer (single or multiple depending on quiz settings)
3. Click **"Submit Answer"**
4. You'll see a confirmation message

### 3. Session Status

The app automatically checks every 5 seconds: 
- **Active**: Answer questions now
- **Closed**: Session has ended, answers no longer accepted

## 🗄️ Database Schema

### Deployment
- **Production**: PostgreSQL hosted on [Neon](https://neon.tech/)
- **Local Development**: Local PostgreSQL with pgAdmin for management

### Tables

| Table | Description |
|-------|-------------|
| `teachers` | Teacher accounts (id, username, email, hashed password) |
| `quizzes` | Quiz definitions (title, settings, mode) |
| `questions` | Quiz questions (one per quiz currently) |
| `answers` | Answer choices for questions |
| `quiz_sessions` | Active/closed quiz sessions with class codes |
| `students` | Students who joined sessions |
| `student_answers` | Submitted answers with correctness |

## 📡 API Documentation

### Interactive API Docs
Once the backend is running, visit: 
- **Swagger UI**: http://localhost:8000/docs
- **ReDoc**:  http://localhost:8000/redoc

### Key Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Teacher registration |
| POST | `/api/auth/login` | Teacher login |
| POST | `/api/quiz/create` | Create new quiz |
| GET | `/api/quiz/teacher/{teacher_id}` | Get teacher's quizzes |
| POST | `/api/session/start` | Start quiz session |
| POST | `/api/session/close/{session_id}` | Close session |
| GET | `/api/session/{session_id}/results` | Get live results |
| POST | `/api/student/join` | Student joins session |
| POST | `/api/student/answer` | Submit answer |

## 🔍 Troubleshooting

### Database Connection Failed
**Problem**: Cannot connect to PostgreSQL

**Solutions**:
1. For Neon:  Verify credentials in `.env` file match your Neon dashboard
2. For local:  Verify PostgreSQL is running: 
   ```bash
   # Windows
   pg_ctl status
   
   # Linux/Mac
   sudo systemctl status postgresql
   ```
3. Check that the database `quiz_app` exists
4. Verify firewall/SSL settings (Neon requires SSL)

### Backend Won't Start
**Solutions**:
1. Install missing dependencies: `pip install -r requirements.txt`
2. Check for port conflicts (port 8000)
3. Verify Python version (3.9+)

### Streamlit Apps Won't Load
**Solutions**: 
1. Ensure backend is running first
2. Check `sys.path` in student. py and teacher.py points to correct backend location
3. Clear Streamlit cache: `streamlit cache clear`

### PowerPoint Add-in Not Appearing
**Solutions**:
1. Rebuild solution in Visual Studio (Run as Administrator)
2. Check if add-in is disabled: 
   - File → Options → Add-ins
   - Manage:  COM Add-ins → Go
   - Enable ClassPointQuiz
3. Restart PowerPoint

### Login Portal Does Not Open from PowerPoint
**Solutions**:
1. Check `app.config` file for correct `StreamlitUrl` and `StreamlitAppPath` settings
2. Manually start Streamlit:  `streamlit run teacher.py`
3. Navigate to `http://localhost:8501` in your browser
4. After logging in, return to PowerPoint and wait for auto-detection

### Students Can't Join Session
**Solutions**: 
1. Verify session is still active (not auto-closed)
2. Check class code spelling (case-sensitive)
3. Ensure backend is running and accessible
4. Verify database connection

### Live Results Not Updating
**Solutions**: 
1. Check that backend is running and responding
2. Refresh the page manually
3. Verify `streamlit-autorefresh` is installed
4. Check browser console for errors

## ⚠️ Known Limitations

| Limitation | Details |
|------------|---------|
| **Windows Only** | The PowerPoint add-in uses VSTO, which only works on Windows |
| **Polling Latency** | Results update every 2-5 seconds (not instant) |
| **Single Question Per Quiz** | Each quiz currently supports one question only |
| **No Student Authentication** | Students join with just a name (no accounts) |
| **Local Path Dependencies** | Streamlit apps have hardcoded `sys.path` that may need adjustment |

## 🚀 Future Improvements

- **WebSocket Integration**: Replace polling with WebSockets for instant updates
- **Multiple Questions Per Session**: Support multi-question quizzes
- **Additional Question Types**: True/False, short answer, matching
- **Cross-Platform Teacher Client**: Web-based alternative to PowerPoint add-in
- **Student Authentication**: Optional student accounts and progress tracking
- **Enhanced Analytics**: Detailed performance reports and export options
- **Mobile-Friendly Design**: Improved responsive design for student app

## 🤝 Contributing

To contribute to this project: 

1. **Fork the repository**
2. **Clone your fork**
3. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```
4. **Make your changes and commit**
   ```bash
   git commit -m 'Add:  description of your feature'
   ```
5. **Push your changes**
   ```bash
   git push origin feature/your-feature-name
   ```
6. **Create a pull request**

## 📞 Contact

For questions or support:
- GitHub:  [@MalakAlKazem](https://github.com/MalakAlKazem)
- Repository: [ClassPointQuiz](https://github.com/MalakAlKazem/ClassPointQuiz)

---

**Note**: This README reflects the actual implementation as of the latest codebase review. Features described here have been verified against the source code. 

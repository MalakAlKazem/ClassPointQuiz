"""
FastAPI Backend for ClassPoint Quiz Application
Matches the C# ApiClient.cs interface
"""

from fastapi import FastAPI, HTTPException, Query, Body
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Optional
import random
import string
import uvicorn


# Import database functions
from database import *

app = FastAPI(title="ClassPoint Quiz API")

# CORS - Allow C# add-in to connect
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ============================================
# REQUEST MODELS clearcl(Match C# classes)
# ============================================

class LoginRequest(BaseModel):
    email: str
    password: str

class RegisterRequest(BaseModel):
    username: str
    email: str
    password: str

class Answer(BaseModel):
    text: str
    order: int
    is_correct: bool

class QuizCreateRequestBody(BaseModel):
    teacher_id: int
    title: str
    question_text: str
    num_choices: int
    allow_multiple: bool = False
    has_correct: bool = True
    quiz_mode: str = "easy"
    start_with_slide: bool = True
    minimize_window: bool = False
    auto_close_minutes: int = 1
    answers: List[Answer] = []

class SessionStartRequest(BaseModel):
    quiz_id: int
    override_auto_close_minutes: Optional[int] = None 

# ============================================
# RESPONSE MODELS (Match C# classes)
# ============================================

class QuizResponse(BaseModel):
    quiz_id: int
    question_id: int
    message: str

class SessionResponse(BaseModel):
    session_id: int
    class_code: str
    status: str

class ResultItem(BaseModel):
    answer_text: str
    answer_order: int
    is_correct: bool
    count: int
    percentage: float

class ResultsResponse(BaseModel):
    results: List[ResultItem]
    participant_count: int
    total_responses: int

class StudentResponse(BaseModel):
    student_id: int
    student_name: str
    answer_text: str
    is_correct: bool
    submitted_at: str

class StudentDetailsResponse(BaseModel):
    students: List[StudentResponse]
    total_students: int
    total_responses: int
# ============================================
# AUTHENTICATION ENDPOINTS
# ============================================

@app.post("/api/auth/register")
async def register_endpoint(request: RegisterRequest):
    """Register new teacher"""
    try:
        teacher_id, error = create_teacher(
            username=request.username,
            email=request.email,
            password=request.password
        )
        
        if not teacher_id:
            raise HTTPException(status_code=400, detail=error or "Registration failed")
        
        return {
            "teacher_id": teacher_id,
            "username": request.username,
            "email": request.email,
            "message": "Registration successful"
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/auth/login")
async def login_endpoint(request: LoginRequest):
    """Teacher login"""
    try:
        teacher = authenticate_teacher(request.email, request.password)
        
        if not teacher:
            raise HTTPException(status_code=401, detail="Invalid email or password")
        
        return {
            "teacher_id": teacher['teacher_id'],
            "username": teacher['username'],
            "email": teacher['email'],
            "message": "Login successful"
        }
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ============================================
# QUIZ ENDPOINTS
# ============================================

@app.post("/api/quiz/create", response_model=QuizResponse)
async def create_quiz_endpoint(
    teacher_id: int = Query(None),
    title: str = Query(None),
    question_text: str = Query(None),
    num_choices: int = Query(None),
    allow_multiple: bool = Query(False),
    has_correct: bool = Query(True),
    quiz_mode: str = Query("easy"),
    start_with_slide: bool = Query(True),
    minimize_window: bool = Query(False),
    auto_close_minutes: int = Query(1),
    body: dict = Body(None)
):
    """
    Create a quiz with question and answers
    Accepts EITHER query params OR body, or BOTH
    """
    try:
        # Get answers from body
        answers_list = []
        if body and 'answers' in body:
            print(f"📋 Got {len(body['answers'])} answers from body")
            answers_list = body['answers']
        
        print(f"\n📝 Creating quiz:")
        print(f"  Teacher ID: {teacher_id}")
        print(f"  Title: {title}")
        print(f"  Question: {question_text}")
        print(f"  Num choices: {num_choices}")
        print(f"  Answers: {len(answers_list)}")
        
        # Create quiz
        quiz_id, error = create_quiz(
            teacher_id=teacher_id,
            title=title or "Untitled Quiz",
            num_choices=num_choices or 4,
            allow_multiple=allow_multiple,
            has_correct=has_correct,
            competition_mode=(quiz_mode != "easy"),
            start_with_slide=start_with_slide,
            minimize_window=minimize_window,
            close_after=auto_close_minutes,
            quiz_mode=quiz_mode  # ✅ ADD THIS: Pass quiz_mode to database

        )
        
        if not quiz_id:
            print(f"❌ Failed to create quiz: {error}")
            raise HTTPException(status_code=400, detail=error or "Failed to create quiz")
        
        print(f"✅ Quiz created with ID: {quiz_id}")
        
        # Add question
        question_id, error = add_question(quiz_id, question_text or "Untitled Question")
        if not question_id:
            print(f"❌ Failed to add question: {error}")
            raise HTTPException(status_code=400, detail=error or "Failed to add question")
        
        print(f"✅ Question created with ID: {question_id}")
        
        # Add answers
        if answers_list:
            print(f"📋 Adding {len(answers_list)} answers...")
            formatted_answers = [
                {
                    'text': ans.get('text', f"Answer {i+1}"),
                    'order': ans.get('order', i),
                    'is_correct': ans.get('is_correct', False)
                }
                for i, ans in enumerate(answers_list)
            ]
            
            success, error = add_answers(question_id, formatted_answers)
            if not success:
                print(f"❌ Failed to add answers: {error}")
                raise HTTPException(status_code=400, detail=error or "Failed to add answers")
            
            print(f"✅ Answers added successfully!")
        else:
            print("⚠️ No answers provided")
        
        print(f"✅ Quiz creation complete!\n")
        return QuizResponse(
            quiz_id=quiz_id,
            question_id=question_id,
            message="Quiz created successfully"
        )
    
    except HTTPException:
        raise
    except Exception as e:
        print(f"❌ Exception: {str(e)}")
        import traceback
        traceback.print_exc()
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/api/quiz/{quiz_id}")
async def get_quiz_endpoint(quiz_id: int):
    """Get quiz details"""
    quiz = get_quiz_details(quiz_id)
    if not quiz:
        raise HTTPException(status_code=404, detail="Quiz not found")
    return quiz


@app.get("/api/teacher/{teacher_id}/quizzes")
async def get_teacher_quizzes_endpoint(teacher_id: int):
    """Get all quizzes for a teacher"""
    try:
        quizzes = get_teacher_quizzes(teacher_id)
        return quizzes
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ============================================
# SESSION ENDPOINTS
# ============================================
@app.post("/api/session/start", response_model=SessionResponse)
async def start_session_endpoint(request: SessionStartRequest):
    try:
        class_code = ''.join(random.choices(string.ascii_uppercase + string.digits, k=6))
        session_id, error = create_quiz_session(
            quiz_id=request.quiz_id,
            class_code=class_code,
            auto_close_minutes=request.override_auto_close_minutes
        )
        if not session_id:
            raise HTTPException(status_code=400, detail=error or "Failed to create session")
        return SessionResponse(session_id=session_id, class_code=class_code, status="active")
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/session/{session_id}/results", response_model=ResultsResponse)
async def get_results_endpoint(session_id: int):
    """Get live results for a session"""
    try:
        results_data = get_session_results(session_id)
        
        if not results_data:
            raise HTTPException(status_code=404, detail="Session not found")
        
        # Calculate percentages
        total_responses = sum(r['count'] for r in results_data['results'])
        
        formatted_results = []
        for r in results_data['results']:
            percentage = (r['count'] / total_responses * 100) if total_responses > 0 else 0
            formatted_results.append(ResultItem(
                answer_text=r['answer_text'],
                answer_order=r['answer_order'],
                is_correct=r['is_correct'],
                count=r['count'],
                percentage=round(percentage, 1)
            ))
        
        return ResultsResponse(
            results=formatted_results,
            participant_count=results_data['participant_count'],
            total_responses=total_responses
        )
    
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/session/{session_id}/close")
async def close_session_endpoint(session_id: int):
    """Close a session"""
    try:
        success = close_session(session_id)
        if not success:
            raise HTTPException(status_code=400, detail="Failed to close session")
        
        return {"success": True, "message": "Session closed successfully"}
    
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/session/{session_id}/info")
async def get_session_info_endpoint(session_id: int):
    """Get session info including start time"""
    try:
        session = get_session_info(session_id)
        
        if not session:
            raise HTTPException(status_code=404, detail="Session not found")
        
        # ✅ Ensure timezone-aware datetime
        import datetime
        started_at = session['started_at']
        if started_at and isinstance(started_at, datetime.datetime):
            # If naive, assume UTC
            if started_at.tzinfo is None:
                started_at = started_at.replace(tzinfo=datetime.timezone.utc)
        
        return {
            "session_id": session['session_id'],
            "started_at": started_at.isoformat() if started_at else None,
            "status": session['status'],
            "auto_close_minutes": session['auto_close_minutes']
        }
    except HTTPException:
        raise
    except Exception as e:
        print(f"Error in get_session_info_endpoint: {e}")
        raise HTTPException(status_code=500, detail=str(e))
# ============================================
# STUDENT ENDPOINTS (for web interface)
# ============================================

@app.get("/api/session/code/{class_code}")
async def get_session_by_code_endpoint(class_code: str):
    """Get session by class code (for students joining)"""
    session = get_session_by_code(class_code)
    if not session:
        raise HTTPException(status_code=404, detail="Session not found or expired")
    
    # Get quiz details
    quiz = get_quiz_details(session['quiz_id'])
    
    return {
        "session_id": session['session_id'],
        "quiz_id": session['quiz_id'],
        "quiz_title": session['title'],
        "status": session['status'],
        "questions": quiz.get('questions', []) if quiz else []
    }


@app.post("/api/student/join")
async def student_join(session_id: int, student_name: str):
    """Student joins a session"""
    try:
        student_id, error = add_student_to_session(session_id, student_name)
        if not student_id:
            raise HTTPException(status_code=400, detail=error or "Failed to join session")
        
        return {"student_id": student_id, "message": "Joined successfully"}
    
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/api/student/answer")
async def submit_student_answer(
    student_id: int,
    session_id: int,
    question_id: int,
    answer_id: int,
    time_taken: int = 0
):
    """Submit student answer"""
    try:
        success, error = submit_answer(
            student_id=student_id,
            session_id=session_id,
            question_id=question_id,
            answer_id=answer_id,
            time_taken=time_taken
        )
        
        if not success:
            raise HTTPException(status_code=400, detail=error or "Failed to submit answer")
        
        return {"success": True, "message": "Answer submitted"}
    
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    
@app.get("/api/session/{session_id}/student-responses")
async def get_student_responses_endpoint(session_id: int):
    student_responses = get_student_responses(session_id)
    if student_responses is None:
        raise HTTPException(status_code=404, detail="Session not found")
    return StudentDetailsResponse(
        students=student_responses['students'],
        total_students=student_responses['total_students'],
        total_responses=student_responses['total_responses']
    )


# ============================================
# HEALTH CHECK
# ============================================

@app.get("/")
async def root():
    """Root endpoint"""
    return {
        "message": "ClassPoint Quiz API",
        "status": "running",
        "version": "1.0"
    }


@app.get("/health")
async def health_check():
    """Health check endpoint - used by C# client"""
    try:
        # Test database connection
        conn = get_db_connection()
        if conn:
            conn.close()
            return {"status": "healthy", "database": "connected"}
        else:
            return {"status": "degraded", "database": "disconnected"}
    except Exception as e:
        return {"status": "unhealthy", "error": str(e)}


# ============================================
# STARTUP MESSAGE
# ============================================

def print_startup_message():
    print("\n" + "="*60)
    print("🚀 ClassPoint Quiz Backend Starting...")
    print("="*60)
    print("\n📡 Server: http://localhost:8000")
    print("📊 API Docs: http://localhost:8000/docs")
    print("🔍 Health Check: http://localhost:8000/health")
    print("\n💡 Keep this window open while using the add-in!")
    print("\n🔗 Endpoints:")
    print("   POST /api/auth/register")
    print("   POST /api/auth/login")
    print("   POST /api/quiz/create")
    print("   POST /api/session/start")
    print("   GET  /api/session/{id}/results")
    print("   POST /api/session/{id}/close")
    print("="*60 + "\n")


# ============================================
# RUN SERVER
# ============================================

if __name__ == "__main__":
    print_startup_message()
    uvicorn.run(
        app,
        host="0.0.0.0",
        port=8000,
        log_level="info"
    )
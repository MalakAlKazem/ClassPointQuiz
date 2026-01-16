# database.py
# Database connection for FastAPI backend

import psycopg2
from psycopg2.extras import RealDictCursor
import hashlib
import os
from dotenv import load_dotenv

load_dotenv()

DB_CONFIG = {
    "host": os.getenv("DB_HOST", "localhost"),
    "port": int(os.getenv("DB_PORT", 5432)),
    "database": os.getenv("DB_NAME", "quiz_app"),
    "user": os.getenv("DB_USER", "yourUserName"),
    "password": os.getenv("DB_PASSWORD", "yourPass"),
    "sslmode": "disable"
}

def get_db_connection():
    """Get database connection"""
    try:
        conn = psycopg2.connect(**DB_CONFIG, cursor_factory=RealDictCursor)
        return conn
    except Exception as e:
        print(f"Database connection error: {e}")
        return None

def hash_password(password):
    """Hash password using SHA256"""
    return hashlib.sha256(password.encode()).hexdigest()

# Teacher functions
def create_teacher(username, email, password):
    """Create new teacher account"""
    conn = get_db_connection()
    if not conn:
        return None, "Database connection failed"
    
    try:
        cur = conn.cursor()
        hashed_pw = hash_password(password)
        cur.execute("""
            INSERT INTO teachers (username, email, password)
            VALUES (%s, %s, %s)
            RETURNING teacher_id
        """, (username, email, hashed_pw))
        teacher_id = cur.fetchone()['teacher_id']
        conn.commit()
        cur.close()
        conn.close()
        return teacher_id, None
    except psycopg2.IntegrityError:
        conn.rollback()
        conn.close()
        return None, "Email or username already exists"
    except Exception as e:
        conn.rollback()
        conn.close()
        return None, str(e)

def authenticate_teacher(email, password):
    """Authenticate teacher login"""
    conn = get_db_connection()
    if not conn:
        return None
    
    try:
        cur = conn.cursor()
        hashed_pw = hash_password(password)
        cur.execute("""
            SELECT teacher_id, username, email
            FROM teachers
            WHERE email = %s AND password = %s
        """, (email, hashed_pw))
        teacher = cur.fetchone()
        cur.close()
        conn.close()
        return teacher
    except Exception as e:
        print(f"Authentication error: {e}")
        conn.close()
        return None

def get_teacher_quizzes(teacher_id):
    """Get all quizzes for a teacher"""
    conn = get_db_connection()
    if not conn:
        return []
    
    try:
        cur = conn.cursor()
        cur.execute("""
            SELECT q.quiz_id, q.title, q.num_choices, q.created_at, q.quiz_mode,
                   COUNT(DISTINCT qs.session_id) as session_count
            FROM quizzes q
            LEFT JOIN quiz_sessions qs ON q.quiz_id = qs.quiz_id
            WHERE q.teacher_id = %s
            GROUP BY q.quiz_id
            ORDER BY q.created_at DESC
        """, (teacher_id,))
        quizzes = cur.fetchall()
        cur.close()
        conn.close()
        return quizzes
    except Exception as e:
        print(f"Error fetching quizzes: {e}")
        conn.close()
        return []

def create_quiz(teacher_id, title, num_choices, allow_multiple, has_correct, 
                competition_mode, start_with_slide, minimize_window, close_after, 
                quiz_mode='easy'):
    """Create a new quiz"""
    conn = get_db_connection()
    if not conn:
        return None, "Database connection failed"
    
    try:
        cur = conn.cursor()
        cur.execute("""
            INSERT INTO quizzes (
                teacher_id, title, num_choices, allow_multiple, has_correct,
                competition_mode, start_with_slide, minimize_result_window,
                close_submission_after, quiz_mode
            ) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s)
            RETURNING quiz_id
        """, (teacher_id, title, num_choices, allow_multiple, has_correct,
              competition_mode, start_with_slide, minimize_window, close_after, 
              quiz_mode))
        quiz_id = cur.fetchone()['quiz_id']
        conn.commit()
        cur.close()
        conn.close()
        return quiz_id, None
    except Exception as e:
        conn.rollback()
        conn.close()
        return None, str(e)
    
def add_question(quiz_id, question_text):
    """Add question to quiz"""
    conn = get_db_connection()
    if not conn:
        return None, "Database connection failed"
    
    try:
        cur = conn.cursor()
        cur.execute("""
            INSERT INTO questions (quiz_id, question_text)
            VALUES (%s, %s)
            RETURNING question_id
        """, (quiz_id, question_text))
        question_id = cur.fetchone()['question_id']
        conn.commit()
        cur.close()
        conn.close()
        return question_id, None
    except Exception as e:
        conn.rollback()
        conn.close()
        return None, str(e)

def add_answers(question_id, answers_list):
    """Add multiple answers to a question
    
    Args:
        question_id: ID of the question
        answers_list: List of dicts with 'text', 'order', 'is_correct'
    """
    conn = get_db_connection()
    if not conn:
        return False, "Database connection failed"
    
    try:
        cur = conn.cursor()
        for ans in answers_list:
            cur.execute("""
                INSERT INTO answers (question_id, answer_text, answer_order, is_correct)
                VALUES (%s, %s, %s, %s)
            """, (question_id, ans['text'], ans['order'], ans['is_correct']))
        conn.commit()
        cur.close()
        conn.close()
        return True, None
    except Exception as e:
        conn.rollback()
        conn.close()
        return False, str(e)

def get_quiz_details(quiz_id):
    """Get full quiz details including question and answers"""
    conn = get_db_connection()
    if not conn:
        return None
    
    try:
        cur = conn.cursor()
        
        # Get quiz
        cur.execute("SELECT * FROM quizzes WHERE quiz_id = %s", (quiz_id,))
        quiz = cur.fetchone()
        
        if not quiz:
            conn.close()
            return None
        
        # Get question
        cur.execute("SELECT * FROM questions WHERE quiz_id = %s", (quiz_id,))
        question = cur.fetchone()
        
        # Get answers
        if question:
            cur.execute("""
                SELECT * FROM answers 
                WHERE question_id = %s 
                ORDER BY answer_order
            """, (question['question_id'],))
            answers = cur.fetchall()
        else:
            answers = []
        
        # Auto-detect multiple correct answers
        correct_count = sum(1 for ans in answers if ans.get('is_correct', False))
        
        # Convert quiz dict to mutable dict
        quiz_dict = dict(quiz)
        if correct_count >= 2:
            quiz_dict['allow_multiple'] = True
        
        quiz_dict['correct_count'] = correct_count
        quiz_dict['quiz_difficulty'] = quiz_dict.get('quiz_mode', 'easy')

        cur.close()
        conn.close()
        
        return {
            'quiz': quiz_dict,
            'question': question,
            'answers': answers
        }
    except Exception as e:
        print(f"Error fetching quiz details: {e}")
        conn.close()
        return None

def create_quiz_session(quiz_id, class_code, auto_close_minutes=None):
    conn = get_db_connection()
    if not conn:
        return None, "Database connection failed"
    try:
        cur = conn.cursor()
        cur.execute("""
            INSERT INTO quiz_sessions (quiz_id, class_code, status, auto_close_minutes, started_at)
            VALUES (%s, %s, 'active', %s, NOW() AT TIME ZONE 'UTC')
            RETURNING session_id
        """, (quiz_id, class_code, auto_close_minutes))
        session_id = cur.fetchone()['session_id']
        conn.commit()
        cur.close()
        conn.close()
        return session_id, None
    except psycopg2.IntegrityError:
        conn.rollback()
        conn.close()
        return None, "Class code already exists"
    except Exception as e:
        conn.rollback()
        conn.close()
        return None, str(e)

def get_session_by_code(class_code):
    """Get session details by class code"""
    conn = get_db_connection()
    if not conn:
        return None
    
    try:
        cur = conn.cursor()
        cur.execute("""
            SELECT qs.*, q.quiz_id, q.title
            FROM quiz_sessions qs
            JOIN quizzes q ON qs.quiz_id = q.quiz_id
            WHERE qs.class_code = %s
        """, (class_code,))
        session = cur.fetchone()
        cur.close()
        conn.close()
        return session
    except Exception as e:
        print(f"Error fetching session: {e}")
        conn.close()
        return None
    
def get_session_info(session_id):
    """Get session info including start time"""
    conn = get_db_connection()
    if not conn:
        return None
    
    try:
        cur = conn.cursor()
        cur.execute("""
            SELECT session_id, quiz_id, class_code, status, 
                   started_at, auto_close_minutes
            FROM quiz_sessions
            WHERE session_id = %s
        """, (session_id,))
        session = cur.fetchone()
        cur.close()
        conn.close()
        return session
    except Exception as e:
        print(f"Error fetching session info: {e}")
        if conn:
            conn.close()
        return None
    
def add_student_to_session(session_id, student_name):
    """Add student to session"""
    conn = get_db_connection()
    if not conn:
        return None, "Database connection failed"
    
    try:
        cur = conn.cursor()
        cur.execute("""
            INSERT INTO students (session_id, name)
            VALUES (%s, %s)
            RETURNING student_id
        """, (session_id, student_name))
        student_id = cur.fetchone()['student_id']
        conn.commit()
        cur.close()
        conn.close()
        return student_id, None
    except Exception as e:
        conn.rollback()
        conn.close()
        return None, str(e)

# ✅ FIXED: Submit answer function
def submit_answer(student_id, session_id, question_id, answer_id, time_taken):
    """Submit student answer - supports multiple correct answers"""
    conn = get_db_connection()
    if not conn:
        return False, "Database connection failed"
    
    try:
        cur = conn.cursor()
        
        # Get all correct answer IDs for this question
        cur.execute("""
            SELECT answer_id FROM answers 
            WHERE question_id = %s AND is_correct = true
        """, (question_id,))
        correct_answer_ids = set(row['answer_id'] for row in cur.fetchall())
        
        # Check if multiple correct answers exist
        allow_multiple = len(correct_answer_ids) >= 2
        
        # Get all answers this student has already submitted
        cur.execute("""
            SELECT answer_id FROM student_answers
            WHERE student_id = %s AND question_id = %s
        """, (student_id, question_id))
        
        existing_answers = set(row['answer_id'] for row in cur.fetchall())
        existing_answers.add(answer_id)  # Add the new answer
        
        # Determine if answer is correct
        if allow_multiple:
            # For multiple: correct only if ALL correct answers selected and NO wrong answers
            is_correct = (existing_answers == correct_answer_ids)
        else:
            # For single: correct if this answer is in correct set
            is_correct = (answer_id in correct_answer_ids)
        
        # Insert the new answer
        cur.execute("""
            INSERT INTO student_answers 
            (student_id, session_id, question_id, answer_id, is_correct, time_taken_seconds)
            VALUES (%s, %s, %s, %s, %s, %s)
        """, (student_id, session_id, question_id, answer_id, is_correct, time_taken))
        
        # ✅ CRITICAL FIX: Update ALL answers from this student for this question
        # This ensures correctness is evaluated on the complete answer set
        if allow_multiple:
            cur.execute("""
                UPDATE student_answers 
                SET is_correct = %s
                WHERE student_id = %s AND question_id = %s
            """, (is_correct, student_id, question_id))
        
        conn.commit()
        cur.close()
        conn.close()
        return True, None
    except Exception as e:
        conn.rollback()
        conn.close()
        print(f"Error submitting answer: {e}")
        return False, str(e)
    
def get_student_responses(session_id):
    """Get student responses with names and answers"""
    conn = get_db_connection()
    if not conn:
        return None
    
    try:
        cur = conn.cursor()
        
        cur.execute("""
            SELECT 
                s.student_id,
                s.name as student_name,
                COALESCE(a.answer_text, 'Not submitted') as answer_text,
                COALESCE(a.is_correct, false) as is_correct,
                '' as submitted_at
            FROM students s
            LEFT JOIN student_answers sa ON s.student_id = sa.student_id
            LEFT JOIN answers a ON sa.answer_id = a.answer_id
            WHERE s.session_id = %s
            ORDER BY s.student_id ASC
        """, (session_id,))
        
        responses = cur.fetchall()
        
        # Get counts
        cur.execute("""
            SELECT COUNT(DISTINCT s.student_id) as total_students,
                   COUNT(sa.id) as total_responses
            FROM students s
            LEFT JOIN student_answers sa ON s.student_id = sa.student_id
            WHERE s.session_id = %s
        """, (session_id,))
        
        counts = cur.fetchone()
        
        cur.close()
        conn.close()
        
        # Format responses
        formatted_responses = []
        for r in responses:
            formatted_responses.append({
                'student_id': r['student_id'],
                'student_name': r['student_name'],
                'answer_text': r['answer_text'],
                'is_correct': r['is_correct'],
                'submitted_at': r['submitted_at']
            })
        
        return {
            'students': formatted_responses,
            'total_students': counts['total_students'],
            'total_responses': counts['total_responses']
        }
    
    except Exception as e:
        print(f"Error fetching student responses: {e}")
        if conn:
            conn.close()
        return None

# ✅ COMPLETELY FIXED: Get session results function
def get_session_results(session_id):
    """Get live results for a session - FIXED VERSION"""
    conn = get_db_connection()
    if not conn:
        return None
    
    try:
        cur = conn.cursor()
        
        # ✅ FIXED: Get answer distribution (counts unique students per answer)
        cur.execute("""
            SELECT 
                a.answer_text, 
                a.answer_order, 
                a.is_correct,
                COUNT(DISTINCT sa.student_id) as count
            FROM answers a
            JOIN questions q ON a.question_id = q.question_id
            JOIN quiz_sessions qs ON q.quiz_id = qs.quiz_id
            LEFT JOIN student_answers sa ON a.answer_id = sa.answer_id 
                AND sa.session_id = qs.session_id
            WHERE qs.session_id = %s
            GROUP BY a.answer_id, a.answer_text, a.answer_order, a.is_correct
            ORDER BY a.answer_order
        """, (session_id,))
        results = cur.fetchall()
        
        # Get total students who joined (not just responded)
        cur.execute("""
            SELECT COUNT(*) as total FROM students WHERE session_id = %s
        """, (session_id,))
        participant_count = cur.fetchone()['total']
        
        cur.close()
        conn.close()
        
        return {
            'results': results,
            'participant_count': participant_count
        }
    except Exception as e:
        print(f"Error fetching results: {e}")
        import traceback
        traceback.print_exc()
        conn.close()
        return None

def close_session(session_id):
    """Close a quiz session"""
    conn = get_db_connection()
    if not conn:
        return False
    
    try:
        cur = conn.cursor()
        cur.execute("""
            UPDATE quiz_sessions 
            SET status = 'closed', closed_at = CURRENT_TIMESTAMP
            WHERE session_id = %s
        """, (session_id,))
        conn.commit()
        cur.close()
        conn.close()
        return True
    except Exception as e:
        print(f"Error closing session: {e}")
        conn.rollback()
        conn.close()
        return False

from pydantic import BaseModel
from typing import List, Optional
from datetime import datetime

class QuizCreate(BaseModel):
    teacher_id: int
    title: str
    question_text: str
    num_choices: int
    allow_multiple: bool = False
    has_correct: bool = True
    quiz_mode: str = "easy"  # easy, medium, hard
    start_with_slide: bool = True
    minimize_window: bool = False
    auto_close_minutes: int = 1

class Answer(BaseModel):
    text: str
    order: int
    is_correct: bool = False

class SessionStart(BaseModel):
    quiz_id: int

class StudentAnswer(BaseModel):
    session_id: int
    student_name: str
    question_id: int
    answer_ids: List[int]
    time_taken: int

class LiveResult(BaseModel):
    answer_text: str
    answer_order: int
    is_correct: bool
    count: int
    percentage: float
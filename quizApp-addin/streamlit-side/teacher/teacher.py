# teacher.py
# Streamlit app for teachers - Complete Fixed Version

import streamlit as st
import requests
import hashlib
import os
import random
import string
import pandas as pd
import plotly.express as px
from datetime import datetime, timedelta
import time
from streamlit_autorefresh import st_autorefresh
import sys
sys.path.insert(0, r'C:\Users\pc\Desktop\ClassPointQuiz')

# Backend API URL
API_URL = "http://localhost:8000"

# Import database functions
from backend.database import (
    get_session_by_code,
    get_quiz_details,
    get_session_results,
    close_session,
    get_teacher_quizzes,
    create_quiz_session,
    get_db_connection
)

# Page config - MUST be first Streamlit command
st.set_page_config(
    page_title="ClassPoint Teacher Portal",
    page_icon="📚",
    layout="wide",
    initial_sidebar_state="collapsed"
)

# Custom CSS - [Keep your existing CSS - it's good]
st.markdown("""
<style>
    /* ===== HIDE STREAMLIT BRANDING ===== */
    #MainMenu {visibility: hidden;}
    footer {visibility: hidden;}
    header {visibility: hidden;}
    .stDeployButton {display: none;}
    [data-testid="stToolbar"] {display: none;}
    [data-testid="stDecoration"] {display: none;}
    [data-testid="stStatusWidget"] {display: none;}

    /* ===== MAIN BACKGROUND ===== */
    .stApp {
        background: linear-gradient(145deg, #ffffff 0%, #f0f7ff 50%, #e6f0ff 100%);
    }

    .main {
        background: transparent;
    }

    [data-testid="stAppViewContainer"] {
        background: linear-gradient(145deg, #ffffff 0%, #f0f7ff 50%, #e6f0ff 100%);
    }

    /* ===== CARD & CONTAINER STYLING ===== */
    .stContainer, [data-testid="stVerticalBlock"] > div {
        background: transparent;
    }

    .custom-card {
        background: white;
        border-radius: 20px;
        padding: 28px;
        box-shadow: 0 8px 32px rgba(37, 99, 235, 0.1);
        border: 1px solid rgba(37, 99, 235, 0.1);
        margin-bottom: 24px;
    }

    /* ===== PRIMARY BUTTONS ===== */
    .stButton > button {
        width: 100%;
        background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);
        color: white !important;
        font-size: 16px;
        font-weight: 600;
        border-radius: 12px;
        padding: 14px 28px;
        border: none;
        box-shadow: 0 4px 20px rgba(37, 99, 235, 0.35);
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        letter-spacing: 0.3px;
    }

    .stButton > button:hover {
        background: linear-gradient(135deg, #1d4ed8 0%, #1e40af 100%);
        box-shadow: 0 8px 30px rgba(37, 99, 235, 0.45);
        transform: translateY(-3px);
    }

    .stButton > button:active {
        transform: translateY(-1px);
    }

    /* Button text - force white for primary buttons */
    .stButton > button p,
    .stButton > button span,
    .stButton > button div {
        color: white !important;
    }

    /* ===== SECONDARY BUTTONS ===== */
    .stButton > button[kind="secondary"] {
        background: white;
        color: #2563eb !important;
        border: 2px solid #2563eb;
        box-shadow: 0 2px 10px rgba(37, 99, 235, 0.1);
    }

    .stButton > button[kind="secondary"]:hover {
        background: #eff6ff;
        box-shadow: 0 4px 15px rgba(37, 99, 235, 0.2);
    }

    /* Secondary button text - force blue */
    .stButton > button[kind="secondary"] p,
    .stButton > button[kind="secondary"] span,
    .stButton > button[kind="secondary"] div {
        color: #2563eb !important;
    }

    /* ===== INPUT FIELDS - BLACK TEXT ===== */
    .stTextInput > div > div > input,
    .stTextArea > div > div > textarea,
    .stSelectbox > div > div > div,
    .stNumberInput > div > div > input {
        background: #ffffff !important;
        border: 2px solid #e2e8f0 !important;
        border-radius: 12px !important;
        padding: 14px 18px !important;
        font-size: 15px !important;
        transition: all 0.25s ease !important;
        color: #000000 !important;
        font-weight: 500 !important;
    }

    .stTextInput > div > div > input:focus,
    .stTextArea > div > div > textarea:focus,
    .stNumberInput > div > div > input:focus {
        border-color: #2563eb !important;
        box-shadow: 0 0 0 4px rgba(37, 99, 235, 0.12) !important;
        outline: none !important;
    }

    .stTextInput > div > div > input::placeholder,
    .stTextArea > div > div > textarea::placeholder {
        color: #94a3b8 !important;
        font-weight: 400 !important;
    }

    /* Select box dropdown text - black */
    .stSelectbox > div > div > div > div {
        color: #000000 !important;
    }

    [data-baseweb="select"] > div {
        color: #000000 !important;
        background: white !important;
    }

    [data-baseweb="select"] span {
        color: #000000 !important;
    }

    /* Selectbox main control */
    .stSelectbox [data-baseweb="select"] {
        background: white !important;
    }

    .stSelectbox [data-baseweb="select"] > div {
        background: white !important;
        border: 2px solid #e2e8f0 !important;
        border-radius: 12px !important;
        min-height: 48px !important;
    }

    .stSelectbox [data-baseweb="select"]:hover > div {
        border-color: #2563eb !important;
    }

    /* Selectbox dropdown styling */
    .stSelectbox [data-baseweb="popover"] {
        background: white !important;
        border: 2px solid #2563eb !important;
        border-radius: 12px !important;
        box-shadow: 0 8px 24px rgba(37, 99, 235, 0.15) !important;
        margin-top: 4px !important;
    }

    /* Dropdown list container */
    .stSelectbox ul {
        background: white !important;
        padding: 4px !important;
    }

    .stSelectbox [role="option"] {
        background: white !important;
        color: #1e40af !important;
        padding: 12px 18px !important;
        transition: all 0.2s ease !important;
        border-radius: 8px !important;
        margin: 2px 0 !important;
    }

    .stSelectbox [role="option"]:hover {
        background: #eff6ff !important;
        color: #1d4ed8 !important;
    }

    .stSelectbox [aria-selected="true"] {
        background: #dbeafe !important;
        color: #1d4ed8 !important;
        font-weight: 600 !important;
    }

    /* Dropdown option text */
    .stSelectbox [role="option"] span,
    .stSelectbox [role="option"] div {
        color: inherit !important;
    }

    /* ===== ALL HEADERS - BLUE FONT ===== */
    h1 {
        color: #1d4ed8 !important;
        font-weight: 800 !important;
        letter-spacing: -0.5px;
        font-size: 2.5rem !important;
    }

    h2 {
        color: #2563eb !important;
        font-weight: 700 !important;
        letter-spacing: -0.3px;
    }

    h3 {
        color: #3b82f6 !important;
        font-weight: 600 !important;
    }

    h4, h5, h6 {
        color: #3b82f6 !important;
        font-weight: 600 !important;
    }

    /* ===== LABELS - BLUE FONT ===== */
    .stTextInput > label,
    .stTextArea > label,
    .stSelectbox > label,
    .stSlider > label,
    .stCheckbox > label,
    .stRadio > label,
    .stNumberInput > label,
    .stDateInput > label,
    .stTimeInput > label {
        color: #1d4ed8 !important;
        font-weight: 600 !important;
        font-size: 14px !important;
        margin-bottom: 6px !important;
    }

    /* ===== METRICS - BLUE FONT ===== */
    [data-testid="stMetricValue"] {
        color: #1d4ed8 !important;
        font-size: 2.2rem !important;
        font-weight: 800 !important;
    }

    [data-testid="stMetricLabel"] {
        color: #2563eb !important;
        font-weight: 600 !important;
        font-size: 14px !important;
    }

    [data-testid="stMetricDelta"] {
        color: #3b82f6 !important;
    }

    /* ===== GENERAL TEXT - BLUE ===== */
    p {
        color: #1e40af !important;
    }

    span {
        color: #1e40af;
    }

    div {
        color: #1e40af;
    }

    /* ===== MARKDOWN TEXT - BLUE ===== */
    [data-testid="stMarkdownContainer"] p,
    [data-testid="stMarkdownContainer"] span,
    [data-testid="stMarkdownContainer"] li {
        color: #1e40af !important;
    }

    [data-testid="stMarkdownContainer"] strong,
    [data-testid="stMarkdownContainer"] b {
        color: #1d4ed8 !important;
        font-weight: 700 !important;
    }

    [data-testid="stMarkdownContainer"] a {
        color: #2563eb !important;
        text-decoration: underline;
    }

    /* ===== SUBHEADER - BLUE ===== */
    .stSubheader, [data-testid="stSubheader"] {
        color: #2563eb !important;
    }

    /* ===== TABS STYLING ===== */
    .stTabs [data-baseweb="tab-list"] {
        gap: 12px;
        background: white;
        padding: 10px 12px;
        border-radius: 16px;
        box-shadow: 0 4px 15px rgba(37, 99, 235, 0.08);
        border: 1px solid rgba(37, 99, 235, 0.1);
    }

    .stTabs [data-baseweb="tab"] {
        border-radius: 10px;
        padding: 12px 28px;
        font-weight: 600;
        color: #2563eb !important;
        background: transparent;
        transition: all 0.2s ease;
    }

    .stTabs [data-baseweb="tab"]:hover {
        background: #eff6ff;
    }

    .stTabs [aria-selected="true"] {
        background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%) !important;
        color: white !important;
        box-shadow: 0 4px 15px rgba(37, 99, 235, 0.3);
    }

    /* Tab text when selected */
    .stTabs [aria-selected="true"] p,
    .stTabs [aria-selected="true"] span {
        color: white !important;
    }

    /* ===== EXPANDER STYLING ===== */
    .streamlit-expanderHeader {
        background: white !important;
        border-radius: 12px !important;
        border: 1px solid rgba(37, 99, 235, 0.15) !important;
        font-weight: 600 !important;
        color: #2563eb !important;
        padding: 12px 16px !important;
    }

    .streamlit-expanderHeader:hover {
        border-color: #2563eb !important;
        background: #eff6ff !important;
    }

    /* Expander header text - force blue */
    .streamlit-expanderHeader p,
    .streamlit-expanderHeader span,
    .streamlit-expanderHeader div {
        color: #2563eb !important;
    }

    .streamlit-expanderHeader:hover p,
    .streamlit-expanderHeader:hover span,
    .streamlit-expanderHeader:hover div {
        color: #1d4ed8 !important;
    }

    /* Expander icon color */
    .streamlit-expanderHeader svg {
        fill: #2563eb !important;
    }

    .streamlit-expanderContent {
        background: white !important;
        border: 1px solid rgba(37, 99, 235, 0.1) !important;
        border-top: none !important;
        border-radius: 0 0 12px 12px !important;
        padding: 16px !important;
    }

    /* Expander content text - blue */
    .streamlit-expanderContent p,
    .streamlit-expanderContent span,
    .streamlit-expanderContent div {
        color: #1e40af !important;
    }

    [data-testid="stExpander"] {
        border: none !important;
        box-shadow: 0 2px 12px rgba(37, 99, 235, 0.08) !important;
        border-radius: 12px !important;
    }

    /* ===== DIVIDER ===== */
    hr {
        border: none !important;
        height: 2px !important;
        background: linear-gradient(90deg, transparent, #93c5fd, #2563eb, #93c5fd, transparent) !important;
        margin: 28px 0 !important;
        opacity: 0.6;
    }

    /* ===== FORM STYLING ===== */
    [data-testid="stForm"] {
        background: white;
        padding: 32px;
        border-radius: 20px;
        box-shadow: 0 8px 40px rgba(37, 99, 235, 0.1);
        border: 1px solid rgba(37, 99, 235, 0.08);
    }

    /* ===== LOGIN HEADER ===== */
    .login-header {
        text-align: center;
        color: #1d4ed8 !important;
        font-size: 48px;
        font-weight: 800;
        margin-bottom: 8px;
        letter-spacing: -1.5px;
        text-shadow: 0 2px 10px rgba(37, 99, 235, 0.1);
    }

    .login-subheader {
        text-align: center;
        color: #3b82f6 !important;
        font-size: 18px;
        margin-bottom: 36px;
        font-weight: 500;
    }

    /* ===== ALERT BOXES ===== */
    .success-box {
        background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%);
        border: 2px solid #34d399;
        color: #065f46 !important;
        padding: 18px 24px;
        border-radius: 14px;
        margin: 20px 0;
        font-weight: 600;
        box-shadow: 0 4px 15px rgba(52, 211, 153, 0.2);
    }

    .error-box {
        background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
        border: 2px solid #f87171;
        color: #991b1b !important;
        padding: 18px 24px;
        border-radius: 14px;
        margin: 20px 0;
        font-weight: 600;
        box-shadow: 0 4px 15px rgba(248, 113, 113, 0.2);
    }

    .info-box {
        background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
        border: 2px solid #60a5fa;
        color: #1e40af !important;
        padding: 18px 24px;
        border-radius: 14px;
        margin: 20px 0;
        font-weight: 600;
        box-shadow: 0 4px 15px rgba(96, 165, 250, 0.2);
    }

    /* ===== CLASS CODE BOX ===== */
    .class-code-box {
        background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 50%, #1e40af 100%);
        padding: 28px 36px;
        border-radius: 20px;
        text-align: center;
        box-shadow: 0 12px 40px rgba(37, 99, 235, 0.4);
        border: 2px solid rgba(255, 255, 255, 0.2);
    }

    .class-code-box h2 {
        color: white !important;
        margin: 0;
        font-size: 32px;
        font-weight: 800;
        letter-spacing: 6px;
        text-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
    }

    /* ===== SESSION CLOSED BOX ===== */
    .session-closed-box {
        background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
        padding: 28px;
        border-radius: 20px;
        text-align: center;
        border: 3px solid #f87171;
        box-shadow: 0 8px 30px rgba(248, 113, 113, 0.25);
    }

    .session-closed-box h2 {
        color: #dc2626 !important;
        margin: 0;
        font-weight: 800;
        font-size: 24px;
    }

    /* ===== QUIZ & STAT CARDS ===== */
    .quiz-card {
        background: white;
        border-radius: 18px;
        padding: 24px;
        margin-bottom: 18px;
        border: 1px solid rgba(37, 99, 235, 0.1);
        box-shadow: 0 4px 20px rgba(37, 99, 235, 0.08);
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .quiz-card:hover {
        box-shadow: 0 12px 35px rgba(37, 99, 235, 0.15);
        transform: translateY(-4px);
        border-color: #2563eb;
    }

    .stat-card {
        background: white;
        border-radius: 18px;
        padding: 28px;
        text-align: center;
        border: 1px solid rgba(37, 99, 235, 0.1);
        box-shadow: 0 4px 20px rgba(37, 99, 235, 0.08);
    }

    .stat-card h3 {
        color: #1d4ed8 !important;
        font-size: 36px;
        margin: 0;
        font-weight: 800;
    }

    .stat-card p {
        color: #3b82f6 !important;
        margin: 10px 0 0 0;
        font-weight: 600;
    }

    /* ===== NAVIGATION CARD ===== */
    .nav-card {
        background: white;
        border-radius: 18px;
        padding: 32px;
        text-align: center;
        border: 2px solid rgba(37, 99, 235, 0.1);
        cursor: pointer;
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .nav-card:hover {
        border-color: #2563eb;
        box-shadow: 0 12px 35px rgba(37, 99, 235, 0.2);
        transform: translateY(-4px);
    }

    /* ===== FOOTER ===== */
    .footer {
        text-align: center;
        color: #3b82f6 !important;
        font-size: 14px;
        padding: 28px 0;
        margin-top: 48px;
    }

    .footer p {
        color: #3b82f6 !important;
        margin: 4px 0;
    }

    /* ===== CHECKBOX STYLING ===== */
    .stCheckbox > label {
        color: #2563eb !important;
        font-weight: 600 !important;
    }

    .stCheckbox > label > span {
        color: #2563eb !important;
    }

    [data-testid="stCheckbox"] label span {
        color: #2563eb !important;
    }

    /* ===== SLIDER STYLING ===== */
    .stSlider > div > div > div {
        background: linear-gradient(90deg, #2563eb, #1d4ed8) !important;
    }

    .stSlider > div > div > div > div {
        background: #1d4ed8 !important;
    }

    /* ===== CAPTION TEXT ===== */
    .stCaption, [data-testid="stCaptionContainer"] {
        color: #3b82f6 !important;
    }

    [data-testid="stCaptionContainer"] p {
        color: #3b82f6 !important;
    }

    /* ===== ALERTS (INFO, WARNING, ERROR, SUCCESS) ===== */
    .stAlert > div {
        border-radius: 12px !important;
    }

    [data-testid="stAlert"] {
        border-radius: 12px !important;
    }

    /* ===== SPINNER ===== */
    .stSpinner > div {
        border-color: #2563eb !important;
    }

    /* ===== WRITE TEXT ===== */
    [data-testid="stText"] {
        color: #1e40af !important;
    }

    /* ===== TITLE ===== */
    [data-testid="stTitle"], .stTitle {
        color: #1d4ed8 !important;
    }

    /* ===== TABLE STYLING ===== */
    .stDataFrame {
        border-radius: 12px !important;
        overflow: hidden;
    }

    /* ===== PLOTLY CHART CONTAINER ===== */
    [data-testid="stPlotlyChart"] {
        background: white;
        border-radius: 16px;
        padding: 16px;
        box-shadow: 0 4px 20px rgba(37, 99, 235, 0.08);
    }

    /* ===== COLUMN CONTAINERS ===== */
    [data-testid="column"] {
        background: transparent;
    }

    /* ===== BUTTON TEXT COLOR FIX ===== */
    .stButton > button p,
    .stButton > button span {
        color: white !important;
    }

    .stButton > button[kind="secondary"] p,
    .stButton > button[kind="secondary"] span {
        color: #2563eb !important;
    }

</style>
""", unsafe_allow_html=True)

def hash_password(password):
    """Hash password using SHA256"""
    return hashlib.sha256(password.encode()).hexdigest()

def login_teacher(email, password):
    """Authenticate teacher"""
    try:
        response = requests.post(
            f"{API_URL}/api/auth/login",
            json={"email": email, "password": password}
        )
        if response.status_code == 200:
            data = response.json()
            save_login_file(data['teacher_id'], data['username'], data['email'])
            return data
        else:
            return None
    except Exception as e:
        st.error(f"Connection error: {e}")
        return None

def save_login_file(teacher_id, username, email):
    """Save login info to file that PowerPoint can read"""
    try:
        home_dir = os.path.expanduser("~")
        login_file = os.path.join(home_dir, "teacher_login.txt")

        with open(login_file, "w") as f:
            f.write(f"{teacher_id}\n")
            f.write(f"{username}\n")
            f.write(f"{email}\n")

        print(f"Login saved to: {login_file}")
    except Exception as e:
        print(f"Error saving login file: {e}")

def register_teacher(username, email, password):
    """Register new teacher"""
    try:
        response = requests.post(
            f"{API_URL}/api/auth/register",
            json={
                "username": username,
                "email": email,
                "password": password
            }
        )
        if response.status_code == 200:
            return response.json()
        else:
            return None
    except Exception as e:
        st.error(f"Connection error: {e}")
        return None

# Initialize session state
if 'logged_in' not in st.session_state:
    st.session_state.logged_in = False
if 'teacher_id' not in st.session_state:
    st.session_state.teacher_id = None
if 'teacher_email' not in st.session_state:
    st.session_state.teacher_email = None
if 'teacher_name' not in st.session_state:
    st.session_state.teacher_name = None
if 'page' not in st.session_state:
    st.session_state.page = 'login'

# Helper functions
def generate_class_code():
    """Generate random class code"""
    return "".join(random.choices(string.ascii_uppercase + string.digits, k=6))

def logout():
    """Logout function"""
    try:
        home_dir = os.path.expanduser("~")
        login_file = os.path.join(home_dir, "teacher_login.txt")
        if os.path.exists(login_file):
            os.remove(login_file)
    except Exception as e:
        print(f"Error deleting login file: {e}")

    st.session_state.logged_in = False
    st.session_state.teacher_id = None
    st.session_state.teacher_email = None
    st.session_state.teacher_name = None
    st.session_state.page = 'login'
    st.rerun()

# Dashboard Page
def show_dashboard():
    # Header
    col1, col2 = st.columns([4, 1])
    with col1:
        st.title(f"Welcome, {st.session_state.teacher_name}!")
    with col2:
        if st.button("Logout", type="secondary"):
            logout()

    st.divider()

    # Stats
    quizzes = get_teacher_quizzes(st.session_state.teacher_id)

    col1, col2, col3 = st.columns(3)
    with col1:
        st.metric("Total Quizzes", len(quizzes))
    with col2:
        total_sessions = sum(q.get('session_count', 0) for q in quizzes)
        st.metric("Total Sessions", total_sessions)
    with col3:
        st.metric("Teacher ID", st.session_state.teacher_id)

    st.divider()

    # Navigation
    st.subheader("What would you like to do?")

    col1, col2, col3 = st.columns(3)

    with col1:
        if st.button("Create New Quiz", use_container_width=True, type="primary"):
            st.session_state.page = 'create_quiz'
            st.rerun()

    with col2:
        if st.button("View My Quizzes", use_container_width=True):
            st.session_state.page = 'view_quizzes'
            st.rerun()

    with col3:
        if st.button("Run Quiz Session", use_container_width=True):
            st.session_state.page = 'run_quiz'
            st.rerun()

    # Recent quizzes
    if quizzes:
        st.divider()
        st.subheader("Recent Quizzes")

        for quiz in quizzes[:5]:
            with st.expander(f"{quiz['title']}"):
                col1, col2, col3, col4 = st.columns(4)
                with col1:
                    st.write(f"**Choices:** {quiz['num_choices']}")
                with col2:
                    st.write(f"**Sessions:** {quiz.get('session_count', 0)}")
                with col3:
                    created = quiz['created_at']
                    if isinstance(created, str):
                        created = datetime.fromisoformat(created)
                    st.write(f"**Created:** {created.strftime('%Y-%m-%d')}")
                with col4:
                    if st.button("Run", key=f"run_{quiz['quiz_id']}"):
                        st.session_state.selected_quiz_id = quiz['quiz_id']
                        st.session_state.page = 'run_quiz'
                        st.rerun()

# ✅ FIXED: Create Quiz Page
def show_create_quiz():
    st.title("Create New Quiz")

    if st.button("Back to Dashboard"):
        st.session_state.page = 'dashboard'
        st.rerun()

    st.divider()

    with st.form("create_quiz_form"):
        # Quiz Title
        st.subheader("Quiz Details")
        title = st.text_input("Quiz Title", value="My Quiz")

        # Question
        st.subheader("Question")
        question_text = st.text_area("Question Text", height=100,
                                     placeholder="Enter your question here...")

        # Number of choices
        num_choices = st.selectbox("Number of Choices", [2, 3, 4, 5, 6, 7, 8], index=2)

        # Answer choices with correct answer selection
        st.subheader("Answer Choices")
        answers = []
        correct_answers = []

        for i in range(num_choices):
            col1, col2 = st.columns([4, 1])
            with col1:
                answer_text = st.text_input(
                    f"Choice {chr(65+i)}",
                    value="",
                    key=f"answer_{i}",
                    placeholder=f"Enter answer {chr(65+i)}"
                )
                answers.append(answer_text)
            with col2:
                st.write("")  # Spacing
                st.write("")  # More spacing to align checkbox
                is_correct = st.checkbox("✓ Correct", key=f"correct_{i}")
                if is_correct:
                    correct_answers.append(i)

        # ✅ FIXED: Quiz Options with proper attribute names
        st.subheader("Quiz Options")
        col1, col2 = st.columns(2)

        with col1:
            has_correct = st.checkbox("Has correct answer(s)", value=True)
            quiz_mode = st.selectbox("Quiz Difficulty", ["Easy", "Medium", "Hard"], index=0)

        with col2:
            close_after = st.slider("Auto-close submission (minutes)", 1, 30, 5)

        # Submit
        submit = st.form_submit_button("Create Quiz", type="primary", use_container_width=True)

    if submit:
        if not title or not question_text:
            st.error("Please fill in title and question!")
        elif any(not ans.strip() for ans in answers):
            st.error("Please fill in all answer choices!")
        elif has_correct and not correct_answers:
            st.error("Please mark at least one correct answer!")
        else:
            # ✅ FIXED: Create quiz via API with correct parameters
            try:
                # Prepare answers with correct marking
                answers_list = []
                for i in range(num_choices):
                    answers_list.append({
                        'text': answers[i].strip(),
                        'order': i,
                        'is_correct': i in correct_answers if has_correct else False
                    })

                # Automatically determine allow_multiple based on correct answers
                allow_multiple = len(correct_answers) >= 2

                # ✅ FIXED: Send request with correct field names
                response = requests.post(
                    f"{API_URL}/api/quiz/create",
                    params={
                        'teacher_id': st.session_state.teacher_id,
                        'title': title,
                        'question_text': question_text,
                        'num_choices': num_choices,
                        'allow_multiple': allow_multiple,
                        'has_correct': has_correct,
                        'quiz_mode': quiz_mode.lower(),  # "easy", "medium", or "hard"
                        'start_with_slide': True,  # Default
                        'minimize_window': False,  # Default
                        'auto_close_minutes': close_after
                    },
                    json={'answers': answers_list}
                )

                if response.status_code == 200:
                    result = response.json()
                    st.success(f"✅ Quiz created successfully! (ID: {result['quiz_id']})")
                    st.balloons()
                    time.sleep(2)
                    st.session_state.page = 'dashboard'
                    st.rerun()
                else:
                    st.error(f"❌ Error creating quiz: {response.text}")
            except Exception as e:
                st.error(f"❌ Error: {str(e)}")

# View Quizzes Page
def show_view_quizzes():
    st.title("My Quizzes")

    if st.button("Back to Dashboard"):
        st.session_state.page = 'dashboard'
        st.rerun()

    st.divider()

    quizzes = get_teacher_quizzes(st.session_state.teacher_id)

    if not quizzes:
        st.info("You haven't created any quizzes yet. Click 'Create New Quiz' to get started!")
    else:
        for quiz in quizzes:
            with st.container():
                col1, col2, col3, col4, col5 = st.columns([3, 1, 1, 1, 1])

                with col1:
                    st.subheader(f"{quiz['title']}")
                with col2:
                    st.metric("Choices", quiz['num_choices'])
                with col3:
                    st.metric("Sessions", quiz.get('session_count', 0))
                with col4:
                    if st.button("Run", key=f"run_{quiz['quiz_id']}"):
                        st.session_state.selected_quiz_id = quiz['quiz_id']
                        st.session_state.page = 'run_quiz'
                        st.rerun()
                with col5:
                    if st.button("Results", key=f"results_{quiz['quiz_id']}"):
                        st.session_state.selected_quiz_id = quiz['quiz_id']
                        st.session_state.page = 'view_results'
                        st.rerun()

                created = quiz['created_at']
                if isinstance(created, str):
                    created = datetime.fromisoformat(created)
                st.caption(f"Created: {created.strftime('%Y-%m-%d %H:%M')}")
                st.divider()

# Run Quiz Page
def show_run_quiz():
    st.title("Run Quiz Session")

    if st.button("Back to Dashboard"):
        st.session_state.page = 'dashboard'
        st.rerun()

    st.divider()

    # Select quiz
    quizzes = get_teacher_quizzes(st.session_state.teacher_id)

    if not quizzes:
        st.warning("You need to create a quiz first!")
        return

    quiz_options = {f"{q['title']} (ID: {q['quiz_id']})": q['quiz_id'] for q in quizzes}

    # Pre-select if coming from dashboard
    default_idx = 0
    if 'selected_quiz_id' in st.session_state:
        for idx, (key, val) in enumerate(quiz_options.items()):
            if val == st.session_state.selected_quiz_id:
                default_idx = idx
                break

    selected = st.selectbox("Select Quiz", options=list(quiz_options.keys()), index=default_idx)
    quiz_id = quiz_options[selected]

    if st.button("Start Quiz Session", type="primary", use_container_width=True):
        # Generate class code
        class_code = generate_class_code()

        # Create session
        session_id, error = create_quiz_session(quiz_id, class_code)

        if session_id:
            st.session_state.active_session_id = session_id
            st.session_state.class_code = class_code
            st.session_state.page = 'live_session'
            st.rerun()
        else:
            st.error(f"Failed to start session: {error}")

# ✅ COMPLETELY FIXED: Live Session Page with correct statistics
def show_live_session():
    """Live session page with real-time results"""

    # Auto-refresh every 3 seconds
    st_autorefresh(interval=3000, key="live_refresh")

    # Validate session_state
    session_id = st.session_state.get('active_session_id')
    class_code = st.session_state.get('class_code')
    if not session_id or not class_code:
        st.error("Session not active! Please start a session from the dashboard.")
        if st.button("Back to Dashboard"):
            st.session_state.page = 'dashboard'
            st.rerun()
        return

    # Get session data
    session = get_session_by_code(class_code)
    if not session:
        st.error("Session not found!")
        return

    # Ensure started_at is a datetime
    start_time = session.get('started_at')
    if isinstance(start_time, str):
        try:
            start_time = datetime.fromisoformat(start_time)
        except Exception:
            start_time = datetime.now()
    session['started_at'] = start_time

    # Get quiz details
    quiz_details = get_quiz_details(session['quiz_id'])
    if not quiz_details:
        st.error("Quiz details not found!")
        return

    quiz = quiz_details['quiz']
    question = quiz_details['question']

    # Header with class code
    col1, col2, col3 = st.columns([2, 2, 1])

    is_closed = session.get('status') == 'closed'

    with col1:
        st.title("Live Session")

    with col2:
        if is_closed:
            st.markdown("""
            <div class='session-closed-box'>
                <h2>Submissions CLOSED</h2>
            </div>
            """, unsafe_allow_html=True)
        else:
            st.markdown(f"""
            <div class='class-code-box'>
                <h2>Class Code: {class_code}</h2>
            </div>
            """, unsafe_allow_html=True)

    with col3:
        if is_closed:
            if st.button("Back to Dashboard"):
                st.session_state.page = 'dashboard'
                st.rerun()
        else:
            if st.button("Close Session"):
                close_session(session_id)
                st.success("Session closed!")
                time.sleep(1)
                st.session_state.page = 'dashboard'
                st.rerun()

    st.divider()

    # Get live results
    results_data = get_session_results(session_id)

    if not results_data:
        st.error("Unable to fetch results!")
        return

    results = results_data['results']
    participant_count = results_data['participant_count']

    # Question display
    col1, col2 = st.columns([3, 1])

    with col1:
        st.subheader("Question")
        st.markdown(f"**{question['question_text']}**")

    with col2:
        st.metric("Students Joined", participant_count)

    st.divider()

    # Bar Chart
    st.subheader("Live Results")

    if not results or all(r['count'] == 0 for r in results):
        st.info("Waiting for student responses...")
        st.markdown("Students can join using the class code above.")
    else:
        # Prepare data for chart
        chart_data = []
        for r in results:
            choice_label = chr(65 + r['answer_order'])
            chart_data.append({
                'Choice': f"{choice_label}. {r['answer_text'][:30]}...",
                'Responses': r['count'],
                'Is Correct': 'Correct' if r['is_correct'] else 'Incorrect'
            })

        df = pd.DataFrame(chart_data)

        # Create bar chart with blue theme
        fig = px.bar(
            df,
            x='Choice',
            y='Responses',
            color='Is Correct',
            color_discrete_map={'Correct': '#22c55e', 'Incorrect': '#3b82f6'},
            text='Responses',
            title='Student Response Distribution'
        )

        fig.update_traces(textposition='outside')
        fig.update_layout(
            height=500,
            xaxis_title="Answer Choices",
            yaxis_title="Number of Students",
            showlegend=True,
            paper_bgcolor='rgba(0,0,0,0)',
            plot_bgcolor='rgba(0,0,0,0)',
            font=dict(family="Inter, sans-serif", color="#1d4ed8")
        )

        st.plotly_chart(fig, use_container_width=True)

        # ✅ COMPLETELY FIXED: Statistics with correct calculations
        st.divider()
        st.subheader("Statistics")

        try:
            conn = get_db_connection()
            cur = conn.cursor()
            
            # ✅ FIX 1: Count unique students who submitted at least one answer
            cur.execute("""
                SELECT COUNT(DISTINCT sa.student_id) as responded_count
                FROM student_answers sa
                WHERE sa.session_id = %s
            """, (session_id,))
            
            result = cur.fetchone()
            students_responded = result['responded_count'] if result and result['responded_count'] else 0
            
            # ✅ FIX 2: Get correct student count with proper logic
            correct_students = 0
            
            if quiz.get('has_correct'):
                # Get student answers grouped by student
                cur.execute("""
                    SELECT 
                        sa.student_id,
                        sa.is_correct
                    FROM student_answers sa
                    WHERE sa.session_id = %s
                    ORDER BY sa.student_id
                """, (session_id,))
                
                student_answers = cur.fetchall()
                
                # Group by student and check if ALL their answers are correct
                from collections import defaultdict
                student_correctness = defaultdict(list)
                
                for row in student_answers:
                    student_correctness[row['student_id']].append(row['is_correct'])
                
                # Count students where all answers are correct
                for student_id, correctness_list in student_correctness.items():
                    if all(correctness_list):  # All answers must be correct
                        correct_students += 1
            
            cur.close()
            conn.close()
            
        except Exception as e:
            print(f"Error calculating statistics: {e}")
            import traceback
            traceback.print_exc()
            students_responded = 0
            correct_students = 0

        # Display metrics
        col1, col2, col3, col4 = st.columns(4)

        with col1:
            st.metric("Students Responded", students_responded)

        with col2:
            response_rate = (students_responded / participant_count * 100) if participant_count > 0 else 0
            st.metric("Response Rate", f"{response_rate:.1f}%")

        with col3:
            if quiz.get('has_correct'):
                accuracy = (correct_students / students_responded * 100) if students_responded > 0 else 0
                st.metric("Accuracy", f"{accuracy:.1f}%")
            else:
                st.metric("Grading", "Disabled")

        with col4:
            elapsed = datetime.now() - start_time
            minutes = int(elapsed.total_seconds() / 60)
            st.metric("Time Elapsed", f"{minutes} min")

    # Bottom controls
    st.divider()
    col1, col2 = st.columns(2)

    with col1:
        if st.button("Refresh Now", use_container_width=True):
            st.rerun()

    with col2:
        if not is_closed:
            if st.button("Close Submission", use_container_width=True):
                success = close_session(session_id)
                if success:
                    st.success("Session closed!")
                    time.sleep(1)
                    st.rerun()

# View Results Page
def show_view_results():
    st.title("Quiz Results")

    if st.button("Back to Quizzes"):
        st.session_state.page = 'view_quizzes'
        st.rerun()

    st.divider()

    if 'selected_quiz_id' not in st.session_state:
        st.warning("No quiz selected!")
        return

    quiz_id = st.session_state.selected_quiz_id

    # Get quiz details
    quiz_details = get_quiz_details(quiz_id)
    if not quiz_details:
        st.error("Quiz not found!")
        return

    quiz = quiz_details['quiz']
    question = quiz_details['question']

    st.subheader(f"{quiz['title']}")
    st.markdown(f"**Question:** {question['question_text']}")

    st.divider()

    # Get all sessions for this quiz
    try:
        conn = get_db_connection()
        cur = conn.cursor()
        cur.execute("""
            SELECT session_id, class_code, started_at, closed_at, status
            FROM quiz_sessions
            WHERE quiz_id = %s
            ORDER BY started_at DESC
        """, (quiz_id,))
        sessions = cur.fetchall()
        cur.close()
        conn.close()
    except Exception as e:
        st.error(f"Error fetching sessions: {e}")
        return

    if not sessions:
        st.info("No sessions found for this quiz yet.")
        return

    # Display each session
    for session in sessions:
        session_id = session['session_id']

        with st.expander(f"Session: {session['class_code']} - {session['started_at'].strftime('%Y-%m-%d %H:%M')}"):
            # Get results for this session
            results_data = get_session_results(session_id)

            if not results_data:
                st.warning("No results available")
                continue

            results = results_data['results']
            participant_count = results_data['participant_count']

            col1, col2, col3 = st.columns(3)
            with col1:
                st.metric("Participants", participant_count)
            with col2:
                # ✅ FIXED: Count unique students who responded
                try:
                    conn = get_db_connection()
                    cur = conn.cursor()
                    cur.execute("""
                        SELECT COUNT(DISTINCT student_id) as count
                        FROM student_answers
                        WHERE session_id = %s
                    """, (session_id,))
                    total_responses = cur.fetchone()['count']
                    cur.close()
                    conn.close()
                except:
                    total_responses = sum(r['count'] for r in results)
                
                st.metric("Responses", total_responses)
            with col3:
                status = session['status']
                st.metric("Status", "Closed" if status == 'closed' else "Active")

            # Get student details
            try:
                conn = get_db_connection()
                cur = conn.cursor()
                cur.execute("""
                    SELECT s.name, a.answer_text, a.is_correct
                    FROM students s
                    LEFT JOIN student_answers sa ON s.student_id = sa.student_id
                    LEFT JOIN answers a ON sa.answer_id = a.answer_id
                    WHERE s.session_id = %s
                    ORDER BY s.name
                """, (session_id,))
                student_data = cur.fetchall()
                cur.close()
                conn.close()

                if student_data:
                    st.subheader("Student Answers")

                    # Group by student (handle multiple selections)
                    from collections import defaultdict
                    student_responses = defaultdict(list)
                    for row in student_data:
                        student_responses[row['name']].append({
                            'answer': row['answer_text'] or 'No answer',
                            'correct': row['is_correct']
                        })

                    # Display in table format
                    for student_name, responses in student_responses.items():
                        col1, col2, col3 = st.columns([2, 3, 1])
                        with col1:
                            st.write(f"**{student_name}**")
                        with col2:
                            answer_texts = [r['answer'] for r in responses]
                            st.write(", ".join(answer_texts))
                        with col3:
                            if responses[0]['answer'] != 'No answer':
                                all_correct = all(r['correct'] for r in responses if r['answer'] != 'No answer')
                                st.write("✅ Correct" if all_correct else "❌ Incorrect")
                            else:
                                st.write("⚪ No answer")
                else:
                    st.info("No student responses yet")
            except Exception as e:
                st.error(f"Error fetching student data: {e}")

# Main UI
if not st.session_state.logged_in:
    # Login/Register page
    st.markdown('<div class="login-header">ClassPoint Teacher Portal</div>', unsafe_allow_html=True)
    st.markdown('<div class="login-subheader">Login or Register to Continue</div>', unsafe_allow_html=True)

    tab1, tab2 = st.tabs(["Login", "Register"])

    with tab1:
        st.subheader("Login to Your Account")

        with st.form("login_form"):
            email = st.text_input("Email Address")
            password = st.text_input("Password", type="password")
            submit = st.form_submit_button("Login", type="primary", use_container_width=True)

            if submit:
                if email and password:
                    with st.spinner("Logging in..."):
                        result = login_teacher(email, password)
                        if result:
                            st.session_state.logged_in = True
                            st.session_state.teacher_id = result['teacher_id']
                            st.session_state.teacher_email = result['email']
                            st.session_state.teacher_name = result['username']
                            st.session_state.page = 'dashboard'
                            st.success(f"Welcome back, {result['username']}!")
                            st.rerun()
                        else:
                            st.markdown('<div class="error-box">Invalid email or password</div>', unsafe_allow_html=True)
                else:
                    st.warning("Please enter email and password")

    with tab2:
        st.subheader("Create New Account")

        with st.form("register_form"):
            username = st.text_input("Username")
            email = st.text_input("Email Address")
            password = st.text_input("Password", type="password")
            password_confirm = st.text_input("Confirm Password", type="password")
            submit = st.form_submit_button("Register", type="primary", use_container_width=True)

            if submit:
                if not all([username, email, password, password_confirm]):
                    st.warning("Please fill all fields")
                elif password != password_confirm:
                    st.error("Passwords do not match")
                elif len(password) < 6:
                    st.error("Password must be at least 6 characters")
                else:
                    with st.spinner("Creating account..."):
                        result = register_teacher(username, email, password)
                        if result:
                            st.markdown(
                                '<div class="success-box">Account created successfully! Please login.</div>',
                                unsafe_allow_html=True
                            )
                        else:
                            st.markdown(
                                '<div class="error-box">Registration failed. Email may already exist.</div>',
                                unsafe_allow_html=True
                            )

else:
    # Logged in - Show pages based on navigation
    page = st.session_state.page

    if page == 'dashboard':
        show_dashboard()
    elif page == 'create_quiz':
        show_create_quiz()
    elif page == 'view_quizzes':
        show_view_quizzes()
    elif page == 'run_quiz':
        show_run_quiz()
    elif page == 'live_session':
        show_live_session()
    elif page == 'view_results':
        show_view_results()

# Footer
st.markdown("---")
st.markdown("""
<div class='footer'>
    <p>ClassPoint Quiz System</p>
    <p>For support: support@classpoint.com</p>
</div>
""", unsafe_allow_html=True)
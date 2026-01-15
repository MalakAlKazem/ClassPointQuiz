# student_app.py
# Streamlit app for students to join and answer quizzes

import streamlit as st
import sys
sys.path.insert(0, r'C:\Users\pc\Desktop\ClassPointQuiz')
from backend import database
import time


from backend.database import (
    get_session_by_code,
    add_student_to_session,
    get_quiz_details,
    submit_answer
)
from streamlit_autorefresh import st_autorefresh

# Page config
st.set_page_config(
    page_title="ClassPoint Student",
    page_icon="🎓",
    layout="centered"
)

# Custom CSS for classy white and blue design
st.markdown("""
<style>
    /* Hide Streamlit banner, header, menu, and footer */
    #MainMenu {visibility: hidden;}
    .stDeployButton {display: none;}
    header {visibility: hidden;}
    footer {visibility: hidden;}
    [data-testid="stHeader"] {display: none;}
    [data-testid="stToolbar"] {display: none;}
    .viewerBadge_container__r5tak {display: none;}
    .styles_viewerBadge__CvC9N {display: none;}

    /* Main background - clean white */
    .stApp {
        background: linear-gradient(135deg, #ffffff 0%, #f0f4f8 100%);
    }

    /* Main container styling */
    .main .block-container {
        background-color: #ffffff;
        border-radius: 15px;
        padding: 2rem;
        box-shadow: 0 4px 20px rgba(0, 82, 155, 0.1);
        border: 1px solid #e0e8f0;
    }

    /* Headers - blue font */
    h1, h2, h3, .stTitle, .stSubheader {
        color: #0052a3 !important;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }

    /* Regular text - blue */
    p, span, label, .stMarkdown {
        color: #1a5490 !important;
    }

    /* General div text */
    div {
        color: #1a5490;
    }

    /* Input fields - blue text */
    .stTextInput input {
        background-color: #ffffff !important;
        border: 2px solid #0066cc !important;
        border-radius: 8px !important;
        color: #1a5490 !important;
        font-weight: 500 !important;
    }

    .stTextInput input:focus {
        border-color: #0052a3 !important;
        box-shadow: 0 0 10px rgba(0, 102, 204, 0.3) !important;
    }

    .stTextInput input::placeholder {
        color: #6688aa !important;
    }

    /* Primary buttons - blue background with WHITE text */
    .stButton > button[kind="primary"],
    .stFormSubmitButton > button,
    .stButton > button[type="primary"] {
        background: linear-gradient(135deg, #0066cc 0%, #0052a3 100%) !important;
        color: #ffffff !important;
        border: none !important;
        border-radius: 10px !important;
        padding: 0.6rem 1.5rem !important;
        font-weight: 600 !important;
        box-shadow: 0 4px 15px rgba(0, 102, 204, 0.4) !important;
        transition: all 0.3s ease !important;
    }

    .stButton > button[kind="primary"]:hover,
    .stFormSubmitButton > button:hover,
    .stButton > button[type="primary"]:hover {
        background: linear-gradient(135deg, #0052a3 0%, #003d7a 100%) !important;
        box-shadow: 0 6px 20px rgba(0, 102, 204, 0.5) !important;
        transform: translateY(-2px) !important;
        color: #ffffff !important;
    }

    /* Force white text on all primary buttons */
    .stButton > button[kind="primary"] p,
    .stButton > button[kind="primary"] span,
    .stButton > button[kind="primary"] div,
    .stFormSubmitButton > button p,
    .stFormSubmitButton > button span,
    .stFormSubmitButton > button div,
    .stButton > button[type="primary"] p,
    .stButton > button[type="primary"] span,
    .stButton > button[type="primary"] div {
        color: #ffffff !important;
    }

    /* Default buttons - also blue background for student app */
    .stButton > button {
        background: linear-gradient(135deg, #0066cc 0%, #0052a3 100%) !important;
        color: #ffffff !important;
        border: none !important;
        border-radius: 10px !important;
        font-weight: 600 !important;
        box-shadow: 0 4px 15px rgba(0, 102, 204, 0.4) !important;
        transition: all 0.3s ease !important;
    }

    .stButton > button:hover {
        background: linear-gradient(135deg, #0052a3 0%, #003d7a 100%) !important;
        color: #ffffff !important;
        box-shadow: 0 6px 20px rgba(0, 102, 204, 0.5) !important;
    }

    /* Force white text on all buttons */
    .stButton > button p,
    .stButton > button span,
    .stButton > button div {
        color: #ffffff !important;
    }

    /* Secondary/outline style buttons */
    .stButton > button[kind="secondary"] {
        background: #ffffff !important;
        color: #0066cc !important;
        border: 2px solid #0066cc !important;
        box-shadow: none !important;
    }

    .stButton > button[kind="secondary"]:hover {
        background: #e6f2ff !important;
        color: #0052a3 !important;
    }

    .stButton > button[kind="secondary"] p,
    .stButton > button[kind="secondary"] span,
    .stButton > button[kind="secondary"] div {
        color: #0066cc !important;
    }

    /* Info boxes */
    .stAlert {
        background-color: #e6f2ff !important;
        border: 1px solid #0066cc !important;
        border-radius: 10px !important;
        color: #0052a3 !important;
    }

    .stAlert p, .stAlert span, .stAlert div {
        color: #0052a3 !important;
    }

    /* Success messages */
    [data-testid="stAlert"][data-baseweb="notification"] {
        border-radius: 10px !important;
    }

    /* Radio buttons - blue text */
    .stRadio label {
        color: #1a5490 !important;
        font-weight: 500 !important;
    }

    .stRadio label span {
        color: #1a5490 !important;
    }

    .stRadio > div {
        background-color: #f8fbff !important;
        padding: 1rem !important;
        border-radius: 10px !important;
        border: 1px solid #d0e0f0 !important;
    }

    .stRadio [role="radiogroup"] label {
        color: #1a5490 !important;
    }

    .stRadio [role="radiogroup"] label p,
    .stRadio [role="radiogroup"] label span {
        color: #1a5490 !important;
    }

    /* Checkbox styling - blue text */
    .stCheckbox label {
        color: #1a5490 !important;
        font-weight: 500 !important;
    }

    .stCheckbox label span {
        color: #1a5490 !important;
    }

    .stCheckbox label p {
        color: #1a5490 !important;
    }

    .stCheckbox {
        background-color: #f8fbff !important;
        padding: 0.5rem 1rem !important;
        border-radius: 8px !important;
        margin: 0.3rem 0 !important;
        border: 1px solid #d0e0f0 !important;
    }

    /* Divider */
    hr {
        border-color: #0066cc !important;
        opacity: 0.3 !important;
    }

    /* Caption text */
    .stCaption {
        color: #5588bb !important;
    }

    .stCaption p {
        color: #5588bb !important;
    }

    /* Form styling */
    [data-testid="stForm"] {
        background-color: #f8fbff !important;
        padding: 1.5rem !important;
        border-radius: 12px !important;
        border: 1px solid #d0e0f0 !important;
    }

    /* Markdown text */
    [data-testid="stMarkdownContainer"] p,
    [data-testid="stMarkdownContainer"] span,
    [data-testid="stMarkdownContainer"] li {
        color: #1a5490 !important;
    }

    [data-testid="stMarkdownContainer"] strong,
    [data-testid="stMarkdownContainer"] b {
        color: #0052a3 !important;
    }

    /* Sidebar if used */
    .css-1d391kg {
        background-color: #0052a3 !important;
    }
</style>
""", unsafe_allow_html=True)

# Session state
if 'student_joined' not in st.session_state:
    st.session_state.student_joined = False
if 'student_id' not in st.session_state:
    st.session_state.student_id = None
if 'session_id' not in st.session_state:
    st.session_state.session_id = None
if 'answered' not in st.session_state:
    st.session_state.answered = False
if 'start_time' not in st.session_state:
    st.session_state.start_time = None

# Join Page
def show_join_page():
    st.title("🎓 ClassPoint Student")
    st.subheader("Join a Quiz Session")

    col1, col2 = st.columns([2, 1])

    with col1:
        with st.form("join_form"):
            class_code = st.text_input(
                "Enter Class Code",
                max_chars=10,
                placeholder="e.g., ABC123"
            ).upper()

            student_name = st.text_input(
                "Your Name",
                placeholder="Enter your name"
            )

            submit = st.form_submit_button("Join Quiz", type="primary", use_container_width=True)

            if submit:
                if not class_code or not student_name:
                    st.error("Please enter both class code and your name!")
                else:
                    # Check if session exists
                    session = get_session_by_code(class_code)

                    if not session:
                        st.error("Invalid class code!")
                    elif session['status'] != 'active':
                        st.warning("⚠️ This quiz session is not currently active. Please check with your teacher.")
                    else:
                        # Add student to session
                        student_id, error = add_student_to_session(
                            session['session_id'],
                            student_name
                        )

                        if student_id:
                            st.session_state.student_joined = True
                            st.session_state.student_id = student_id
                            st.session_state.session_id = session['session_id']
                            st.session_state.student_name = student_name
                            st.session_state.class_code = class_code
                            st.session_state.start_time = time.time()
                            st.success(f"Welcome, {student_name}!")
                            st.rerun()
                        else:
                            st.error(f"Failed to join: {error}")

    with col2:
        st.info("""
        **How to Join:**

        1. Get the class code from your teacher
        2. Enter the code above
        3. Enter your name
        4. Click Join Quiz
        """)

# Quiz Page
def show_quiz_page():
    st.title("🎓 Quiz Time!")

    # Check session status on every render
    session = get_session_by_code(st.session_state.class_code)

    # If session doesn't exist or is closed, show message and exit
    if not session or session.get('status') != 'active':
        st.warning("⚠️ This quiz session has been closed by your teacher.")
        st.info("Thank you for participating!")

        if st.button("Return to Join Page"):
            st.session_state.clear()
            st.rerun()
        return

    # Get quiz details
    quiz_details = get_quiz_details(session['quiz_id'])

    if not quiz_details or not quiz_details['question']:
        st.error("Quiz not found!")
        return

    quiz = quiz_details['quiz']
    question = quiz_details['question']
    answers = quiz_details['answers']

    # Header
    st.subheader(f"👤 {st.session_state.student_name}")
    st.caption(f"Class Code: {st.session_state.class_code}")

    # ✅ NEW: Show difficulty badge
    difficulty = quiz.get('quiz_difficulty', 'easy').upper()

    # Custom CSS for difficulty badges
    if difficulty == 'EASY':
        st.success(f"🟢 **{difficulty} QUIZ**")
    elif difficulty == 'MEDIUM':
        st.warning(f"🟡 **{difficulty} QUIZ**")
    elif difficulty == 'HARD':
        st.error(f"🔴 **{difficulty} QUIZ**")
    else:
       # Fallback
       st.info(f"ℹ️ **QUIZ**")
    st.divider()

    if st.session_state.answered:
        # Show results
        st.success("✅ Answer submitted!")
        st.balloons()

        st.info("**Thank you for participating!**\n\nYour teacher will review the results.")

        if st.button("Leave Quiz"):
            st.session_state.clear()
            st.rerun()

    else:
        # Show question
        st.markdown(f"### {question['question_text']}")
        st.divider()

        # Initialize selected answers in session state
        if 'selected_answers' not in st.session_state:
            st.session_state.selected_answers = []

        # Answer options
        if quiz['allow_multiple']:
            # Multiple selection with limit
            correct_count = quiz.get('correct_count', 2)
            selected_count = len(st.session_state.selected_answers)

            st.info(f"ℹ️ **Select exactly {correct_count} answer{'s' if correct_count != 1 else ''}** ({selected_count}/{correct_count} selected)")

            # Display checkboxes (outside form to allow dynamic disabling)
            for ans in answers:
                answer_id = ans['answer_id']
                is_checked = answer_id in st.session_state.selected_answers
                is_disabled = not is_checked and selected_count >= correct_count

                if st.checkbox(
                    f"{chr(65 + ans['answer_order'])}. {ans['answer_text']}",
                    value=is_checked,
                    key=f"ans_{answer_id}",
                    disabled=is_disabled
                ):
                    if answer_id not in st.session_state.selected_answers:
                        st.session_state.selected_answers.append(answer_id)
                        st.rerun()
                else:
                    if answer_id in st.session_state.selected_answers:
                        st.session_state.selected_answers.remove(answer_id)
                        st.rerun()

            st.divider()

            # Submit button
            if st.button("Submit Answers", type="primary", use_container_width=True):
                # Double-check session is still active before submission
                session_check = get_session_by_code(st.session_state.class_code)
                if not session_check or session_check.get('status') != 'active':
                    st.error("⚠️ The session has been closed. Your answer cannot be submitted.")
                    st.rerun()
                    return

                selected = st.session_state.selected_answers
                if len(selected) != correct_count:
                    st.error(f"Please select exactly {correct_count} answer{'s' if correct_count != 1 else ''}!")
                else:
                    # Submit each selected answer with error handling
                    time_taken = int(time.time() - st.session_state.start_time)
                    all_success = True
                    error_msg = None

                    for answer_id in selected:
                        success, error = submit_answer(
                            st.session_state.student_id,
                            st.session_state.session_id,
                            question['question_id'],
                            answer_id,
                            time_taken
                        )
                        if not success:
                            all_success = False
                            error_msg = error
                            break

                    if all_success:
                        st.session_state.answered = True
                        st.session_state.selected_answers = []  # Clear selections
                        st.rerun()
                    else:
                        st.error(f"Failed to submit: {error_msg}")

        else:
            # Single selection
            with st.form("single_answer_form"):
                options = {f"{chr(65 + ans['answer_order'])}. {ans['answer_text']}": ans['answer_id']
                          for ans in answers}

                selected = st.radio(
                    "Choose your answer:",
                    options=list(options.keys()),
                    index=None
                )

                submit = st.form_submit_button("Submit Answer", type="primary", use_container_width=True)

                if submit:
                    # Double-check session is still active before submission
                    session_check = get_session_by_code(st.session_state.class_code)
                    if not session_check or session_check.get('status') != 'active':
                        st.error("⚠️ The session has been closed. Your answer cannot be submitted.")
                        st.rerun()
                        return

                    if not selected:
                        st.error("Please select an answer!")
                    else:
                        answer_id = options[selected]
                        time_taken = int(time.time() - st.session_state.start_time)

                        success, error = submit_answer(
                            st.session_state.student_id,
                            st.session_state.session_id,
                            question['question_id'],
                            answer_id,
                            time_taken
                        )
                        if success:
                            st.session_state.answered = True
                            st.rerun()
                        else:
                            st.error(f"Failed to submit: {error}")

# Main
def main():
    # Auto-refresh every 5 seconds to check session status
    st_autorefresh(interval=5000, key="student_refresh")

    if not st.session_state.student_joined:
        show_join_page()
    else:
        show_quiz_page()

if __name__ == "__main__":
    main()

"""
Test script for the quiz creation endpoint
Run this to test if the API works correctly
"""

import requests
import json

# Test data
test_quiz = {
    "answers": [
        {"text": "Monday", "order": 0, "is_correct": False},
        {"text": "Tuesday", "order": 1, "is_correct": False},
        {"text": "Wednesday", "order": 2, "is_correct": True},
        {"text": "Thursday", "order": 3, "is_correct": False}
    ]
}

# Query parameters
params = {
    "teacher_id": 1,
    "title": "Test Quiz",
    "question_text": "What day is it?",
    "num_choices": 4,
    "allow_multiple": False,
    "has_correct": True,
    "quiz_mode": "easy",
    "start_with_slide": True,
    "minimize_window": False,
    "auto_close_minutes": 1
}

print("="*60)
print("Testing Quiz Creation Endpoint")
print("="*60)

try:
    # Make request
    print("\n📡 Sending request to API...")
    response = requests.post(
        "http://localhost:8000/api/quiz/create",
        params=params,
        json=test_quiz,
        headers={"Content-Type": "application/json"}
    )
    
    print(f"Status Code: {response.status_code}")
    
    if response.status_code == 200:
        print("✅ SUCCESS!")
        result = response.json()
        print(f"\nQuiz ID: {result['quiz_id']}")
        print(f"Question ID: {result['question_id']}")
        print(f"Message: {result['message']}")
    else:
        print("❌ FAILED!")
        print(f"Error: {response.text}")
        
except requests.exceptions.ConnectionError:
    print("❌ ERROR: Cannot connect to backend!")
    print("Make sure the backend is running:")
    print("  python main.py")
    
except Exception as e:
    print(f"❌ ERROR: {e}")

print("\n" + "="*60)
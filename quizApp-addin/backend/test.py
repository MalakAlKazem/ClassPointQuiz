from database import get_db_connection

conn = get_db_connection()
if conn:
    print("✅ Database connection successful!")
    cur = conn.cursor()
    cur.execute("SELECT COUNT(*) as count FROM teachers")
    result = cur.fetchone()
    print(f"📊 Teachers in database: {result['count']}")
    conn.close()
else:
    print("❌ Database connection failed!")
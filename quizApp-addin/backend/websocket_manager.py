from fastapi import WebSocket
from typing import Dict, List
import json

class ConnectionManager:
    def __init__(self):
        # session_id -> list of websocket connections
        self.active_connections: Dict[int, List[WebSocket]] = {}

    async def connect(self, websocket: WebSocket, session_id: int):
        await websocket.accept()
        if session_id not in self.active_connections:
            self.active_connections[session_id] = []
        self.active_connections[session_id].append(websocket)
        print(f"Client connected to session {session_id}")

    def disconnect(self, websocket: WebSocket, session_id: int):
        if session_id in self.active_connections:
            self.active_connections[session_id].remove(websocket)
            print(f"Client disconnected from session {session_id}")

    async def broadcast_to_session(self, session_id: int, message: dict):
        """Send message to all connected clients in a session"""
        if session_id in self.active_connections:
            dead_connections = []
            for connection in self.active_connections[session_id]:
                try:
                    await connection.send_json(message)
                except Exception as e:
                    print(f"Error sending message: {e}")
                    dead_connections.append(connection)
            
            # Remove dead connections
            for conn in dead_connections:
                self.active_connections[session_id].remove(conn)

manager = ConnectionManager()
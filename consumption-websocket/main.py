import asyncio
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from websocket_manager import WebSocketManager
from consumption_simulation import simulate_consumption
from meross_device_manager import MerossDeviceManager
from fastapi.middleware.cors import CORSMiddleware


app = FastAPI(docs_url="/docs", redoc_url=None, title="Real-time consumption websocket", version="1.0.0")
ws_manager = WebSocketManager()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

device_manager = MerossDeviceManager(
    email="dresyv@gmail.com",
    password="Password123",
)

@app.on_event("startup")
async def startup():
    await device_manager.init()

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await ws_manager.connect(websocket)
    try:
        while True:
            try:
                await device_manager.check_device_status()
                consumption_data = await simulate_consumption()
                await websocket.send_text(consumption_data)
                await asyncio.sleep(1)
            except Exception as e:
                await asyncio.sleep(2)
    except WebSocketDisconnect:
        ws_manager.disconnect(websocket)

@app.post("/device/turn_off")
async def shutdown_device():
    await device_manager.turn_off_device()
    return {"status": "ok", "device": "smart_plug", "state": "off"}

@app.post("/device/turn_on")
async def bring_device_online():    
    await device_manager.turn_on_device()
    return {"status": "ok", "device": "smart_plug", "state": "on"}

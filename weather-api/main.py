from fastapi import FastAPI, HTTPException
import requests

app = FastAPI()

HEADERS = {"User-Agent": "WeatherAPI/1.0 (asyvash.work.it@gmail.com)"}

def get_coordinates(address: str):
    url = f"https://nominatim.openstreetmap.org/search?q={address}&format=json&limit=1"
    response = requests.get(url, headers=HEADERS)

    if response.status_code != 200:
        raise HTTPException(status_code=500, detail=f"Request error: {response.status_code}")

    try:
        data = response.json()
    except requests.exceptions.JSONDecodeError:
        raise HTTPException(status_code=500, detail="Decode error")

    if not data:
        raise HTTPException(status_code=404, detail="Address not found")

    return float(data[0]["lat"]), float(data[0]["lon"])

def get_temperature(lat: float, lon: float):
    url = f"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true"
    response = requests.get(url, headers=HEADERS)

    if response.status_code != 200:
        raise HTTPException(status_code=500, detail="Request error to Open-Meteo")

    try:
        data = response.json()
    except requests.exceptions.JSONDecodeError:
        raise HTTPException(status_code=500, detail="Decode error from Open-Meteo")

    return data.get("current_weather", {}).get("temperature", "N/A")

@app.get("/weather")
def weather(address: str):
    lat, lon = get_coordinates(address)
    temperature = get_temperature(lat, lon)
    return {"address": address, "latitude": lat, "longitude": lon, "temperature": temperature}

# http://localhost:51243/weather?address=Paris

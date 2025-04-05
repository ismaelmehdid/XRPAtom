from fastapi import FastAPI, HTTPException
from datetime import datetime, timedelta
from typing import Optional
import requests

app = FastAPI(docs_url="/docs", redoc_url=None, title="Weather API", version="1.0.0")

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


def get_temperature(lat: float, lon: float, start_date: str, end_date: str):
    url = f"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&daily=weather_code&hourly=temperature_2m&timezone=Europe%2FParis&start_date={start_date}&end_date={end_date}"
    response = requests.get(url, headers=HEADERS)

    if response.status_code != 200:
        raise HTTPException(status_code=500, detail="Request error to Open-Meteo")

    try:
        data = response.json()
    except requests.exceptions.JSONDecodeError:
        raise HTTPException(status_code=500, detail="Decode error from Open-Meteo")

    return data.get("hourly", {})


def get_weather_code(lat: float, lon: float, start_date: str, end_date: str):
    url = f"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&daily=weather_code&timezone=Europe%2FParis&start_date={start_date}&end_date={end_date}"
    response = requests.get(url, headers=HEADERS)

    if response.status_code != 200:
        raise HTTPException(status_code=500, detail="Request error to Open-Meteo")

    try:
        data = response.json()
    except requests.exceptions.JSONDecodeError:
        raise HTTPException(status_code=500, detail="Decode error from Open-Meteo")

    return data.get("daily", {})


@app.get(
    "/weather",
    summary="Get Weather Data",
    description="Fetch weather data for a given address(city)",
)
def weather(address: str, start_date: Optional[str]=datetime.now().strftime("%Y-%m-%d"), end_date: Optional[str]=datetime.now().strftime("%Y-%m-%d")):
    if start_date > end_date:
        raise HTTPException(status_code=400, detail="Start date must be before end date")
    elif start_date < (datetime.now() - timedelta(days=90)).strftime("%Y-%m-%d") or start_date > (datetime.now() + timedelta(days=7)).strftime("%Y-%m-%d"):
        raise HTTPException(status_code=400, detail="Start date must be within 90 days in the past and 7 days in the future")
    elif end_date < (datetime.now() - timedelta(days=90)).strftime("%Y-%m-%d") or end_date > (datetime.now() + timedelta(days=7)).strftime("%Y-%m-%d"):
        raise HTTPException(status_code=400, detail="End date must be within 90 days in the past and 7 days in the future")
    lat, lon = get_coordinates(address)
    temperature = get_temperature(lat, lon, start_date, end_date)
    weather_code = get_weather_code(lat, lon, start_date, end_date)
    return {"address": address, "latitude": lat, "longitude": lon, "temperature": temperature, "weather_code": weather_code}

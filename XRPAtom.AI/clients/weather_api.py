import requests
from typing import Optional
from datetime import datetime

class WeatherAPIClient:
    def __init__(self, base_url: str) -> None:
        self._base_url = base_url

    def get_weather(self, address: str, start_date: Optional[str]=datetime.now().strftime("%Y-%m-%d"), end_date: Optional[str]=datetime.now().strftime("%Y-%m-%d")) -> dict:
        # Simulate an API call to get weather data
        response = requests.get(f"{self._base_url}/weather", params={
            "address": address,
            "start_date": start_date,
            "end_date": end_date
        })
        if response.status_code == 200:
            return response.json()
        else:
            raise Exception(f"Error fetching weather data: {response.status_code}")


if __name__ == "__main__":
    from dotenv import load_dotenv, find_dotenv
    import os

    load_dotenv(find_dotenv())

    # Example usage
    base_url = os.getenv("WEATHER_API_BASE_URL")
    client = WeatherAPIClient(base_url=base_url)
    weather_data = client.get_weather("9820_Alicante/Alacant", "2025-04-01", "2025-04-05")
    print(weather_data)
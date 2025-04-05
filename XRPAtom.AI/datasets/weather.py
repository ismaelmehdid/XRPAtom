import pandas as pd
from pathlib import Path
from typing import Dict
from datetime import datetime, timedelta
from clients.weather_api import WeatherAPIClient

class WeatherDataset:
    """
    A class to represent a power consumption dataset.
    
    Attributes:
        file_path (str): Path to the dataset file.
        data (pd.DataFrame): Data loaded from the dataset file.
    """

    def __init__(self, weather_api_client: WeatherAPIClient) -> None:
        """
        Initializes the PowerConsumptionDataset with the given file path.

        Args:
            data_path (Path): Path to the dataset files
        """
        self._weather_api_client = weather_api_client
    

    def get_weather_data(self, address: str) -> pd.DataFrame:
        """
        Loads the data from the specified file path into a pandas DataFrame.
        Args:
            address (str): The address for which to load the weather data.
        """
        start_date = (datetime.now() - timedelta(days=90)).strftime("%Y-%m-%d")
        end_date = (datetime.now() + timedelta(days=7)).strftime("%Y-%m-%d")
        weather_data = self._weather_api_client.get_weather(address=address, start_date=start_date, end_date=end_date)
        times = weather_data["temperature"]["time"]
        temperatures = weather_data["temperature"]["temperature_2m"]
        weather_code_date = weather_data["weather_code"]["time"]
        weather_code = weather_data["weather_code"]["weather_code"]
        
        weather_df = pd.DataFrame({
            "time": times,
            "temperature_2m": temperatures
        }).astype({
            "time": "datetime64[ns]",
            "temperature_2m": "float"
        })

        weather_code_df = pd.DataFrame({
            "time": weather_code_date,
            "weather_code": weather_code
        }).astype({
            "time": "datetime64[ns]",
            "weather_code": "object"
        })

        weather_df["date"] = pd.to_datetime(weather_df["time"]).dt.date
        weather_code_df["date"] = pd.to_datetime(weather_code_df["time"]).dt.date
        weather_code_df.drop(columns=["time"], inplace=True)
        weather_df = weather_df.merge(weather_code_df, on="date", how="left")
        weather_df = weather_df.drop(columns=["date"])
        return weather_df



if __name__ == "__main__":
    from dotenv import load_dotenv, find_dotenv
    import os

    load_dotenv(find_dotenv())

    # Example usage
    weather_api_client = WeatherAPIClient(base_url=os.getenv("WEATHER_API_BASE_URL"))
    weather_dataset = WeatherDataset(weather_api_client=weather_api_client)
    weather_data = weather_dataset.get_weather_data("Bizkaia")
    
import pandas as pd
import numpy as np
import time
import os
from torch.utils.data import Dataset
import torch
from typing import Dict

from datasets.power_consumption.raw_weather import RawWeatherData
from datasets.power_consumption.raw_power_consumption import RawPowerConsumptionData
from clients.weather_api import WeatherAPIClient
from pathlib import Path
from shared.constants import (
    LARGEST_LOWERBOUND_DATETIME_POWERCONSUMPTION_DS,
    SMALLEST_UPPERRBOUND_DATETIME_POWERCONSUMPTION_DS,
    POWER_CONSUMPTION_PATH,

)


class PowerConsumptionDataset(Dataset):
    """
    PowerConsumptionDataset is used to train TimeSeries models for predicting power consumption.
    This class handles the loading of the dataset and its metadata.
    
    Attributes:
        file_path (str): Path to the dataset file.
        data (pd.DataFrame): Data loaded from the dataset file.
    """

    def __init__(self, config: Dict) -> None:
        """
        Initializes the PowerConsumptionDataset with the given file path.

        Args:
            config (Dict): Configuration dictionary containing dataset paths and other parameters.
                - weather_api_base_url (str): Base URL for the weather API.
        """
        self._config = config
        weather_api_client = WeatherAPIClient(base_url=self._config.get("weather_api_base_url"))
        self._weather_dataset = RawWeatherData(weather_api_client=weather_api_client)

        data_path = Path(self._config.get("data_path"))
        self._power_consumption_dataset = RawPowerConsumptionData(data_path=data_path)
        self._available_users = self._init_available_users(data_path=data_path)
        self._weather_data_cache = {}
    
    
    def __len__(self) -> int:
        """
        Returns the number of samples in the dataset.
        """
        return len(self._available_users)
        

    def __getitem__(self, index: int) -> Dict:
        user = self._available_users[index]
        user_metadata = self._power_consumption_dataset.metadata[self._power_consumption_dataset.metadata["user"] == user]
        province = user_metadata["province"].values[0]
        if province not in self._weather_data_cache:
            #TODO integrate weather data right now no matching time windows are available
            #self._weather_data_cache[province] = self._weather_dataset.get_weather_data(address=province)
            pass
        #weather_data = self._weather_data_cache[province]
        power_consumption = self._power_consumption_dataset.get_power_consumption(user=user)
        power_consumption["timestamp"] = pd.to_datetime(power_consumption["timestamp"])
        power_consumption = power_consumption[
            (power_consumption["timestamp"] >= pd.to_datetime(LARGEST_LOWERBOUND_DATETIME_POWERCONSUMPTION_DS)) &
            (power_consumption["timestamp"] <= pd.to_datetime(SMALLEST_UPPERRBOUND_DATETIME_POWERCONSUMPTION_DS))
        ]
        power_consumption["timestamp"] = pd.to_datetime(power_consumption["timestamp"])
        return {
            "year" : torch.tensor(power_consumption["timestamp"].dt.year.values, dtype=torch.float32),
            "month" : torch.tensor(power_consumption["timestamp"].dt.month.values,dtype=torch.float32),
            "day_in_week" : torch.tensor(power_consumption["timestamp"].dt.dayofweek.values, dtype=torch.float32),
            "hour" : torch.tensor(power_consumption["timestamp"].dt.hour.values, dtype=torch.float32),
            "kWh": torch.tensor(power_consumption["kWh"].values, dtype=torch.float32)
        }


    def _init_available_users(self, data_path: Path) -> Dict:
        """
        Initializes the available users by loading the metadata file.
        Args:
            data_path (str): Path to the dataset files.
        """

        user_files_path = data_path / POWER_CONSUMPTION_PATH
        users_with_data = [Path(file_name).stem for file_name in os.listdir(user_files_path) if file_name.endswith(".csv")]
        metadata = self._power_consumption_dataset.metadata
        users_with_metadata = metadata[metadata["province"].notna()]["user"].unique()
        users = set(users_with_data) & set(users_with_metadata)
        return np.array(list(users))


    def _init_weather_data(self) -> Dict:
        places = self._power_consumption_dataset.metadata["province"].unique()
        weather_data = {}
        for place in places:
            if pd.isna(place):
                continue
            weather_data[place] = self._weather_dataset.get_weather_data(address=place)
            time.sleep(1)
        return weather_data


if __name__ == '__main__':
    from dotenv import load_dotenv, find_dotenv
    load_dotenv(find_dotenv())

    weather_api_base_url = os.getenv("WEATHER_API_BASE_URL")
    data_path = Path('./data')
    config = {
        "data_path": data_path,
        "weather_api_base_url": weather_api_base_url
    }
    dataset = PowerConsumptionDataset(config=config)
    for i in range(len(dataset)):
        data = dataset[i]

        


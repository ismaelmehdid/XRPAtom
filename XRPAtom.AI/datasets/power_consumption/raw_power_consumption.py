import pandas as pd
from pathlib import Path
from shared.constants import (
    POWER_CONSUMPTION_PATH,
    POWER_CONSUMPTION_METADATA_PATH
)

class RawPowerConsumptionData:
    """
    A class to represent a power consumption dataset.
    
    Attributes:
        file_path (str): Path to the dataset file.
        data (pd.DataFrame): Data loaded from the dataset file.
    """

    def __init__(self, data_path: Path) -> None:
        """
        Initializes the PowerConsumptionDataset with the given file path.

        Args:
            data_path (Path): Path to the dataset files
        """
        self._data_path = data_path
        self._metadata = None
    

    @property
    def metadata(self):
        """
        Returns the metadata of the dataset.
        """
        metadata_file_path = self._data_path / POWER_CONSUMPTION_METADATA_PATH
        if self._metadata is None:
            metadata = pd.read_csv(metadata_file_path, sep=',')
            self._metadata = metadata
        return self._metadata


    def get_power_consumption(self, user: str) -> pd.DataFrame:
        """
        Loads the data from the specified file path into a pandas DataFrame.
        Args:
            user (str): The user for whom to load the power consumption data.
        """
        file_path = self._data_path / POWER_CONSUMPTION_PATH / f"{user}.csv"
        if not file_path.exists():
            raise FileNotFoundError(f"User data file not found for user {user}")
        
        return pd.read_csv(file_path, sep=',')


if __name__ == "__main__":
    from dotenv import load_dotenv, find_dotenv
    import os

    load_dotenv(find_dotenv())

    # Example usage
    data_path = Path('./data')
    dataset = RawPowerConsumptionData(data_path=data_path)
    smallest_datetime = None
    largest_datetime = None
    for user in dataset.metadata['user']:
        print(f"User: {user}")
        try:
            power_data = dataset.get_power_consumption(user)
            smallest_datetime_user = list(power_data["timestamp"])[0]
            largest_datetime_user = list(power_data["timestamp"])[-1]
            if smallest_datetime is None or smallest_datetime_user > smallest_datetime  :
                smallest_datetime = smallest_datetime_user
            if largest_datetime is None or largest_datetime_user < largest_datetime:
                largest_datetime = largest_datetime_user
        except FileNotFoundError as e:
            print(e)
            continue
    print(smallest_datetime)
    print(largest_datetime)
    place = dataset.metadata.apply(lambda x: x['province'], axis=1)
    print(len(set(place)))
from dotenv import load_dotenv, find_dotenv
from pathlib import Path
import os
import torch

from training.power_consumption_trainer import PowerConsumptionTrainer
from training.power_consumption_validator import PowerConsumptionValidator
from datasets.power_consumption.power_consumtion_dataset import PowerConsumptionDataset
from models.lstm_model import LSTMModel


load_dotenv(find_dotenv())



if __name__ == "__main__":
    
    # Example usage
    config = {
        "data_path": Path('./data'),
        "weather_api_base_url": os.getenv("WEATHER_API_BASE_URL"),
        "batch_size": 32,
        "lr_hourly_model": 0.01,
        "lr_daily_model": 0.01,
    }

    dataset = PowerConsumptionDataset(config=config)
    models = {
        "hourly": LSTMModel(input_size=5, hidden_size=40, dropout_rate=0.2, output_size=1),
        "daily": LSTMModel(input_size=5, hidden_size=20, dropout_rate=0.3, output_size=1),
    }
    train_size = int(0.8 * len(dataset))
    val_size = len(dataset) - train_size
    train_dataset, val_dataset = torch.utils.data.random_split(dataset, [train_size, val_size])

    validator = PowerConsumptionValidator(models=models, val_dataset=val_dataset, config=config)
    trainer = PowerConsumptionTrainer(models=models, train_dataset=dataset, validator=validator, config=config)

    trainer.train(epochs=10)
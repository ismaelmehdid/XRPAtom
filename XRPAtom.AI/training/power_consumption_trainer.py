from typing import Dict
from tqdm import tqdm
import torch
from torch.utils.data import DataLoader

import torch.nn as nn
import torch.optim as optim

class PowerConsumptionTrainer:
    def __init__(self, models, train_dataset, validator, device='cuda', config: Dict={}):
        self._device = torch.device(device if torch.cuda.is_available() else 'cpu')
        self._hourly_model = models["hourly"].to(self._device)
        self._daily_model = models["daily"].to(self._device)
        self._config = config
        self._batch_size = self._config.get("batch_size", 32)
        lr_hourly_model = self._config.get("lr_hourly_model", 0.001)
        lr_daily_model = self._config.get("lr_daily_model", 0.001)


        self._validator = validator
        self._train_loader = DataLoader(train_dataset, batch_size=self._batch_size, shuffle=True, num_workers=4)
        self._criterion = nn.MSELoss()
        self._optimizer_hourly_model = optim.Adam(self._hourly_model.parameters(), lr=lr_hourly_model)
        self._optimizer_daily_model = optim.Adam(self._daily_model.parameters(), lr=lr_daily_model)

    def train_one_epoch(self):
        self._hourly_model.train()
        self._daily_model.train()
        running_loss_hourly_model = 0.0
        running_loss_daily_model = 0.0

        for i, batch_data in tqdm(enumerate(self._train_loader)):
            try:
                features_labels = torch.stack([
                    batch_data["kWh"],
                    batch_data["year"],
                    batch_data["month"],
                    batch_data["day_in_week"],
                    batch_data["hour"]
                ], dim=1).to(self._device).float()
                feature_labels_week = features_labels.reshape(self._batch_size, 5, 24, -1)
                feature_labels_week = feature_labels_week.mean(dim=2)
                days_count = features_labels.shape[-1] / 24
                week_count = days_count / 7
                for day in range(int(days_count)-10, int(days_count)-3):
                    self._optimizer_hourly_model.zero_grad()
                    features = features_labels[:, :, 0:(day + 2)*24]
                    labels = features_labels[:, 0, (day + 2)*24:(day + 3)*24].squeeze(1)
                    outputs = self._hourly_model(features)[:, -24:, :].squeeze(2)
                    loss = self._criterion(outputs, labels)
                    loss.backward()
                    self._optimizer_hourly_model.step()
                    running_loss_hourly_model += loss.item()
                for week in range(int(week_count)-10, int(week_count)-3):
                    self._optimizer_daily_model.zero_grad()
                    features = feature_labels_week[:, :, :(week + 2)*7]
                    labels = feature_labels_week[:, 0, (week + 2)*7:(week + 3)*7].squeeze(1)
                    outputs = self._daily_model(features)[:, -7:, :].squeeze(2)
                    loss = self._criterion(outputs, labels)
                    loss.backward()
                    self._optimizer_daily_model.step()
                    running_loss_daily_model += loss.item()
                self._optimizer_daily_model.zero_grad()
            except Exception as e:
                print(f"Error in training loop")
                continue           
           
        return {
            "hourly_model_loss": running_loss_hourly_model / (days_count - 3) / len(self._train_loader),
            "daily_model_loss": running_loss_daily_model / (week_count - 3) / len(self._train_loader)
        }

    def train(self, epochs):
        for epoch in range(epochs):
            val_loss = self._validator.validate()
            print(f"Epoch {epoch}/{epochs}, Val Loss: {val_loss}")
            train_loss = self.train_one_epoch()
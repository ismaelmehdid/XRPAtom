from typing import Dict
import torch
from tqdm import tqdm
from torch.utils.data import DataLoader

import torch.nn as nn

class PowerConsumptionValidator:
    def __init__(self, models, val_dataset, device='cuda', config: Dict={}):
        self._device = torch.device(device if torch.cuda.is_available() else 'cpu')
        self._hourly_model = models["hourly"].to(self._device)
        self._daily_model = models["daily"].to(self._device)
        self._config = config
        self._batch_size = self._config.get("batch_size", 32)
        self._weights = torch.tensor([0.6, 0.4]).to(self._device)

        self._val_loader = DataLoader(val_dataset, batch_size=self._batch_size, shuffle=False, num_workers=4)
        self._criterion = nn.MSELoss()

    def validate(self):
        self._daily_model.eval()
        self._hourly_model.eval()
        running_loss = 0.0
        with torch.no_grad():
            for i, batch_data in tqdm(enumerate(self._val_loader)):
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
                    features = feature_labels_week[:, :, :int(week_count-1)*7]
                    week_outputs = self._daily_model(features)[:, -7:, :].squeeze(2)
                    for day in range(int(days_count)-7, int(days_count)-3):
                        features = features_labels[:, :, 0:(day + 2)*24]
                        labels = features_labels[:, 0, (day + 2)*24:(day + 3)*24].squeeze(1)
                        outputs = self._hourly_model(features)[:, -24:, :].squeeze(2)
                        week_output_day = week_outputs[:, day - int(week_count-1)*7 ].unsqueeze(1).repeat(1, 24)
                        outputs = torch.sum(torch.concat([outputs.unsqueeze(2), week_output_day.unsqueeze(2)], dim=2) * self._weights, dim=2)
                        loss = self._criterion(outputs, labels)
                        running_loss += loss.item()
                except Exception as e:
                    print(f"Validation error")
                    continue
        return running_loss / len(self._val_loader)
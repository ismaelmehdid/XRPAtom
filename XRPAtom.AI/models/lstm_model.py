import torch
import torch.nn as nn
import torch.optim as optim

class LSTMModel(nn.Module):
    def __init__(self, input_size, hidden_size, dropout_rate, output_size):
        super(LSTMModel, self).__init__()
        self.lstm = nn.LSTM(input_size, hidden_size, batch_first=True)
        self.dropout = nn.Dropout(dropout_rate)
        self.fc = nn.Linear(hidden_size, output_size)

    def forward(self, x):
        x = x.permute(0, 2, 1)
        _, (hn, _) = self.lstm(x)  # Only take the hidden state from the last time step
        x = self.dropout(hn[-1])  # hn[-1] is the last layer's hidden state
        x = self.fc(x)
        return x

    

class PowerConsumptionPreprocessor:
    def __init__(self, data):
        self.data = data

    def preprocess(self):
        # Example preprocessing steps
        self.data['Date'] = pd.to_datetime(self.data['Date'])
        self.data.set_index('Date', inplace=True)
        self.data = self.data.resample('H').mean()
        return self.data
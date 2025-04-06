import random
import logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

meross_devices_consumption = {
    "smart_plug": 1500,  # 1.5 kWh
}

enabled_devices = {
    "smart_plug": False,
}

async def simulate_consumption():
    base_consumption = random.uniform(3100, 3400)
    for device, is_enabled in enabled_devices.items():
        if not is_enabled:
            base_consumption -= meross_devices_consumption[device]

    return f'{{"consumption": {base_consumption:.2f}}}'

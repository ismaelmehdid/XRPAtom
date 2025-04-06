from meross_iot.http_api import MerossHttpClient
from meross_iot.manager import MerossManager
from consumption_simulation import enabled_devices
from meross_iot.model.enums import OnlineStatus
from meross_iot.model.exception import CommandTimeoutError
import asyncio
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class MerossDeviceManager:
    def __init__(self, email: str, password: str, api_base_url: str = "https://iotx-eu.meross.com"):
        self.email = email
        self.password = password
        self.api_base_url = api_base_url
        self.manager = None
        self.devices = []

    async def init(self):
        http_api_client = await MerossHttpClient.async_from_user_password(
            api_base_url=self.api_base_url, 
            email=self.email, 
            password=self.password
        )

        self.manager = MerossManager(http_client=http_api_client)
        await self.manager.async_init()
        await self.manager.async_device_discovery()
        self.devices = self.manager.find_devices()
        self.device = self.get_device_by_name("smart_plug")
        if self.device:
            print(f"Device found: {self.device.name}")
        else:
            print("No device found with name smart_plug")

    def get_device_by_name(self, name: str):
        for device in self.devices:
            if name.lower() in device.name.lower():
                return device
        return None

    async def turn_on_device(self, channel: int = 0):
        if self.device:
            await self.device.async_turn_on(channel=channel)

    async def turn_off_device(self, channel: int = 0):
        if self.device:
            await self.device.async_turn_off(channel=channel)

    async def refresh_devices(self):
        if self.manager:
            await self.manager.async_device_discovery()
            self.devices = self.manager.find_devices()
            self.device = self.get_device_by_name("smart_plug")

    async def check_device_status(self):
        if self.device:
            try:
                if self.device.online_status == OnlineStatus.ONLINE:
                    if self.device.is_on(channel=0):
                        enabled_devices["smart_plug"] = True
                    else:
                        enabled_devices["smart_plug"] = False
                else:
                    enabled_devices["smart_plug"] = False
            except Exception as e:
                enabled_devices["smart_plug"] = False
                logger.error(f"Unexpected error checking device status: {e}")
        else:
            enabled_devices["smart_plug"] = False
            logger.warning("Device not initialized or found.")

    async def check_device_presence(self, interval: int = 5):
        while True:
            await self.refresh_devices()
            if self.device:
                try:
                    await self.device.async_update()
                    if self.device.is_on(channel=0):
                        enabled_devices["smart_plug"] = True
                        logger.info(f"{self.device.name} is reachable and ON.")
                    else:
                        enabled_devices["smart_plug"] = False
                        logger.info(f"{self.device.name} is reachable but OFF.")
                except CommandTimeoutError as e:
                    logger.warning(f"{self.device.name} is unreachable (timeout).")
                    enabled_devices["smart_plug"] = False
                    self.device = None
                except Exception as e:
                    logger.error(f"Unexpected error checking device presence: {e}")
                    enabled_devices["smart_plug"] = False
                    self.device = None
            else:
                enabled_devices["smart_plug"] = False
                logger.warning("Device not initialized or found.")
                self.device = None
            await asyncio.sleep(interval)

    async def close(self):
        if self.manager:
            self.manager.close()
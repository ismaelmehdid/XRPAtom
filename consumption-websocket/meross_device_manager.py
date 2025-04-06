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
    queue: asyncio.Queue
    
    def __init__(self, email: str, password: str, api_base_url: str = "https://iotx-eu.meross.com"):
        self.email = email
        self.password = password
        self.api_base_url = api_base_url
        self.manager = None
        self.devices = []
        self.offline = False
        self.queue = asyncio.Queue()

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
        asyncio.create_task(self.check_device_presence())
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

    async def check_device_status(self):
        try:
            if not self.queue.empty():
                status = await self.queue.get()
                if status == "online":
                    enabled_devices["smart_plug"] = True
                    self.offline = False
                    logger.info(f"Device {self.device.name} is online.")
                elif status == "offline":
                    enabled_devices["smart_plug"] = False
                    self.offline = True
                    logger.warning(f"Device {self.device.name} is offline.")
                else:
                    logger.error(f"Unexpected status: {status}")
        except asyncio.QueueEmpty:
            logger.debug("Queue is empty, no status update available.")
            return
        if self.offline == True:
            return
        if self.device:
            try:
                if self.device.is_on(channel=0):
                    enabled_devices["smart_plug"] = True
                else:
                    enabled_devices["smart_plug"] = False
            except Exception as e:
                enabled_devices["smart_plug"] = False
                logger.error(f"Unexpected error checking device status: {e}")
        else:
            enabled_devices["smart_plug"] = False

    async def check_device_presence(self, interval: int = 2):
        while True:
            if self.device:
                try:
                    await self.device.async_update()
                    await self.queue.put("online")
                except CommandTimeoutError as e:
                    logger.warning(f"{self.device.name} is unreachable (timeout).")
                    await self.queue.put("offline")
                except Exception as e:
                    logger.error(f"Unexpected error checking device presence: {e}")
                    await self.queue.put("offline")
            await asyncio.sleep(interval)

    async def close(self):
        if self.manager:
            self.manager.close()
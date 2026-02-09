"""DSC Powerseries Neo integration via TLink relay."""

from __future__ import annotations

import asyncio
import json
import logging

from homeassistant.config_entries import ConfigEntry
from homeassistant.const import Platform
from homeassistant.core import HomeAssistant, callback
from homeassistant.helpers.dispatcher import async_dispatcher_send

from .const import CONF_HOST, CONF_RELAY_PORT, DOMAIN

_LOGGER = logging.getLogger(__name__)

PLATFORMS = [Platform.BINARY_SENSOR, Platform.ALARM_CONTROL_PANEL]


async def async_setup_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Set up DSC Neo from a config entry."""
    coordinator = DscNeoCoordinator(
        hass, entry.data[CONF_HOST], entry.data[CONF_RELAY_PORT]
    )
    hass.data.setdefault(DOMAIN, {})[entry.entry_id] = coordinator

    await hass.config_entries.async_forward_entry_setups(entry, PLATFORMS)

    entry.async_on_unload(coordinator.stop)
    coordinator.start()

    return True


async def async_unload_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Unload a config entry."""
    coordinator = hass.data[DOMAIN].pop(entry.entry_id)
    await coordinator.stop()
    return await hass.config_entries.async_unload_platforms(entry, PLATFORMS)


class DscNeoCoordinator:
    """Manages the TCP connection to the C# relay and dispatches updates."""

    def __init__(self, hass: HomeAssistant, host: str, port: int) -> None:
        """Initialize the coordinator."""
        self.hass = hass
        self.host = host
        self.port = port
        self._task: asyncio.Task | None = None
        self._running = False
        self.zones: dict[int, bool] = {}
        self.partitions: dict[int, dict] = {}
        self.connected = False

    def start(self) -> None:
        """Start the TCP connection loop."""
        self._running = True
        self._task = self.hass.async_create_task(self._connect_loop())

    async def stop(self) -> None:
        """Stop the TCP connection loop."""
        self._running = False
        if self._task:
            self._task.cancel()
            try:
                await self._task
            except asyncio.CancelledError:
                pass

    async def _connect_loop(self) -> None:
        """Maintain persistent connection to relay, reconnecting on failure."""
        while self._running:
            try:
                reader, writer = await asyncio.wait_for(
                    asyncio.open_connection(self.host, self.port), timeout=10
                )
                self.connected = True
                _LOGGER.info(
                    "Connected to DSC Neo relay at %s:%s", self.host, self.port
                )
                try:
                    while self._running:
                        line = await asyncio.wait_for(
                            reader.readline(), timeout=300
                        )
                        if not line:
                            _LOGGER.warning("Relay connection closed")
                            break
                        await self._process_line(line.decode().strip())
                except asyncio.TimeoutError:
                    _LOGGER.warning("No data from relay for 5 minutes")
                except asyncio.CancelledError:
                    raise
                except Exception:
                    _LOGGER.exception("Error reading from relay")
                finally:
                    self.connected = False
                    writer.close()
                    try:
                        await writer.wait_closed()
                    except Exception:
                        pass
            except asyncio.CancelledError:
                raise
            except (OSError, asyncio.TimeoutError):
                _LOGGER.warning(
                    "Cannot connect to relay at %s:%s, retrying in 10s",
                    self.host,
                    self.port,
                )

            if self._running:
                await asyncio.sleep(10)

    async def _process_line(self, line: str) -> None:
        """Parse a JSON line and dispatch updates to entities."""
        if not line:
            return
        try:
            data = json.loads(line)
        except json.JSONDecodeError:
            _LOGGER.debug("Invalid JSON from relay: %s", line)
            return

        msg_type = data.get("type")

        if msg_type == "zone_status":
            zone = data["zone"]
            is_open = data["open"]
            is_new = zone not in self.zones
            self.zones[zone] = is_open
            if is_new:
                async_dispatcher_send(self.hass, f"{DOMAIN}_new_zone", zone)
            else:
                async_dispatcher_send(
                    self.hass, f"{DOMAIN}_zone_update", zone
                )

        elif msg_type == "arming":
            partition = data["partition"]
            is_new = partition not in self.partitions
            self.partitions.setdefault(partition, {})["state"] = data["state"]
            if is_new:
                self.partitions[partition].setdefault("ready", True)
                async_dispatcher_send(
                    self.hass, f"{DOMAIN}_new_partition", partition
                )
            else:
                async_dispatcher_send(
                    self.hass, f"{DOMAIN}_partition_update", partition
                )

        elif msg_type == "partition_ready":
            partition = data["partition"]
            is_new = partition not in self.partitions
            self.partitions.setdefault(partition, {})["ready"] = data["ready"]
            if is_new:
                self.partitions[partition].setdefault("state", "disarmed")
                async_dispatcher_send(
                    self.hass, f"{DOMAIN}_new_partition", partition
                )
            else:
                async_dispatcher_send(
                    self.hass, f"{DOMAIN}_partition_update", partition
                )

        elif msg_type == "exit_delay":
            partition = data["partition"]
            self.partitions.setdefault(partition, {})["state"] = "arming"
            self.partitions[partition]["exit_delay_seconds"] = data["seconds"]
            async_dispatcher_send(
                self.hass, f"{DOMAIN}_partition_update", partition
            )

        elif msg_type == "entry_delay":
            partition = data["partition"]
            self.partitions.setdefault(partition, {})["state"] = "pending"
            self.partitions[partition]["entry_delay_seconds"] = data["seconds"]
            async_dispatcher_send(
                self.hass, f"{DOMAIN}_partition_update", partition
            )

        elif msg_type == "heartbeat":
            pass

"""DSC Powerseries Neo integration via TLink relay."""

from __future__ import annotations

import asyncio
import hashlib
import json
import logging
import os
import struct

from cryptography.hazmat.primitives.ciphers.aead import AESGCM

from homeassistant.config_entries import ConfigEntry
from homeassistant.const import Platform
from homeassistant.core import HomeAssistant, callback
from homeassistant.helpers.dispatcher import async_dispatcher_send

from .const import CONF_HOST, CONF_RELAY_PORT, CONF_RELAY_SECRET, DOMAIN

_LOGGER = logging.getLogger(__name__)

PLATFORMS = [Platform.BINARY_SENSOR, Platform.ALARM_CONTROL_PANEL]

PBKDF2_SALT = b"DSC-TLink-Relay-v1"
PBKDF2_ITERATIONS = 100_000
NONCE_SIZE = 12


def _derive_key(secret: str) -> bytes:
    """Derive a 256-bit AES key from a passphrase using PBKDF2-SHA256."""
    return hashlib.pbkdf2_hmac(
        "sha256", secret.encode(), PBKDF2_SALT, PBKDF2_ITERATIONS, dklen=32
    )


def _encrypt(key: bytes, plaintext: bytes) -> bytes:
    """Encrypt plaintext with AES-256-GCM, returning nonce + ciphertext + tag."""
    nonce = os.urandom(NONCE_SIZE)
    aesgcm = AESGCM(key)
    ciphertext_and_tag = aesgcm.encrypt(nonce, plaintext, None)
    return nonce + ciphertext_and_tag


def _decrypt(key: bytes, data: bytes) -> bytes:
    """Decrypt AES-256-GCM payload (nonce + ciphertext + tag)."""
    nonce = data[:NONCE_SIZE]
    ciphertext_and_tag = data[NONCE_SIZE:]
    aesgcm = AESGCM(key)
    return aesgcm.decrypt(nonce, ciphertext_and_tag, None)


async def async_setup_entry(hass: HomeAssistant, entry: ConfigEntry) -> bool:
    """Set up DSC Neo from a config entry."""
    coordinator = DscNeoCoordinator(
        hass,
        entry.data[CONF_HOST],
        entry.data[CONF_RELAY_PORT],
        entry.data[CONF_RELAY_SECRET],
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

    def __init__(
        self, hass: HomeAssistant, host: str, port: int, secret: str
    ) -> None:
        """Initialize the coordinator."""
        self.hass = hass
        self.host = host
        self.port = port
        self._key = _derive_key(secret)
        self._task: asyncio.Task | None = None
        self._running = False
        self._writer: asyncio.StreamWriter | None = None
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

    async def _read_message(self, reader: asyncio.StreamReader) -> bytes:
        """Read a length-prefixed encrypted message and decrypt it."""
        length_bytes = await asyncio.wait_for(
            reader.readexactly(4), timeout=300
        )
        length = struct.unpack(">I", length_bytes)[0]
        payload = await asyncio.wait_for(
            reader.readexactly(length), timeout=30
        )
        return _decrypt(self._key, payload)

    async def _connect_loop(self) -> None:
        """Maintain persistent connection to relay, reconnecting on failure."""
        while self._running:
            try:
                reader, writer = await asyncio.wait_for(
                    asyncio.open_connection(self.host, self.port), timeout=10
                )
                self._writer = writer
                self.connected = True
                _LOGGER.info(
                    "Connected to DSC Neo relay at %s:%s", self.host, self.port
                )
                async_dispatcher_send(
                    self.hass, f"{DOMAIN}_connection_update"
                )
                # Ensure default partition exists so alarm panel entity
                # is available immediately, even before the first panel event.
                if 1 not in self.partitions:
                    self.partitions[1] = {"state": "disarmed", "ready": False}
                    async_dispatcher_send(
                        self.hass, f"{DOMAIN}_new_partition", 1
                    )
                try:
                    while self._running:
                        plaintext = await self._read_message(reader)
                        line = plaintext.decode().strip()
                        if not line:
                            continue
                        await self._process_line(line)
                except asyncio.IncompleteReadError:
                    _LOGGER.warning("Relay connection closed")
                except asyncio.TimeoutError:
                    _LOGGER.warning("No data from relay for 5 minutes")
                except asyncio.CancelledError:
                    raise
                except Exception:
                    _LOGGER.exception("Error reading from relay")
                finally:
                    self.connected = False
                    self._writer = None
                    async_dispatcher_send(
                        self.hass, f"{DOMAIN}_connection_update"
                    )
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

    async def send_command(self, command: dict) -> None:
        """Send an encrypted JSON command to the relay server."""
        writer = self._writer
        if writer is None or writer.is_closing():
            _LOGGER.warning("Cannot send command — not connected to relay")
            return
        try:
            plaintext = json.dumps(command).encode()
            payload = _encrypt(self._key, plaintext)
            length_prefix = struct.pack(">I", len(payload))
            writer.write(length_prefix + payload)
            await writer.drain()
            _LOGGER.debug("Sent command to relay: %s", command)
        except (OSError, ConnectionError) as exc:
            _LOGGER.error("Failed to send command to relay: %s", exc)

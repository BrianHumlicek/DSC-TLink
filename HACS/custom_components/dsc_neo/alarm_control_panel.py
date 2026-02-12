"""Alarm control panel platform for DSC Powerseries Neo."""

from __future__ import annotations

import logging

from homeassistant.components.alarm_control_panel import (
    AlarmControlPanelEntity,
    AlarmControlPanelEntityFeature,
    AlarmControlPanelState,
    CodeFormat,
)
from homeassistant.config_entries import ConfigEntry
from homeassistant.core import HomeAssistant, callback
from homeassistant.helpers.dispatcher import async_dispatcher_connect
from homeassistant.helpers.entity import DeviceInfo
from homeassistant.helpers.entity_platform import AddEntitiesCallback

from .const import DOMAIN

_LOGGER = logging.getLogger(__name__)

STATE_MAP = {
    "disarmed": AlarmControlPanelState.DISARMED,
    "armed_away": AlarmControlPanelState.ARMED_AWAY,
    "armed_home": AlarmControlPanelState.ARMED_HOME,
    "arming": AlarmControlPanelState.ARMING,
    "pending": AlarmControlPanelState.PENDING,
}


async def async_setup_entry(
    hass: HomeAssistant,
    entry: ConfigEntry,
    async_add_entities: AddEntitiesCallback,
) -> None:
    """Set up DSC Neo alarm control panel."""
    coordinator = hass.data[DOMAIN][entry.entry_id]
    added_partitions: set[int] = set()

    @callback
    def async_add_partition(partition: int) -> None:
        """Add a new partition alarm panel when discovered."""
        if partition not in added_partitions:
            added_partitions.add(partition)
            async_add_entities(
                [DscNeoAlarmPanel(coordinator, partition, entry.entry_id)]
            )

    # Add any partitions already discovered
    for partition in list(coordinator.partitions):
        async_add_partition(partition)

    # Listen for new partition discoveries
    entry.async_on_unload(
        async_dispatcher_connect(
            hass, f"{DOMAIN}_new_partition", async_add_partition
        )
    )


class DscNeoAlarmPanel(AlarmControlPanelEntity):
    """Represents a DSC Neo partition as an alarm control panel."""

    _attr_has_entity_name = True
    _attr_supported_features = (
        AlarmControlPanelEntityFeature.ARM_HOME
        | AlarmControlPanelEntityFeature.ARM_AWAY
        | AlarmControlPanelEntityFeature.ARM_NIGHT
    )
    _attr_code_arm_required = True
    _attr_code_format = CodeFormat.NUMBER

    def __init__(
        self, coordinator, partition: int, entry_id: str
    ) -> None:
        """Initialize the alarm panel."""
        self._coordinator = coordinator
        self._partition = partition
        self._attr_unique_id = f"{entry_id}_partition_{partition}"
        self._attr_name = f"Partition {partition}"
        self._attr_device_info = DeviceInfo(
            identifiers={(DOMAIN, entry_id)},
            name="DSC Neo Alarm Panel",
            manufacturer="DSC",
            model="Powerseries Neo",
        )

    @property
    def state(self) -> str:
        """Return the state of the partition."""
        pdata = self._coordinator.partitions.get(self._partition, {})
        state = pdata.get("state", "disarmed")
        return STATE_MAP.get(state, AlarmControlPanelState.DISARMED)

    @property
    def available(self) -> bool:
        """Return true if the relay connection is active."""
        return self._coordinator.connected

    @property
    def extra_state_attributes(self) -> dict:
        """Return additional state attributes."""
        pdata = self._coordinator.partitions.get(self._partition, {})
        attrs = {"ready": pdata.get("ready", False)}
        if "exit_delay_seconds" in pdata:
            attrs["exit_delay_seconds"] = pdata["exit_delay_seconds"]
        if "entry_delay_seconds" in pdata:
            attrs["entry_delay_seconds"] = pdata["entry_delay_seconds"]
        return attrs

    async def async_added_to_hass(self) -> None:
        """Register update dispatcher."""
        self.async_on_remove(
            async_dispatcher_connect(
                self.hass,
                f"{DOMAIN}_partition_update",
                self._handle_update,
            )
        )

    async def async_alarm_arm_away(self, code: str | None = None) -> None:
        """Send arm away command."""
        if not code:
            _LOGGER.warning("Arm away requires an access code")
            return
        await self._coordinator.send_command(
            {"type": "arm_away", "partition": self._partition, "code": code}
        )

    async def async_alarm_arm_home(self, code: str | None = None) -> None:
        """Send arm home (stay) command."""
        if not code:
            _LOGGER.warning("Arm home requires an access code")
            return
        await self._coordinator.send_command(
            {"type": "arm_home", "partition": self._partition, "code": code}
        )

    async def async_alarm_arm_night(self, code: str | None = None) -> None:
        """Send arm night (zero entry delay) command."""
        if not code:
            _LOGGER.warning("Arm night requires an access code")
            return
        await self._coordinator.send_command(
            {"type": "arm_night", "partition": self._partition, "code": code}
        )

    async def async_alarm_disarm(self, code: str | None = None) -> None:
        """Send disarm command with access code."""
        if not code:
            _LOGGER.warning("Disarm requires an access code")
            return
        await self._coordinator.send_command(
            {"type": "disarm", "partition": self._partition, "code": code}
        )

    @callback
    def _handle_update(self, partition: int) -> None:
        """Handle a partition state update."""
        if partition == self._partition:
            self.async_write_ha_state()

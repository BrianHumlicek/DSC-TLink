"""Binary sensor platform for DSC Powerseries Neo zones."""

from __future__ import annotations

import logging

from homeassistant.components.binary_sensor import (
    BinarySensorDeviceClass,
    BinarySensorEntity,
)
from homeassistant.config_entries import ConfigEntry
from homeassistant.core import HomeAssistant, callback
from homeassistant.helpers.dispatcher import async_dispatcher_connect
from homeassistant.helpers.entity import DeviceInfo
from homeassistant.helpers.entity_platform import AddEntitiesCallback

from .const import DOMAIN

_LOGGER = logging.getLogger(__name__)


async def async_setup_entry(
    hass: HomeAssistant,
    entry: ConfigEntry,
    async_add_entities: AddEntitiesCallback,
) -> None:
    """Set up DSC Neo zone binary sensors."""
    coordinator = hass.data[DOMAIN][entry.entry_id]
    added_zones: set[int] = set()

    @callback
    def async_add_zone(zone: int) -> None:
        """Add a new zone binary sensor when discovered."""
        if zone not in added_zones:
            added_zones.add(zone)
            async_add_entities(
                [DscNeoZoneSensor(coordinator, zone, entry.entry_id)]
            )

    # Add any zones already discovered before platform setup
    for zone in list(coordinator.zones):
        async_add_zone(zone)

    # Listen for new zone discoveries
    entry.async_on_unload(
        async_dispatcher_connect(
            hass, f"{DOMAIN}_new_zone", async_add_zone
        )
    )


class DscNeoZoneSensor(BinarySensorEntity):
    """Represents a DSC Neo zone as a binary sensor."""

    _attr_has_entity_name = True
    _attr_device_class = BinarySensorDeviceClass.DOOR

    def __init__(
        self, coordinator, zone: int, entry_id: str
    ) -> None:
        """Initialize the zone sensor."""
        self._coordinator = coordinator
        self._zone = zone
        self._attr_unique_id = f"{entry_id}_zone_{zone}"
        self._attr_name = f"Zone {zone}"
        self._attr_device_info = DeviceInfo(
            identifiers={(DOMAIN, entry_id)},
            name="DSC Neo Alarm Panel",
            manufacturer="DSC",
            model="Powerseries Neo",
        )

    @property
    def is_on(self) -> bool:
        """Return true if the zone is open/faulted."""
        return self._coordinator.zones.get(self._zone, False)

    @property
    def available(self) -> bool:
        """Return true if the relay connection is active."""
        return self._coordinator.connected

    async def async_added_to_hass(self) -> None:
        """Register update dispatcher."""
        self.async_on_remove(
            async_dispatcher_connect(
                self.hass,
                f"{DOMAIN}_zone_update",
                self._handle_update,
            )
        )

    @callback
    def _handle_update(self, zone: int) -> None:
        """Handle a zone status update."""
        if zone == self._zone:
            self.async_write_ha_state()

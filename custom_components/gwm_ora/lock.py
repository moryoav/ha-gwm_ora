"""Lock platform for GWM ORA."""

from __future__ import annotations

from homeassistant.components.lock import LockEntity
from homeassistant.core import HomeAssistant
from homeassistant.helpers.entity_platform import AddEntitiesCallback

from . import GwmOraConfigEntry
from .entity import GwmOraEntity, vehicle_value


async def async_setup_entry(
    hass: HomeAssistant,
    entry: GwmOraConfigEntry,
    async_add_entities: AddEntitiesCallback,
) -> None:
    """Set up GWM ORA locks."""
    coordinator = entry.runtime_data.coordinator
    async_add_entities(
        GwmOraDoorLock(entry.runtime_data.api, coordinator, vehicle["vin"])
        for vehicle in coordinator.vehicles
    )


class GwmOraDoorLock(GwmOraEntity, LockEntity):
    """GWM ORA door lock."""

    _attr_translation_key = "door_lock"
    _attr_icon = "mdi:car-door-lock"

    def __init__(self, api, coordinator, vin: str) -> None:
        super().__init__(coordinator, vin)
        self._api = api
        self._attr_unique_id = f"{vin}_door_lock"

    @property
    def available(self) -> bool:
        """Return whether lock commands are available."""
        return super().available and self.remote_commands_available

    @property
    def is_locked(self) -> bool | None:
        """Return whether the vehicle is locked."""
        return vehicle_value(self.vehicle, "locked")

    async def async_lock(self, **kwargs) -> None:
        """Lock the vehicle."""
        await self._api.async_lock(self.vin, "lock")
        await self.coordinator.async_request_refresh()

    async def async_unlock(self, **kwargs) -> None:
        """Unlock the vehicle."""
        await self._api.async_lock(self.vin, "unlock")
        await self.coordinator.async_request_refresh()

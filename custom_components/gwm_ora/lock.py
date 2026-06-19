"""Lock platform for GWM ORA."""

from __future__ import annotations

from homeassistant.components.lock import LockEntity
from homeassistant.core import HomeAssistant
from homeassistant.helpers.entity_platform import AddEntitiesCallback

from . import GwmOraConfigEntry
from .entity import GwmOraEntity, async_call_addon_api, setup_vehicle_entities, vehicle_value

PARALLEL_UPDATES = 0


async def async_setup_entry(
    hass: HomeAssistant,
    entry: GwmOraConfigEntry,
    async_add_entities: AddEntitiesCallback,
) -> None:
    """Set up GWM ORA locks."""
    setup_vehicle_entities(
        entry,
        async_add_entities,
        lambda vehicle: (
            GwmOraDoorLock(entry.runtime_data.api, entry.runtime_data.coordinator, vehicle["vin"]),
        ),
    )


class GwmOraDoorLock(GwmOraEntity, LockEntity):
    """GWM ORA door lock."""

    _attr_translation_key = "door_lock"

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
        await async_call_addon_api(self._api.async_lock(self.vin, "lock"))
        await self.coordinator.async_request_refresh()

    async def async_unlock(self, **kwargs) -> None:
        """Unlock the vehicle."""
        await async_call_addon_api(self._api.async_lock(self.vin, "unlock"))
        await self.coordinator.async_request_refresh()

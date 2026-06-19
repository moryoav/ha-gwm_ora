"""Button platform for GWM ORA."""

from __future__ import annotations

from homeassistant.components.button import ButtonEntity
from homeassistant.core import HomeAssistant
from homeassistant.helpers.entity_platform import AddEntitiesCallback

from . import GwmOraConfigEntry
from .entity import GwmOraEntity


async def async_setup_entry(
    hass: HomeAssistant,
    entry: GwmOraConfigEntry,
    async_add_entities: AddEntitiesCallback,
) -> None:
    """Set up GWM ORA buttons."""
    coordinator = entry.runtime_data.coordinator
    async_add_entities(
        GwmOraCloseWindowsButton(entry.runtime_data.api, coordinator, vehicle["vin"])
        for vehicle in coordinator.vehicles
    )


class GwmOraCloseWindowsButton(GwmOraEntity, ButtonEntity):
    """Button that closes all windows."""

    _attr_translation_key = "close_windows"
    _attr_icon = "mdi:window-closed-variant"

    def __init__(self, api, coordinator, vin: str) -> None:
        super().__init__(coordinator, vin)
        self._api = api
        self._attr_unique_id = f"{vin}_close_windows"

    @property
    def available(self) -> bool:
        """Return whether close-window commands are available."""
        return super().available and self.remote_commands_available

    async def async_press(self) -> None:
        """Close windows."""
        await self._api.async_close_windows(self.vin)
        await self.coordinator.async_request_refresh()

"""Base entities for GWM ORA."""

from __future__ import annotations

from typing import Any

from homeassistant.helpers.device_registry import DeviceInfo
from homeassistant.helpers.update_coordinator import CoordinatorEntity

from .const import DOMAIN
from .coordinator import GwmOraDataUpdateCoordinator


class GwmOraEntity(CoordinatorEntity[GwmOraDataUpdateCoordinator]):
    """Base entity bound to one GWM ORA vehicle."""

    _attr_has_entity_name = True

    def __init__(self, coordinator: GwmOraDataUpdateCoordinator, vin: str) -> None:
        super().__init__(coordinator)
        self.vin = vin

    @property
    def vehicle(self) -> dict[str, Any] | None:
        """Return the current vehicle snapshot."""
        return self.coordinator.vehicle(self.vin)

    @property
    def available(self) -> bool:
        """Return whether the entity is available."""
        return super().available and self.vehicle is not None

    @property
    def device_info(self) -> DeviceInfo:
        """Return device registry info."""
        vehicle = self.vehicle or {}
        return DeviceInfo(
            identifiers={(DOMAIN, self.vin)},
            name=vehicle.get("name") or "GWM ORA",
            manufacturer=vehicle.get("manufacturer") or "GWM",
            model=vehicle.get("model"),
            serial_number=vehicle.get("serial_number"),
        )

    @property
    def remote_commands_available(self) -> bool:
        """Return whether remote commands are available for this vehicle."""
        vehicle = self.vehicle or {}
        capabilities = vehicle.get("capabilities") or {}
        return bool(capabilities.get("remote_commands"))


def vehicle_value(vehicle: dict[str, Any] | None, key: str) -> Any:
    """Return a value from a vehicle snapshot."""
    if vehicle is None:
        return None
    return (vehicle.get("values") or {}).get(key)

"""Data coordinator for GWM ORA."""

from __future__ import annotations

import logging
from datetime import timedelta
from typing import Any

from homeassistant.core import HomeAssistant
from homeassistant.exceptions import ConfigEntryAuthFailed
from homeassistant.helpers.update_coordinator import DataUpdateCoordinator, UpdateFailed

from .api import GwmOraApiAuthError, GwmOraApiClient, GwmOraApiError, GwmOraApiUnavailable
from .const import DOMAIN

_LOGGER = logging.getLogger(__name__)


class GwmOraDataUpdateCoordinator(DataUpdateCoordinator[dict[str, Any]]):
    """Coordinator that polls the add-on's cached vehicle data."""

    def __init__(self, hass: HomeAssistant, api: GwmOraApiClient) -> None:
        super().__init__(
            hass,
            _LOGGER,
            name=DOMAIN,
            update_interval=timedelta(seconds=30),
        )
        self.api = api

    async def _async_update_data(self) -> dict[str, Any]:
        try:
            return await self.api.async_get_vehicles()
        except GwmOraApiAuthError as err:
            raise ConfigEntryAuthFailed("Add-on API token rejected") from err
        except (GwmOraApiUnavailable, GwmOraApiError) as err:
            raise UpdateFailed(str(err)) from err

    @property
    def vehicles(self) -> list[dict[str, Any]]:
        """Return vehicle snapshots."""
        data = self.data or {}
        return list(data.get("vehicles", []))

    def vehicle(self, vin: str) -> dict[str, Any] | None:
        """Return one vehicle snapshot by VIN."""
        return next((vehicle for vehicle in self.vehicles if vehicle.get("vin") == vin), None)

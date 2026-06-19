"""GWM ORA native integration."""

from __future__ import annotations

from dataclasses import dataclass

from homeassistant.config_entries import ConfigEntry
from homeassistant.const import CONF_HOST, CONF_PORT
from homeassistant.core import HomeAssistant
from homeassistant.exceptions import ConfigEntryNotReady
from homeassistant.helpers.aiohttp_client import async_get_clientsession

from .api import GwmOraApiAuthError, GwmOraApiClient, GwmOraApiError, GwmOraApiUnavailable
from .const import CONF_TOKEN, DOMAIN, PLATFORMS
from .coordinator import GwmOraDataUpdateCoordinator


@dataclass(slots=True)
class GwmOraRuntimeData:
    """Runtime data for a GWM ORA config entry."""

    api: GwmOraApiClient
    coordinator: GwmOraDataUpdateCoordinator


GwmOraConfigEntry = ConfigEntry[GwmOraRuntimeData]


async def async_setup_entry(hass: HomeAssistant, entry: GwmOraConfigEntry) -> bool:
    """Set up GWM ORA from a config entry."""
    session = async_get_clientsession(hass)
    api = GwmOraApiClient(
        session,
        entry.data[CONF_HOST],
        entry.data[CONF_PORT],
        entry.data[CONF_TOKEN],
    )
    coordinator = GwmOraDataUpdateCoordinator(hass, api)

    try:
        await coordinator.async_config_entry_first_refresh()
    except GwmOraApiAuthError:
        raise
    except (GwmOraApiUnavailable, GwmOraApiError) as err:
        raise ConfigEntryNotReady(str(err)) from err

    entry.runtime_data = GwmOraRuntimeData(api=api, coordinator=coordinator)
    await hass.config_entries.async_forward_entry_setups(entry, PLATFORMS)
    return True


async def async_unload_entry(hass: HomeAssistant, entry: GwmOraConfigEntry) -> bool:
    """Unload a config entry."""
    return await hass.config_entries.async_unload_platforms(entry, PLATFORMS)

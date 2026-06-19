"""Static coverage checks for the GWM ORA integration entity descriptions."""

import pytest


def test_sensor_description_keys_cover_v1_contract() -> None:
    pytest.importorskip("homeassistant")
    from custom_components.gwm_ora.sensor import SENSORS

    keys = {description.key for description in SENSORS}

    assert {
        "soc",
        "range_km",
        "odometer_km",
        "remaining_charging_time_min",
        "soce",
        "interior_temperature_c",
        "command_status",
    } <= keys


def test_binary_sensor_description_keys_cover_v1_contract() -> None:
    pytest.importorskip("homeassistant")
    from custom_components.gwm_ora.binary_sensor import BINARY_SENSORS

    keys = {description.key for description in BINARY_SENSORS}

    assert {
        "charging_active",
        "charge_plug_connected",
        "ac_active",
        "lock_open",
        "window_front_left_open",
        "window_front_right_open",
        "window_rear_left_open",
        "window_rear_right_open",
    } <= keys

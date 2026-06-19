"""Static coverage checks for the GWM ORA integration entity descriptions."""

import pytest


def test_sensor_description_keys_cover_v1_contract() -> None:
    pytest.importorskip("homeassistant")
    from homeassistant.helpers.entity import EntityCategory

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

    descriptions = {description.key: description for description in SENSORS}
    assert descriptions["acquisition_time"].entity_category is EntityCategory.DIAGNOSTIC
    assert descriptions["update_time"].entity_category is EntityCategory.DIAGNOSTIC
    assert descriptions["command_status"].entity_category is EntityCategory.DIAGNOSTIC
    assert descriptions["acquisition_time"].entity_registry_enabled_default is False
    assert descriptions["update_time"].entity_registry_enabled_default is False
    assert descriptions["command_status"].entity_registry_enabled_default is False


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


def test_platforms_declare_parallel_updates() -> None:
    pytest.importorskip("homeassistant")
    from custom_components.gwm_ora import binary_sensor, button, climate, device_tracker, lock, sensor

    assert sensor.PARALLEL_UPDATES == 0
    assert binary_sensor.PARALLEL_UPDATES == 0
    assert climate.PARALLEL_UPDATES == 0
    assert lock.PARALLEL_UPDATES == 0
    assert button.PARALLEL_UPDATES == 0
    assert device_tracker.PARALLEL_UPDATES == 0

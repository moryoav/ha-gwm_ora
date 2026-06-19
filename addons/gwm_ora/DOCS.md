# GWM ORA Add-on Documentation

## Installation

1. Add this repository to the Home Assistant add-on store.
2. Install the **GWM ORA** add-on.
3. Fill in the add-on configuration.
4. Start the add-on.
5. Install the `custom_components/gwm_ora` integration from this repository.
6. Confirm the discovered **GWM ORA** integration in Home Assistant.

## Configuration

| Option | Required | Description |
| --- | --- | --- |
| `country` | yes | Two-letter GWM account region such as `DE` or `GB`. |
| `username` | yes | GWM account e-mail address. |
| `password` | yes | GWM account password. |
| `security_pin` | no | Vehicle remote control PIN from the official app. |
| `enable_remote_commands` | yes | Enables A/C, lock, unlock, and close-window commands. |
| `poll_interval_seconds` | yes | GWM cloud polling interval from 30 to 3600 seconds. |
| `log_level` | yes | One of `trace`, `debug`, `info`, `warning`, or `error`. |

## Notes

If GWM requires SMS or e-mail verification during login, use the official app to complete account verification first. The add-on is non-interactive and cannot prompt for a verification code.

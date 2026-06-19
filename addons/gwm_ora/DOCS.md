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
| `verification_code` | no | One-time SMS/e-mail verification code requested by GWM for this add-on device. Leave empty unless the add-on log asks for it. |
| `security_pin` | no | Vehicle remote control PIN from the official app. |
| `enable_remote_commands` | yes | Enables A/C, lock, unlock, and close-window commands. |
| `poll_interval_seconds` | yes | GWM cloud polling interval from 30 to 3600 seconds. |
| `log_level` | yes | One of `trace`, `debug`, `info`, `warning`, or `error`. |

## Notes

If GWM requires SMS or e-mail verification during login, the add-on requests a code and reports `verification_required` in health. Enter that code in `verification_code`, save, and restart the add-on. After a successful login, the add-on stores GWM tokens under `/data` and tries to clear `verification_code` from the add-on options.

## Web UI

The **Open Web UI** button uses Home Assistant Ingress and shows add-on health plus the latest cached vehicle summary. Remote controls are exposed by the native Home Assistant integration rather than the add-on web page.

## Security

The add-on does not publish a LAN port, does not use host networking, does not request Docker API access, and does not use `full_access`. The internal API requires a generated bearer token, Ingress pages are restricted to Home Assistant's ingress proxy, and the container ships with a custom AppArmor profile.

This repository publishes stable releases only for now; no canary branch is offered.

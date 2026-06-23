# GWM ORA for Home Assistant
[![HACS][hacs-badge]][hacs-url] [![release][release-badge]][release-url] ![downloads][downloads-badge] [![license][license-badge]][license-url]

![GWM ORA][banner]

Control and monitor your GWM ORA from Home Assistant. The add-on connects to your GWM account, and the Home Assistant integration creates sensors and controls for your car.

## What You Get

- Battery SOC, range, odometer, charging, plug, cabin temperature, tire pressure, tire temperature, lock, window, A/C, and location entities.
- Native Home Assistant controls for A/C, door lock/unlock, and closing windows.
- A remote command status sensor that shows progress while commands are being sent to the car.
- Automatic discovery of the add-on by the integration.
- A small add-on Web UI showing add-on health and the latest cached vehicle summary.

Remote commands can take time. The car may report several pending attempts before a command succeeds, especially for A/C and locking. Watch the **Remote command status** sensor after pressing a command.

## Installation

### 1. Add the Add-on Repository

[![Add the GWM ORA add-on repository to Home Assistant](https://my.home-assistant.io/badges/supervisor_add_addon_repository.svg)](https://my.home-assistant.io/redirect/supervisor_add_addon_repository/?repository_url=https%3A%2F%2Fgithub.com%2Fmoryoav%2Fha-gwm_ora)

Manual path:

1. Go to **Settings** -> **Add-ons**.
2. Open **Repositories** from the menu.
3. Add:

```text
https://github.com/moryoav/ha-gwm_ora
```

### 2. Install and Configure the Add-on

Install **GWM ORA**, then fill in:

```yaml
country: DE
username: owner@example.com
password: your-gwm-password
enable_remote_commands: false
poll_interval_seconds: 60
log_level: info
```

For remote controls, also set:

```yaml
enable_remote_commands: true
security_pin: "123456"
```

If GWM asks for a one-time SMS or e-mail verification code, the add-on log will say so. Enter the code in `verification_code`, save, and restart the add-on.

### 3. Install the Integration

#### HACS

[![Open the GWM ORA HACS repository](https://my.home-assistant.io/badges/hacs_repository.svg)](https://my.home-assistant.io/redirect/hacs_repository/?owner=moryoav&repository=ha-gwm_ora&category=integration)

Manual HACS path:

1. Open HACS.
2. Add a custom repository.
3. Use:

```text
https://github.com/moryoav/ha-gwm_ora
```

4. Select category **Integration**.
5. Install **GWM ORA**.
6. Restart Home Assistant.

### 4. Add the Integration

[![Add the GWM ORA integration](https://my.home-assistant.io/badges/config_flow_start.svg)](https://my.home-assistant.io/redirect/config_flow_start/?domain=gwm_ora)

The integration should discover the running add-on and ask for confirmation. You do not enter your GWM username or password in the integration.

## Entities

- Sensors: SOC, range, odometer, remaining charging time, SOCE, tire pressures, tire temperatures, interior temperature, acquisition/update timestamps, remote command status.
- Binary sensors: charging active, charge plug, A/C active, lock open, windows open, air circulation, front defroster.
- Device tracker: vehicle GPS location when available.
- Climate: A/C mode `off`/`cool`, target temperature, current cabin temperature.
- Lock: lock and unlock vehicle doors.
- Button: close all windows.

Remote command entities are unavailable until remote commands are enabled and a security PIN is configured in the add-on.

## Remote Commands

Remote commands are slower than normal Home Assistant switches because they go through the GWM cloud and then to the car. After you send a command, the **Remote command status** sensor should show messages such as:

```text
A/C: sending command to GWM
A/C: accepted by GWM, waiting for vehicle result
A/C: waiting for vehicle result (3/18)
A/C: completed - Success [0]
```

The integration follows the command while it is running and refreshes vehicle data after a successful command.

## Supported Vehicles

This project is designed for GWM ORA vehicles that use the same GWM cloud behavior as the original `ora2mqtt` project, including ORA 03/Funky Cat style models exposed by the GWM mobile app.

Regional GWM services and vehicle firmware can differ, so some entities may be unavailable on some cars.

> **Regional availability:** This integration currently supports GWM accounts that use the European GWM cloud servers. That includes EU countries, Israel, and possibly other markets linked to the same EU backend. It is not expected to work in Russia, the United States, China, or other regions that use different GWM servers and authentication flows. I cannot add or validate support for those regions without help from users there, so developers or technically minded users in non-EU-backed markets are very welcome to get in touch and help gather the details needed for support.

## Troubleshooting

### Add-on Is Not Discovered

- Confirm the add-on is installed and started.
- Check the add-on log for login or configuration errors.
- Restart the add-on.
- Restart Home Assistant if the integration was installed after the add-on had already started.

### Integration Cannot Connect

- Confirm the add-on is running.
- Remove and re-add the integration if discovery data changed.
- For manual development installs, reconfigure the integration with the current host, port, and token.

### GWM Login Fails

- Verify the same account works in the official GWM app.
- Confirm `country`, `username`, and `password`.
- If the add-on reports `verification_required`, enter the received one-time code in `verification_code`, save, and restart the add-on.

### Remote Commands Are Unavailable

Remote commands require both:

- `enable_remote_commands: true`
- `security_pin` configured in the add-on

After changing either option, restart the add-on and reload the integration.

### Remote Command Status Does Not Change

- Make sure the **Remote command status** sensor is enabled.
- Update to the latest release.
- Restart the add-on and reload the integration.
- Check the add-on log for GWM command errors.

### Entities Are Missing or Unavailable

- Some entities depend on data returned by your vehicle and region.
- Newly discovered vehicles are added automatically after Home Assistant sees them.
- If a value is not returned by GWM, the related entity may be unavailable.

## Example Automations

Notify when the charge plug is connected but charging is not active:

```yaml
alias: ORA plugged in but not charging
triggers:
  - trigger: state
    entity_id: binary_sensor.ora_charge_plug
    to: "on"
conditions:
  - condition: state
    entity_id: binary_sensor.ora_charging_active
    state: "off"
actions:
  - action: notify.mobile_app_phone
    data:
      message: "The ORA is plugged in but not charging."
```

Pre-cool the cabin before a commute:

```yaml
alias: ORA pre-cool before commute
triggers:
  - trigger: time
    at: "07:20:00"
conditions:
  - condition: numeric_state
    entity_id: sensor.ora_soc
    above: 30
actions:
  - action: climate.set_temperature
    target:
      entity_id: climate.ora_a_c_climate
    data:
      temperature: 22
      hvac_mode: cool
```

Test remote command automations manually first and use them only when the vehicle is parked somewhere safe.

## Removal

1. Delete the `GWM ORA` integration entry from Home Assistant.
2. Stop and uninstall the `GWM ORA` add-on.
3. Remove this repository from the add-on store if you no longer need it.
4. Remove the custom integration from HACS or delete `/config/custom_components/gwm_ora`.
5. Restart Home Assistant.

## Privacy And Safety

Your GWM account details and vehicle PIN are configured in the add-on, not the integration. The add-on stores generated tokens in its own add-on data folder.

Remote commands can affect the real vehicle. Use them carefully.

## Disclaimer

This project is unofficial and is not affiliated with or endorsed by Great Wall Motor, GWM, ORA, or Home Assistant. Vehicle cloud APIs and remote command behavior may change without notice.

Use at your own risk. You are responsible for validating behavior, protecting credentials, keeping backups, and deciding whether remote commands are appropriate for your vehicle and environment.

## Special Thanks

Special thanks to [zivillian](https://github.com/zivillian) and the [zivillian/ora2mqtt](https://github.com/zivillian/ora2mqtt) project for blazing the trail. Their work uncovered many of the details behind ORA/GWM connectivity and helped inspire the current development of this integration.

[banner]: https://raw.githubusercontent.com/moryoav/ha-gwm_ora/main/brand/banner.png
[hacs-badge]: https://img.shields.io/badge/HACS-Custom-41BDF5.svg?style=flat-square
[hacs-url]: https://github.com/hacs/integration
[release-badge]: https://img.shields.io/github/v/release/moryoav/ha-gwm_ora?style=flat-square
[release-url]: https://github.com/moryoav/ha-gwm_ora/releases
[downloads-badge]: https://img.shields.io/github/downloads/moryoav/ha-gwm_ora/total?style=flat-square
[license-badge]: https://img.shields.io/github/license/moryoav/ha-gwm_ora?style=flat-square
[license-url]: https://github.com/moryoav/ha-gwm_ora/blob/main/LICENSE

# GWM ORA for Home Assistant
[![HACS][hacs-badge]][hacs-url] [![release][release-badge]][release-url] ![downloads][downloads-badge] [![.NET][dotnet-badge]][dotnet-url] [![Python][python-badge]][python-url] [![license][license-badge]][license-url]

![GWM ORA][banner]

Native Home Assistant add-on and custom integration for GWM ORA vehicles. The add-on owns GWM login, token storage, cloud polling, and remote commands; the integration exposes polished native Home Assistant entities without MQTT.

The add-on is standalone: Home Assistant Supervisor builds it locally from this repository's Dockerfile. No GHCR-published image is required.

## What It Does

- Polls the GWM cloud from a Home Assistant add-on.
- Exposes SOC, range, odometer, tire pressure, tire temperature, cabin temperature, charging, plug, window, lock, A/C, and location entities.
- Provides native Home Assistant climate, lock, and close-window controls.
- Keeps GWM credentials and vehicle PIN inside add-on configuration/storage.
- Lets the integration auto-discover the add-on through Home Assistant Supervisor discovery.
- Uses an internal token-protected add-on API; no MQTT broker or MQTT discovery is required.

## Repository Pieces

- `addons/gwm_ora`: Home Assistant add-on metadata, docs, Dockerfile, and .NET add-on source.
- `addons/gwm_ora/src/GwmOra.Addon`: .NET add-on service.
- `addons/gwm_ora/src/LibGwmApi`: adapted reusable GWM API client.
- `custom_components/gwm_ora`: Home Assistant custom integration.
- `brand`: project brand images used by the README, add-on, and integration.

## Security Notes

The packaged add-on follows a conservative shape:

- No published LAN port by default.
- No host network.
- No Docker API access.
- No `full_access`.
- Token-protected internal API.
- Home Assistant Ingress web UI, restricted to the Supervisor ingress proxy.
- Custom AppArmor profile.
- GWM username, password, refresh tokens, and security PIN stay in add-on configuration/storage.
- Remote commands are disabled by default and require both `enable_remote_commands: true` and `security_pin`.

Remote commands can affect the real vehicle. Test carefully and use a dedicated/shared GWM account where possible.

## Supported Architectures

The add-on declares support for:

- `amd64`
- `aarch64`
- `armv7`

Because the add-on is built locally by Supervisor, installation can take longer than a prebuilt image download.

## Installation

### 1. Add the Add-on Repository

[![Add the GWM ORA add-on repository to Home Assistant](https://my.home-assistant.io/badges/supervisor_add_addon_repository.svg)](https://my.home-assistant.io/redirect/supervisor_add_addon_repository/?repository_url=https%3A%2F%2Fgithub.com%2Fmoryoav%2Fha-gwm_ora)

Use the button above to add this repository to Home Assistant's Apps/Add-ons store.

Manual path:

1. Go to **Settings** -> **Apps** or **Add-ons**.
2. Open the menu in the top right.
3. Choose **Repositories**.
4. Add:

```text
https://github.com/moryoav/ha-gwm_ora
```

### 2. Install and Start the Add-on

Install **GWM ORA**, then configure:

```yaml
country: DE
username: owner@example.com
password: your-gwm-password
enable_remote_commands: false
poll_interval_seconds: 60
log_level: info
```

Remote commands require:

```yaml
enable_remote_commands: true
security_pin: "123456"
```

If GWM requires SMS or e-mail verification, the add-on will request a one-time code and report `verification_required` in its health state. Enter the received code in the optional `verification_code` add-on option, save, and restart the add-on. After successful login, the add-on stores GWM tokens under `/data` and tries to clear `verification_code` from the add-on options.

The add-on also provides an **Open Web UI** button through Home Assistant Ingress. It shows current add-on health and the latest cached vehicle summary; remote controls stay in the native Home Assistant integration.

### 3. Install the Custom Integration

#### HACS

[![Open the GWM ORA HACS repository](https://my.home-assistant.io/badges/hacs_repository.svg)](https://my.home-assistant.io/redirect/hacs_repository/?owner=moryoav&repository=ha-gwm_ora&category=integration)

Use the button above to add this custom integration in HACS.

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

#### Manual

Copy:

```text
custom_components/gwm_ora
```

to:

```text
/config/custom_components/gwm_ora
```

Then restart Home Assistant.

### 4. Add the Integration

[![Add the GWM ORA integration](https://my.home-assistant.io/badges/config_flow_start.svg)](https://my.home-assistant.io/redirect/config_flow_start/?domain=gwm_ora)

Use the button above after Home Assistant restarts. The integration should discover the running add-on and ask for confirmation. There are no GWM credentials to enter in the integration.

## Entities

- Sensors: SOC, range, odometer, remaining charging time, SOCE, tire pressures, tire temperatures, interior temperature, acquisition/update timestamps, remote command status.
- Binary sensors: charging active, charge plug, A/C active, lock open, windows open, air circulation, front defroster.
- Device tracker: vehicle GPS location when available.
- Climate: A/C mode `off`/`cool`, target temperature, current cabin temperature.
- Lock: lock and unlock vehicle doors.
- Button: close all windows.

Climate, lock, and button entities are unavailable until remote commands are enabled and a security PIN is configured in the add-on.

## Supported Vehicles

The integration is designed for GWM ORA vehicles that use the same GWM cloud API behavior as the original `ora2mqtt` project. Initial mapping targets ORA vehicles such as ORA 03/Funky Cat style models exposed by the GWM mobile app.

Known constraints:

- Regional GWM backends can differ.
- Some item codes may be absent on some vehicle model years or trims.
- Remote commands depend on the official app account, region, vehicle support, security PIN, and current vehicle/cloud state.
- Firmware/software updates are not exposed by this integration because the GWM cloud API behavior used here does not provide a safe update path.

## Data Updates

The add-on polls GWM cloud data on its own schedule using `poll_interval_seconds`. The integration polls the add-on's cached vehicle snapshot every 30 seconds and does not trigger a fresh GWM cloud request for every Home Assistant refresh.

Use the add-on option to control cloud polling frequency:

```yaml
poll_interval_seconds: 60
```

Lower values make Home Assistant feel fresher but can increase GWM cloud traffic. Keep the interval conservative unless you have a specific reason to change it.

## Diagnostics

Home Assistant diagnostics are available from the integration entry. Diagnostics include config-entry metadata and the latest cached vehicle snapshot, with the add-on API token redacted.

Before sharing diagnostics publicly, still review them for VINs, precise locations, raw item codes, timestamps, or anything else you consider private.

## Troubleshooting

### Add-on Is Not Discovered

- Confirm the add-on is installed and started.
- Check the add-on log for login or options errors.
- Restart the add-on to publish Supervisor discovery again.
- Restart Home Assistant if the integration was installed after the add-on had already started.

### Integration Cannot Connect

- Confirm the add-on health endpoint is running in the add-on log.
- Confirm the integration was discovered by Supervisor rather than manually pointed at the wrong host or port.
- For development installs, reconfigure the integration with the current host, port, and generated API token.

### API Token Rejected

Restart the add-on first. The add-on persists its generated API token under `/data`; if the state file changes, Supervisor discovery should publish the new token and the integration can update from rediscovery. Development installs can use the integration's reconfigure flow.

### GWM Login Fails

- Verify the same account works in the official GWM app.
- If the add-on reports `verification_required`, enter the received one-time code in `verification_code`, save, and restart the add-on.
- Confirm the add-on `country`, `username`, and `password` options.
- Increase log level temporarily if needed.

### Remote Commands Are Unavailable

Remote commands are intentionally disabled unless both are true:

- `enable_remote_commands: true`
- `security_pin` is configured in the add-on

After changing either option, restart the add-on and reload the integration.

### Entities Are Missing or Unavailable

- Some entities depend on values returned by your vehicle and region.
- Newly discovered vehicles are added automatically after the coordinator sees them.
- If a previously known VIN disappears from the GWM cloud response, existing entities remain but become unavailable instead of being deleted immediately.

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

Pre-cool the cabin when remote commands are explicitly enabled:

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

Remote command automations should be tested manually first and used only when the vehicle is parked in a safe location.

## Removal

To remove the integration:

1. Delete the `GWM ORA` integration entry from Home Assistant.
2. Stop and uninstall the `GWM ORA` add-on.
3. Remove this repository from the add-on store if you no longer need it.
4. Remove the custom integration from HACS or delete `/config/custom_components/gwm_ora`.
5. Restart Home Assistant.

The add-on stores generated state under its `/data` directory. Removing the add-on removes that stored token state.

## Quality Scale

This repository tracks Home Assistant Integration Quality Scale progress in `custom_components/gwm_ora/quality_scale.yaml` and [docs/QUALITY_SCALE.md](docs/QUALITY_SCALE.md). The goal is Gold-level user experience, with remaining work called out honestly for custom-integration coverage and strict typing.

## Development

The add-on targets .NET 10 LTS. Local .NET builds require a .NET 10 SDK.

```powershell
dotnet test
python -m compileall custom_components tests/python
python -m pytest tests/python
```

To test the same build context Home Assistant Supervisor uses:

```powershell
docker build -f addons/gwm_ora/Dockerfile addons/gwm_ora
```

## Releases

HACS uses GitHub releases for version detection. For every version, update `CHANGELOG.md`, `addons/gwm_ora/config.yaml`, and `custom_components/gwm_ora/manifest.json`, then push a `vX.Y.Z` tag. The release workflow creates the GitHub release from the matching changelog section.

This repository publishes stable releases only for now; no canary branch is offered.

## Disclaimer

This project is unofficial and is not affiliated with or endorsed by Great Wall Motor, GWM, ORA, or Home Assistant. Vehicle cloud APIs and remote command behavior may change without notice.

Use at your own risk. You are responsible for validating behavior, protecting credentials, keeping backups, and deciding whether remote commands are appropriate for your vehicle and environment.

[banner]: https://raw.githubusercontent.com/moryoav/ha-gwm_ora/main/brand/banner.png
[hacs-badge]: https://img.shields.io/badge/HACS-Custom-41BDF5.svg?style=flat-square
[hacs-url]: https://github.com/hacs/integration
[release-badge]: https://img.shields.io/github/v/release/moryoav/ha-gwm_ora?style=flat-square
[release-url]: https://github.com/moryoav/ha-gwm_ora/releases
[downloads-badge]: https://img.shields.io/github/downloads/moryoav/ha-gwm_ora/total?style=flat-square
[dotnet-badge]: https://img.shields.io/github/actions/workflow/status/moryoav/ha-gwm_ora/dotnet.yml?branch=main&style=flat-square&label=.NET
[dotnet-url]: https://github.com/moryoav/ha-gwm_ora/actions/workflows/dotnet.yml
[python-badge]: https://img.shields.io/github/actions/workflow/status/moryoav/ha-gwm_ora/python.yml?branch=main&style=flat-square&label=Python
[python-url]: https://github.com/moryoav/ha-gwm_ora/actions/workflows/python.yml
[license-badge]: https://img.shields.io/github/license/moryoav/ha-gwm_ora?style=flat-square
[license-url]: https://github.com/moryoav/ha-gwm_ora/blob/main/LICENSE

# GWM ORA for Home Assistant
[![HACS][hacs-badge]][hacs-url] [![release][release-badge]][release-url] ![downloads][downloads-badge] [![.NET][dotnet-badge]][dotnet-url] [![Python][python-badge]][python-url] [![images][images-badge]][images-url] [![license][license-badge]][license-url]

![GWM ORA][banner]

Native Home Assistant add-on and custom integration for GWM ORA vehicles. The add-on owns GWM login, token storage, cloud polling, and remote commands; the integration exposes polished native Home Assistant entities without MQTT.

## What It Does

- Polls the GWM cloud from a Home Assistant add-on.
- Exposes SOC, range, odometer, tire pressure, tire temperature, cabin temperature, charging, plug, window, lock, A/C, and location entities.
- Provides native Home Assistant climate, lock, and close-window controls.
- Keeps GWM credentials and vehicle PIN inside add-on configuration/storage.
- Lets the integration auto-discover the add-on through Home Assistant Supervisor discovery.
- Uses an internal token-protected add-on API; no MQTT broker or MQTT discovery is required.

## Repository Pieces

- `addons/gwm_ora`: Home Assistant add-on metadata, docs, and container packaging.
- `src/GwmOra.Addon`: .NET add-on service.
- `src/LibGwmApi`: adapted reusable GWM API client.
- `custom_components/gwm_ora`: Home Assistant custom integration.
- `brand`: project brand images used by the README, add-on, and integration.

## Security Notes

The packaged add-on follows a conservative shape:

- No published LAN port by default.
- No host network.
- No Docker API access.
- No `full_access`.
- Token-protected internal API.
- GWM username, password, refresh tokens, and security PIN stay in add-on configuration/storage.
- Remote commands are disabled by default and require both `enable_remote_commands: true` and `security_pin`.

Remote commands can affect the real vehicle. Test carefully and use a dedicated/shared GWM account where possible.

## Supported Architectures

Prebuilt add-on images are published for:

- `amd64`
- `aarch64`
- `armv7`

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
security_pin: ""
enable_remote_commands: false
poll_interval_seconds: 60
log_level: info
```

Remote commands require:

```yaml
enable_remote_commands: true
security_pin: "123456"
```

If GWM requires SMS or e-mail verification during login, complete that flow once in the official app first. The add-on is non-interactive and cannot prompt for a verification code.

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

## Development

The add-on targets .NET 10 LTS. Local .NET builds require a .NET 10 SDK.

```powershell
dotnet test
python -m compileall custom_components tests/python
python -m pytest tests/python
```

Build the add-on image from the repository root:

```powershell
docker build -f addons/gwm_ora/Dockerfile .
```

## Publishing Images

GitHub Actions builds and publishes architecture-specific images to GitHub Container Registry:

```text
ghcr.io/moryoav/ha-gwm-ora-amd64
ghcr.io/moryoav/ha-gwm-ora-aarch64
ghcr.io/moryoav/ha-gwm-ora-armv7
```

The add-on `config.yaml` points at `ghcr.io/moryoav/ha-gwm-ora-{arch}`.

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
[images-badge]: https://img.shields.io/github/actions/workflow/status/moryoav/ha-gwm_ora/docker.yml?branch=main&style=flat-square&label=Images
[images-url]: https://github.com/moryoav/ha-gwm_ora/actions/workflows/docker.yml
[license-badge]: https://img.shields.io/github/license/moryoav/ha-gwm_ora?style=flat-square
[license-url]: https://github.com/moryoav/ha-gwm_ora/blob/main/LICENSE

# Changelog

All notable changes to this project will be documented in this file.

This project follows semantic versioning. HACS uses the latest GitHub release tag as the remote version, so every released version must have both a tag and a GitHub release.

## [Unreleased]

## [0.2.3] - 2026-06-19

### Fixed

- Fixed Supervisor local add-on builds by copying `gwm_root.pem` from the Docker build stage instead of reading it again from the source context in the runtime stage.

### Added

- Added a non-publishing multi-architecture add-on Docker build check in CI.

## [0.2.2] - 2026-06-19

### Fixed

- Corrected the maintainer name to Yoav Mor in repository metadata and license text.
- Removed the stale README image-workflow badge after switching the add-on to local Supervisor builds.

## [0.2.1] - 2026-06-19

### Changed

- Removed GHCR image publishing and the add-on `image` setting so Home Assistant Supervisor builds the standalone add-on locally from the repository Dockerfile.
- Added Home Assistant local-build labels to the add-on Dockerfile.

## [0.2.0] - 2026-06-19

### Added

- Added Home Assistant Integration Quality Scale tracking with `custom_components/gwm_ora/quality_scale.yaml`.
- Added Gold-track documentation for supported devices, data updates, diagnostics, troubleshooting, use cases, examples, known limitations, and removal.
- Added GitHub community health files: Code of Conduct, Contributing, Security, Support, issue forms, and pull request template.
- Added reconfigure and reauthentication flows for manual/development add-on API connection updates.
- Added Home Assistant repair issue creation when the add-on API token is rejected.
- Added dynamic entity creation for vehicles discovered after initial setup.
- Added entity icon translations and disabled-by-default diagnostic timestamp/command-status entities.

### Changed

- Distinguished add-on authentication failures from remote-command permission failures.
- Wrapped remote command entity failures in translated Home Assistant errors.
- Declared platform parallel update behavior for all integration platforms.

## [0.1.0] - 2026-06-19

### Added

- Initial native Home Assistant add-on for GWM ORA cloud polling and remote commands.
- Initial Home Assistant custom integration with Supervisor discovery and manual development setup.
- Native sensor, binary sensor, device tracker, climate, lock, and button entities.
- Token-protected internal add-on API with persistent add-on state under `/data`.
- Multi-architecture add-on image builds for `amd64`, `aarch64`, and `armv7`.
- Brand assets and installation documentation for add-on store and HACS setup.

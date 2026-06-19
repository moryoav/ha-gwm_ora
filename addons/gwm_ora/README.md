# GWM ORA Add-on

This add-on runs the native GWM ORA bridge service used by the `gwm_ora` Home Assistant custom integration.

Configure the GWM account country, e-mail, and password in the add-on options before starting it. If GWM requests SMS/e-mail verification for the add-on device, enter the received code in `verification_code` and restart the add-on. Remote commands require both `enable_remote_commands: true` and the vehicle security PIN from the official app.

The add-on exposes an authenticated Home Assistant Ingress status page through **Open Web UI**. Vehicle controls are provided by the native `gwm_ora` Home Assistant integration.

The add-on exposes only an internal Home Assistant API port and publishes Supervisor discovery for the custom integration. MQTT is not used.

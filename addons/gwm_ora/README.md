# GWM ORA Add-on

This add-on runs the native GWM ORA bridge service used by the `gwm_ora` Home Assistant custom integration.

Configure the GWM account country, e-mail, and password in the add-on options before starting it. Remote commands require both `enable_remote_commands: true` and the vehicle security PIN from the official app.

The add-on exposes only an internal Home Assistant API port and publishes Supervisor discovery for the custom integration. MQTT is not used.

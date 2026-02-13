# DSC TLink Server

This add-on runs the DSC Powerseries Neo relay server, which bridges communication between your DSC panel (via ITv2/TLink) and Home Assistant.

## How it works

The server listens for inbound connections from your DSC panel on the **panel port** (default 3072) and exposes a relay port (default 3078) that the Home Assistant HACS integration connects to.

```
DSC Panel ---(ITv2 port 3072)---> [This Add-on] ---(relay port 3078)---> HA Integration
```

## Configuration

| Option           | Description                                              | Default     |
|------------------|----------------------------------------------------------|-------------|
| `integration_id` | The integration account ID configured on your DSC panel  | *(required)* |
| `encryption_key` | The encryption key configured on your DSC panel          | *(required)* |
| `relay_secret`   | Shared secret for relay communication with HA            | *(required)* |
| `panel_port`     | Port the panel connects to (ITv2)                        | `3072`      |
| `relay_port`     | Port the HA integration connects to                      | `3078`      |
| `relay_ip`       | IP address to bind the relay listener                    | `0.0.0.0`   |
| `log_level`      | Logging verbosity: `info`, `debug`, or `trace`           | `info`      |

## Installation

1. In Home Assistant, go to **Settings → Add-ons → Add-on Store**.
2. Click the three-dot menu and select **Repositories**.
3. Add this repository URL: `https://github.com/Z6543/DSC-TLink`
4. Find **DSC TLink Server** in the add-on store and install it.
5. Configure the add-on with your panel's integration ID, encryption key, and relay secret.
6. Start the add-on.

## Network

This add-on uses **host networking** so that the DSC panel can reach the server on the host IP. Ensure your panel is configured to connect to your Home Assistant host on port 3072 (or your configured panel port).

#!/bin/sh

CONFIG="/data/options.json"

INTEGRATION_ID=$(jq -r '.integration_id' "$CONFIG")
ENCRYPTION_KEY=$(jq -r '.encryption_key' "$CONFIG")
RELAY_SECRET=$(jq -r '.relay_secret' "$CONFIG")
PANEL_PORT=$(jq -r '.panel_port' "$CONFIG")
RELAY_PORT=$(jq -r '.relay_port' "$CONFIG")
RELAY_IP=$(jq -r '.relay_ip' "$CONFIG")
LOG_LEVEL=$(jq -r '.log_level' "$CONFIG")

ARGS="${INTEGRATION_ID} ${ENCRYPTION_KEY}"
ARGS="${ARGS} --port ${PANEL_PORT}"
ARGS="${ARGS} --relay-port ${RELAY_PORT}"
ARGS="${ARGS} --relay-ip ${RELAY_IP}"
ARGS="${ARGS} --relay-secret ${RELAY_SECRET}"

case "${LOG_LEVEL}" in
  debug) ARGS="${ARGS} --debug" ;;
  trace) ARGS="${ARGS} --trace" ;;
esac

echo "Starting DSC TLink server..."
echo "  Panel port: ${PANEL_PORT}"
echo "  Relay port: ${RELAY_PORT}"

exec /app/Demo ${ARGS}

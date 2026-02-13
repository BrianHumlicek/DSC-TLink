#!/usr/bin/with-contenv bashio

INTEGRATION_ID=$(bashio::config 'integration_id')
ENCRYPTION_KEY=$(bashio::config 'encryption_key')
RELAY_SECRET=$(bashio::config 'relay_secret')
PANEL_PORT=$(bashio::config 'panel_port')
RELAY_PORT=$(bashio::config 'relay_port')
RELAY_IP=$(bashio::config 'relay_ip')
LOG_LEVEL=$(bashio::config 'log_level')

ARGS=("${INTEGRATION_ID}" "${ENCRYPTION_KEY}")
ARGS+=(--port "${PANEL_PORT}")
ARGS+=(--relay-port "${RELAY_PORT}")
ARGS+=(--relay-ip "${RELAY_IP}")
ARGS+=(--relay-secret "${RELAY_SECRET}")

case "${LOG_LEVEL}" in
  debug) ARGS+=(--debug) ;;
  trace) ARGS+=(--trace) ;;
esac

bashio::log.info "Starting DSC TLink server..."
bashio::log.info "  Panel port: ${PANEL_PORT}"
bashio::log.info "  Relay port: ${RELAY_PORT}"

exec /app/Demo "${ARGS[@]}"

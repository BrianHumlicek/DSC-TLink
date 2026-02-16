"""Config flow for DSC Powerseries Neo integration."""

from __future__ import annotations

import asyncio
import logging

import voluptuous as vol

from homeassistant import config_entries

from .const import CONF_HOST, CONF_RELAY_PORT, CONF_RELAY_SECRET, DEFAULT_RELAY_PORT, DOMAIN

_LOGGER = logging.getLogger(__name__)


class DscNeoConfigFlow(config_entries.ConfigFlow, domain=DOMAIN):
    """Handle a config flow for DSC Neo."""

    VERSION = 1

    async def async_step_user(self, user_input=None):
        """Handle the initial step."""
        errors = {}

        if user_input is not None:
            try:
                reader, writer = await asyncio.wait_for(
                    asyncio.open_connection(
                        user_input[CONF_HOST], user_input[CONF_RELAY_PORT]
                    ),
                    timeout=5,
                )
                writer.close()
                await writer.wait_closed()
            except (OSError, asyncio.TimeoutError):
                errors["base"] = "cannot_connect"
            else:
                await self.async_set_unique_id(
                    f"{user_input[CONF_HOST]}:{user_input[CONF_RELAY_PORT]}"
                )
                self._abort_if_unique_id_configured()

                return self.async_create_entry(
                    title=f"DSC Neo ({user_input[CONF_HOST]})",
                    data=user_input,
                )

        return self.async_show_form(
            step_id="user",
            data_schema=vol.Schema(
                {
                    vol.Required(CONF_HOST): str,
                    vol.Optional(
                        CONF_RELAY_PORT, default=DEFAULT_RELAY_PORT
                    ): int,
                    vol.Required(CONF_RELAY_SECRET): str,
                }
            ),
            errors=errors,
        )

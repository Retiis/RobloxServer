#!/bin/bash
# RobloxServer - asks the router (UPnP IGD) to forward the web port and the game ports to the Pi,
# and refreshes the optional dynamic DNS name. Run by robloxserver-upnp.timer every 30 minutes.
set -uo pipefail

CONF="${RS_CONF:-/etc/robloxserver/robloxserver-pi.conf}"
# shellcheck source=/dev/null
. "$CONF"

if [ "${UPNP_ENABLED:-no}" = "yes" ]; then
    args=("$HTTP_PORT" TCP)
    for forward in $GAME_FORWARDS; do
        external="${forward%%:*}"
        for proto in $GAME_PROTOCOLS; do
            args+=("$external" "${proto^^}")
        done
    done
    # -e description, -r port proto [port proto ...] maps to this host (the Pi).
    if upnpc -e "RobloxServer" -r "${args[@]}"; then
        echo "UPnP mappings refreshed: ${args[*]}"
    else
        echo "UPnP failed - forward the ports to $PI_IP manually in the router" >&2
    fi
    upnpc -s 2>/dev/null | grep -i "ExternalIPAddress" || true
fi

if [ -n "${DDNS_URL:-}" ]; then
    curl -fsS --max-time 20 "$DDNS_URL" && echo " (DDNS updated)"
fi
exit 0

#!/bin/bash
# RobloxServer - port forwarding on the Raspberry Pi.
# Players on the Internet -> router (forward to the Pi) -> Pi DNAT -> PC hosting the game.
# MASQUERADE makes the game host answer through the Pi, so the router's NAT sees a symmetric flow.
set -euo pipefail

CONF="${RS_CONF:-/etc/robloxserver/robloxserver-pi.conf}"
# shellcheck source=/dev/null
. "$CONF"

IPT="${IPTABLES:-iptables}"

flush() {
    for table_chain in "nat RS_PREROUTING" "nat RS_POSTROUTING" "filter RS_FORWARD"; do
        set -- $table_chain
        $IPT -t "$1" -N "$2" 2>/dev/null || true
        $IPT -t "$1" -F "$2"
    done
    $IPT -t nat -C PREROUTING -j RS_PREROUTING 2>/dev/null || $IPT -t nat -A PREROUTING -j RS_PREROUTING
    $IPT -t nat -C POSTROUTING -j RS_POSTROUTING 2>/dev/null || $IPT -t nat -A POSTROUTING -j RS_POSTROUTING
    $IPT -t filter -C FORWARD -j RS_FORWARD 2>/dev/null || $IPT -t filter -I FORWARD 1 -j RS_FORWARD
}

start() {
    sysctl -q -w net.ipv4.ip_forward=1
    flush

    for forward in $GAME_FORWARDS; do
        IFS=: read -r external host internal <<< "$forward"
        internal="${internal:-$external}"
        for proto in $GAME_PROTOCOLS; do
            $IPT -t nat -A RS_PREROUTING -i "$PI_INTERFACE" -d "$PI_IP" -p "$proto" --dport "$external" \
                -j DNAT --to-destination "$host:$internal"
            $IPT -t nat -A RS_POSTROUTING -d "$host" -p "$proto" --dport "$internal" -j MASQUERADE
            $IPT -t filter -A RS_FORWARD -d "$host" -p "$proto" --dport "$internal" -j ACCEPT
            $IPT -t filter -A RS_FORWARD -s "$host" -p "$proto" --sport "$internal" -j ACCEPT
            echo "forward $proto $PI_IP:$external -> $host:$internal"
        done
    done
}

stop() {
    flush
}

case "${1:-start}" in
    start) start ;;
    stop) stop ;;
    *) echo "usage: $0 start|stop" >&2; exit 1 ;;
esac

#!/bin/bash
# =============================================================================
# Raspberry Pi (ARM64) — Setup & Deployment Script for ProcessLoader
#
# This script does everything needed to get ProcessLoader running:
#   1. Installs .NET 8 runtime (if not present)
#   2. Installs libgdiplus (required by System.Drawing on Linux)
#   3. Builds OpenCV + OpenCvSharpExtern native libraries
#   4. Final setup and permissions (commented out — uncomment as needed)
#
# Prerequisites:
#   - Raspberry Pi 4/5 with 64-bit Raspberry Pi OS (Debian 11 Bullseye / 12 Bookworm / 13 Trixie)
#   - Internet access (for initial setup only)
#
# NOTE: If .NET was previously installed per-user (e.g. /home/pi/.dotnet),
#       this script installs .NET 8 system-wide at /usr/share/dotnet instead.
#
# =============================================================================

set -e

APP_DIR="${1:-$HOME/app}"
SCRIPTS_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "============================================="
echo "  Raspberry Pi — ProcessLoader Setup"
echo "============================================="
echo "  App directory:  ${APP_DIR}"
echo "  Scripts dir:    ${SCRIPTS_DIR}"
echo "  Date:           $(date)"
echo "============================================="
echo ""

# -----------------------------------------------
# Step 1: Install .NET 8 Runtime
# -----------------------------------------------
echo "[Step 1/4] Checking .NET 8 runtime..."

# Check for dotnet in common locations (system-wide and user-level installs)
DOTNET_CMD=""
if command -v dotnet &>/dev/null; then
    DOTNET_CMD="dotnet"
elif [ -x /usr/share/dotnet/dotnet ]; then
    DOTNET_CMD="/usr/share/dotnet/dotnet"
elif [ -x /home/pi/.dotnet/dotnet ]; then
    DOTNET_CMD="/home/pi/.dotnet/dotnet"
fi

NEED_INSTALL=true
if [ -n "$DOTNET_CMD" ] && $DOTNET_CMD --list-runtimes 2>/dev/null | grep -q "Microsoft.NETCore.App 8"; then
    echo "  .NET 8 runtime already installed: $($DOTNET_CMD --version)"
    NEED_INSTALL=false
fi

if [ "$NEED_INSTALL" = true ]; then
    if [ -n "$DOTNET_CMD" ]; then
        echo "  Found existing .NET: $($DOTNET_CMD --version 2>/dev/null || echo 'unknown version')"
        echo "  .NET 8 not present — installing alongside..."
    else
        echo "  .NET not found — installing .NET 8 SDK..."
    fi

    # For Raspberry Pi OS (Debian-based), use the dotnet-install script.
    # The Microsoft apt feed does not support Raspberry Pi OS directly.
    echo "  Using dotnet-install script for Raspberry Pi OS (Debian)..."
    wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet
    ln -sf /usr/share/dotnet/dotnet /usr/bin/dotnet
    rm /tmp/dotnet-install.sh

    # Set up environment so dotnet is available for the rest of this script
    export DOTNET_ROOT=/usr/share/dotnet
    export PATH="$DOTNET_ROOT:$PATH"

    # Persist environment for all users
    cat > /etc/profile.d/dotnet.sh <<'DOTNETEOF'
export DOTNET_ROOT=/usr/share/dotnet
export PATH="$DOTNET_ROOT:$PATH"
DOTNETEOF

    echo "  .NET 8 installed: $(dotnet --version)"
    echo "  DOTNET_ROOT set to /usr/share/dotnet"
fi
echo ""

# -----------------------------------------------
# Step 2: Install libgdiplus (System.Drawing)
# -----------------------------------------------
echo "[Step 2/4] Installing libgdiplus (for System.Drawing.Common)..."
if dpkg -l | grep -q libgdiplus; then
    echo "  libgdiplus already installed."
else
    apt-get update
    apt-get install -y libgdiplus
    # Create symlink if not present (some distros need this)
    if [ ! -f /usr/lib/aarch64-linux-gnu/libgdiplus.so ]; then
        ln -sf /usr/lib/libgdiplus.so /usr/lib/aarch64-linux-gnu/libgdiplus.so
    fi
    echo "  libgdiplus installed."
fi
echo ""

# -----------------------------------------------
# Step 3: Build OpenCV + OpenCvSharpExtern
# -----------------------------------------------
echo "[Step 3/4] OpenCV + OpenCvSharpExtern native libraries..."

# Check if libOpenCvSharpExtern.so already exists in the app dir
if [ -f "${APP_DIR}/libOpenCvSharpExtern.so" ]; then
    echo "  libOpenCvSharpExtern.so already present in ${APP_DIR}."
    echo "  Skipping OpenCV build. Delete the file and re-run to rebuild."
else
    BUILD_SCRIPT="${SCRIPTS_DIR}/rpi-build-opencv-and-wrapper.sh"
    if [ -f "$BUILD_SCRIPT" ]; then
        echo "  Building OpenCV + OpenCvSharpExtern (this takes 2-4 hours on RPi)..."
        chmod +x "$BUILD_SCRIPT"
        bash "$BUILD_SCRIPT" "${APP_DIR}"
    else
        echo "  WARNING: ${BUILD_SCRIPT} not found!"
        echo "  You need libOpenCvSharpExtern.so in ${APP_DIR} for OpenCvSharp to work."
        echo "  See: scripts/rpi/rpi-build-opencv-and-wrapper.sh"
    fi
fi
echo ""

# -----------------------------------------------
# Step 4: Final setup and permissions (commented out — uncomment as needed)
# -----------------------------------------------
# echo "[Step 4/4] Configuring environment..."

# # Make the app executable
# if [ -f "${APP_DIR}/ProcessLoader" ]; then
#     chmod +x "${APP_DIR}/ProcessLoader"
# fi

# # Create output directories
# mkdir -p "${APP_DIR}/output"
# mkdir -p "${APP_DIR}/AudioOutput"
# mkdir -p "${APP_DIR}/input"

# # Create the systemd service file for ProcessLoader
# cat > /etc/systemd/system/processloader.service <<EOF
# [Unit]
# Description=IVA ProcessLoader Service
# After=network-online.target
# Wants=network-online.target
#
# [Service]
# Type=simple
# WorkingDirectory=${APP_DIR}
# ExecStart=${APP_DIR}/ProcessLoader
# Restart=on-failure
# RestartSec=10
# Environment=DOTNET_ENVIRONMENT=Production
# Environment=DOTNET_ROOT=/usr/share/dotnet
# Environment=LD_LIBRARY_PATH=/usr/local/lib:${APP_DIR}
# Environment=DISPLAY=:0
#
# # Resource limits for Raspberry Pi
# LimitNOFILE=65536
# LimitMEMLOCK=infinity
#
# [Install]
# WantedBy=multi-user.target
# EOF
#
# systemctl daemon-reload
# echo "  Systemd service 'processloader' created."

# # Create a convenience start/stop script
# cat > "${APP_DIR}/run.sh" <<'RUNEOF'
# #!/bin/bash
# # Quick-start script for ProcessLoader
# export DOTNET_ROOT=/usr/share/dotnet
# export PATH="$DOTNET_ROOT:$PATH"
# export LD_LIBRARY_PATH=/usr/local/lib:$(dirname "$0"):$LD_LIBRARY_PATH
# export DISPLAY=${DISPLAY:-:0}
# cd "$(dirname "$0")"
# echo "Starting ProcessLoader from $(pwd)..."
# ./ProcessLoader
# RUNEOF
# chmod +x "${APP_DIR}/run.sh"

echo ""
echo "============================================="
echo "  Setup Complete!"
echo "============================================="
echo ""
echo "  Installed:"
echo "    - .NET 8 runtime"
echo "    - libgdiplus (System.Drawing support)"
echo "    - OpenCV + OpenCvSharpExtern (if built)"
echo ""
echo "  To run ProcessLoader:"
echo ""
echo "    Option A — Directly:"
echo "      cd ${APP_DIR}"
echo "      export LD_LIBRARY_PATH=/usr/local/lib:${APP_DIR}:\$LD_LIBRARY_PATH"
echo "      ./ProcessLoader"
echo ""
echo "    Option B — Using the run script:"
echo "      ${APP_DIR}/run.sh"
echo ""
echo "    Option C — As a systemd service:"
echo "      sudo systemctl start processloader"
echo "      sudo systemctl status processloader"
echo "      journalctl -u processloader -f    # view logs"
echo ""
echo "  Configuration files are in:"
echo "      ${APP_DIR}/Configurations/"
echo "============================================="

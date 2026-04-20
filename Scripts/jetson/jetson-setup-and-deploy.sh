#!/bin/bash
# =============================================================================
# Jetson Orin Nano — Full Setup & Deployment Script for ProcessLoader
#
# This script does everything needed to get ProcessLoader running:
#   1. Installs .NET 8 runtime (if not present)
#   2. Installs libgdiplus (required by System.Drawing on Linux)
#   3. Builds OpenCV + OpenCvSharpExtern (calls sibling script)
#   4. Deploys ProcessLoader from the publish output
#
# Prerequisites:
#   - NVIDIA Jetson with JetPack 5.x / 6.x
#     JetPack 5.x = Ubuntu 20.04, CUDA 11.4
#     JetPack 6.x = Ubuntu 22.04, CUDA 12.2+
#   - Internet access (for initial setup only)
#
# =============================================================================

set -e

APP_DIR="${1:-$HOME/app}"
SCRIPTS_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "============================================="
echo "  Jetson Orin Nano — IVA Setup"
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
if command -v dotnet &>/dev/null && dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 8"; then
    echo "  .NET 8 runtime already installed: $(dotnet --version)"
else
    echo "  Installing .NET 8 SDK for ARM64..."
    # Detect Ubuntu version from /etc/os-release for the correct apt feed
    UBUNTU_VER=$(. /etc/os-release && echo "$VERSION_ID")
    if [ -z "$UBUNTU_VER" ]; then
        UBUNTU_VER="22.04"
        echo "  WARNING: Could not detect Ubuntu version, defaulting to ${UBUNTU_VER}"
    fi
    echo "  Detected Ubuntu ${UBUNTU_VER}"
    wget -q "https://packages.microsoft.com/config/ubuntu/${UBUNTU_VER}/packages-microsoft-prod.deb" -O /tmp/packages-microsoft-prod.deb
    dpkg -i /tmp/packages-microsoft-prod.deb
    rm /tmp/packages-microsoft-prod.deb
    apt-get update
    apt-get install -y dotnet-sdk-8.0
    echo "  .NET 8 installed: $(dotnet --version)"
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
    BUILD_SCRIPT="${SCRIPTS_DIR}/jetson-build-opencv-and-wrapper.sh"
    if [ -f "$BUILD_SCRIPT" ]; then
        echo "  Building OpenCV + OpenCvSharpExtern (this takes 30-60 minutes)..."
        chmod +x "$BUILD_SCRIPT"
        bash "$BUILD_SCRIPT" "${APP_DIR}"
    else
        echo "  WARNING: ${BUILD_SCRIPT} not found!"
        echo "  You need libOpenCvSharpExtern.so in ${APP_DIR} for OpenCvSharp to work."
        echo "  See: scripts/jetson-build-opencv-and-wrapper.sh"
    fi
fi
echo ""

# -----------------------------------------------
# Step 4: Final setup and permissions
# -----------------------------------------------
# echo "[Step 4/4] Configuring environment..."

# # Make the app executable
# if [ -f "${APP_DIR}/ProcessLoader" ]; then
#     chmod +x "${APP_DIR}/ProcessLoader"
# fi

# Create output directories
# mkdir -p "${APP_DIR}/output"
# mkdir -p "${APP_DIR}/AudioOutput"
# mkdir -p "${APP_DIR}/input"

# # Create the systemd service file for ProcessLoader
# cat > /etc/systemd/system/processloader.service <<EOF
# [Unit]
# Description=IVA ProcessLoader Service
# After=network-online.target
# Wants=network-online.target

# [Service]
# Type=simple
# WorkingDirectory=${APP_DIR}
# ExecStart=${APP_DIR}/ProcessLoader
# Restart=on-failure
# RestartSec=10
# Environment=DOTNET_ENVIRONMENT=Production
# Environment=LD_LIBRARY_PATH=/usr/local/lib:${APP_DIR}
# Environment=DISPLAY=:0

# # Resource limits for Jetson
# LimitNOFILE=65536

# [Install]
# WantedBy=multi-user.target
# EOF

# systemctl daemon-reload
# echo "  Systemd service 'processloader' created."

# # Create a convenience start/stop script
# cat > "${APP_DIR}/run.sh" <<'RUNEOF'
# #!/bin/bash
# # Quick-start script for ProcessLoader
# export LD_LIBRARY_PATH=/usr/local/lib:$(dirname "$0"):$LD_LIBRARY_PATH
# export DISPLAY=${DISPLAY:-:0}
# cd "$(dirname "$0")"
# echo "Starting ProcessLoader from $(pwd)..."
# ./ProcessLoader
# RUNEOF
# chmod +x "${APP_DIR}/run.sh"

# echo ""
# echo "============================================="
# echo "  Setup Complete!"
# echo "============================================="
# echo ""
# echo "  Installed:"
# echo "    - .NET 8 runtime"
# echo "    - libgdiplus (System.Drawing support)"
# echo "    - Piper TTS + voice model"
# echo "    - OpenCV + OpenCvSharpExtern (if built)"
# echo ""
# echo "  To run ProcessLoader:"
# echo ""
# echo "    Option A — Directly:"
# echo "      cd ${APP_DIR}"
# echo "      export LD_LIBRARY_PATH=/usr/local/lib:${APP_DIR}:\$LD_LIBRARY_PATH"
# echo "      ./ProcessLoader"
# echo ""
# echo "    Option B — Using the run script:"
# echo "      ${APP_DIR}/run.sh"
# echo ""
# echo "    Option C — As a systemd service:"
# echo "      sudo systemctl start processloader"
# echo "      sudo systemctl status processloader"
# echo "      journalctl -u processloader -f    # view logs"
# echo ""
# echo "  Configuration files are in:"
# echo "      ${APP_DIR}/Configurations/"
# echo "============================================="

#!/bin/bash
# =============================================================================
# Build OpenCV + OpenCvSharpExtern for Raspberry Pi (ARM64)
#
# This script:
#   1. Installs build dependencies
#   2. Builds OpenCV 4.11.0 with FFmpeg support (no CUDA)
#   3. Builds the OpenCvSharpExtern native wrapper against it
#   4. Copies the resulting .so files to your app's publish directory
#
# Prerequisites:
#   - Raspberry Pi 4/5 with 64-bit Raspberry Pi OS (Debian 11 Bullseye / 12 Bookworm / 13 Trixie)
#   - cmake, git, build-essential
#
# Usage:
#   chmod +x rpi-build-opencv-and-wrapper.sh
#   sudo ./rpi-build-opencv-and-wrapper.sh [publish_dir]
#
#   publish_dir: Path to your dotnet publish output (default: ~/app)
#
# NOTE: This build takes a long time on RPi (~2-4 hours). Consider using
#       a swap file if your RPi has limited RAM:
#         sudo fallocate -l 4G /swapfile
#         sudo chmod 600 /swapfile && sudo mkswap /swapfile && sudo swapon /swapfile
# =============================================================================

set -e

PUBLISH_DIR="${1:-$HOME/app}"
OPENCV_VERSION="4.11.0"
BUILD_DIR="/tmp/opencv-build"

echo "============================================="
echo "RPi OpenCV + OpenCvSharpExtern Build Script"
echo "============================================="
echo "OpenCV version: ${OPENCV_VERSION}"
echo "Publish dir:    ${PUBLISH_DIR}"
echo ""

# -----------------------------------------------
# Step 0: Install dependencies
# -----------------------------------------------
echo "[Step 0] Installing build dependencies..."
apt-get update
apt-get install -y --no-install-recommends \
    build-essential cmake git pkg-config \
    libv4l-dev v4l-utils \
    libavcodec-dev libavformat-dev libswscale-dev \
    libgtk-3-dev libcanberra-gtk3-dev \
    libtbb-dev libjpeg-dev libpng-dev libtiff-dev \
    liblapack-dev libopenblas-dev \
    python3-dev python3-numpy

# libdc1394-dev may be named libdc1394-22-dev on Debian 11
apt-get install -y --no-install-recommends libdc1394-dev 2>/dev/null || \
    apt-get install -y --no-install-recommends libdc1394-22-dev 2>/dev/null || \
    echo "  WARNING: libdc1394 not available (IEEE 1394 camera support won't be built)"

# Check available RAM and warn if swap might be needed
TOTAL_RAM_MB=$(free -m | awk '/Mem:/{print $2}')
if [ "$TOTAL_RAM_MB" -lt 4000 ]; then
    echo ""
    echo "  WARNING: Only ${TOTAL_RAM_MB}MB RAM detected."
    echo "  OpenCV build may run out of memory. If the build fails, add swap:"
    echo "    sudo fallocate -l 4G /swapfile"
    echo "    sudo chmod 600 /swapfile && sudo mkswap /swapfile && sudo swapon /swapfile"
    SWAP_MB=$(free -m | awk '/Swap:/{print $2}')
    if [ "$SWAP_MB" -lt 2000 ]; then
        echo "  Current swap: ${SWAP_MB}MB (recommended: 4096MB+)"
    else
        echo "  Current swap: ${SWAP_MB}MB (should be sufficient)"
    fi
    echo ""
fi

# -----------------------------------------------
# Step 1: Build OpenCV from source
# -----------------------------------------------
echo ""
echo "[Step 1] Building OpenCV ${OPENCV_VERSION} with FFmpeg..."
echo "  (This may take 2-4 hours on Raspberry Pi — be patient)"
rm -rf "${BUILD_DIR}"
mkdir -p "${BUILD_DIR}"
cd "${BUILD_DIR}"

git clone --depth 1 --branch ${OPENCV_VERSION} https://github.com/opencv/opencv.git
git clone --depth 1 --branch ${OPENCV_VERSION} https://github.com/opencv/opencv_contrib.git

cd opencv
mkdir build && cd build

cmake -D CMAKE_BUILD_TYPE=RELEASE \
      -D CMAKE_INSTALL_PREFIX=/usr/local \
      -D OPENCV_EXTRA_MODULES_PATH=${BUILD_DIR}/opencv_contrib/modules \
      -D WITH_GSTREAMER=OFF \
      -D WITH_LIBV4L=ON \
      -D WITH_V4L=ON \
      -D WITH_FFMPEG=ON \
      -D ENABLE_NEON=ON \
      -D WITH_CUDA=OFF \
      -D BUILD_SHARED_LIBS=ON \
      -D OPENCV_GENERATE_PKGCONFIG=ON \
      -D BUILD_opencv_python3=OFF \
      -D BUILD_TESTS=OFF \
      -D BUILD_PERF_TESTS=OFF \
      -D BUILD_EXAMPLES=OFF \
      -D BUILD_opencv_java=OFF \
      ..

make -j$(nproc)
make install
ldconfig

echo ""
echo "--- Verifying OpenCV build info ---"
python3 -c "import cv2; print(cv2.getBuildInformation())" 2>/dev/null || true
echo ""
pkg-config --modversion opencv4 && echo "OpenCV installed via pkg-config" || echo "Warning: pkg-config not finding opencv4"

# -----------------------------------------------
# Step 2: Build OpenCvSharpExtern wrapper
# -----------------------------------------------
echo ""
echo "[Step 2] Building OpenCvSharpExtern native wrapper..."
cd "${BUILD_DIR}"

# Clone OpenCvSharp matching the NuGet version used in the project (4.11.x)
# Use the tag that matches your OpenCvSharp4 NuGet package version
git clone --depth 1 --branch 4.11.0.20250507 https://github.com/shimat/opencvsharp.git 2>/dev/null || \
git clone --depth 1 https://github.com/shimat/opencvsharp.git

cd opencvsharp

mkdir build && cd build

cmake \
    -D CMAKE_BUILD_TYPE=Release \
    -D CMAKE_INSTALL_PREFIX=/usr/local \
    -D OpenCV_DIR=/usr/local/lib/cmake/opencv4 \
    ../src

make -j$(nproc)

echo ""
echo "--- Built libraries ---"
find . -name "*.so" -type f
echo ""

# -----------------------------------------------
# Step 3: Install and copy libraries
# -----------------------------------------------
echo ""
echo "[Step 3] Installing libraries..."

# Copy the OpenCvSharpExtern wrapper to the publish directory
EXTERN_SO=$(find . -name "libOpenCvSharpExtern.so" -type f | head -1)
if [ -z "$EXTERN_SO" ]; then
    echo "ERROR: libOpenCvSharpExtern.so not found in build output!"
    exit 1
fi

mkdir -p "${PUBLISH_DIR}"
cp -v "$EXTERN_SO" "${PUBLISH_DIR}/libOpenCvSharpExtern.so"

# Also install system-wide
cp -v "$EXTERN_SO" /usr/local/lib/libOpenCvSharpExtern.so
ldconfig

echo ""
echo "============================================="
echo "Build complete!"
echo ""
echo "Files deployed to: ${PUBLISH_DIR}"
echo "  - libOpenCvSharpExtern.so (OpenCvSharp native wrapper)"
echo ""
echo "OpenCV libraries are installed to /usr/local/lib/"
echo ""
echo "IMPORTANT: Make sure LD_LIBRARY_PATH includes /usr/local/lib"
echo "  export LD_LIBRARY_PATH=/usr/local/lib:\$LD_LIBRARY_PATH"
echo "============================================="

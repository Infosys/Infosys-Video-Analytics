# Deployment Guide — Linux ARM64 (NVIDIA Jetson & Raspberry Pi)

This guide covers deploying IVA on ARM64 Linux devices. IVA supports cross-platform deployment on edge devices — the pipeline automatically detects the platform at runtime and selects the appropriate video backends.

## Supported Hardware

| Device | OS | CUDA | Tested |
|--------|----|------|--------|
| **NVIDIA Jetson Orin Nano** | JetPack 5.x (Ubuntu 20.04) / 6.x (Ubuntu 22.04) | Yes (11.4 / 12.2+) | Yes |
| **NVIDIA Jetson Xavier** | JetPack 5.x / 6.x | Yes | Yes |
| **Raspberry Pi 4** (64-bit) | Raspberry Pi OS (Debian 11/12/13) | No | Yes |
| **Raspberry Pi 5** (64-bit) | Raspberry Pi OS (Debian 12/13) | No | Yes |

## Prerequisites

| Component | Jetson | Raspberry Pi |
|-----------|--------|-------------|
| **.NET 8.0 Runtime** | ARM64 build from [Microsoft .NET](https://dotnet.microsoft.com/download) | ARM64 build from [Microsoft .NET](https://dotnet.microsoft.com/download) |
| **FFmpeg** | `sudo apt install ffmpeg` | `sudo apt install ffmpeg` |
| **Python >= 3.10** | Required for local inference | Required for local inference |
| **CUDA** | Comes with JetPack | N/A |
| **OpenCvSharp native lib** | Must be built from source (see below) | Must be built from source (see below) |

> **Note:** There is no pre-built NuGet runtime package for OpenCvSharp on ARM64. The native `libOpenCvSharpExtern.so` must be compiled on the target device.

## Quick Start — Automated Scripts

The `Scripts/` folder contains ready-to-use setup and build scripts for each platform:

```
Scripts/
├── jetson/
│   ├── jetson-build-opencv-and-wrapper.sh    # Builds OpenCV 4.11 + OpenCvSharpExtern with CUDA
│   └── jetson-setup-and-deploy.sh            # Full setup: .NET 8, OpenCV build, deploy
└── rpi/
    ├── rpi-build-opencv-and-wrapper.sh       # Builds OpenCV 4.11 + OpenCvSharpExtern (no CUDA)
    └── rpi-setup-and-deploy.sh               # Full setup: .NET 8, OpenCV build, deploy
```

### NVIDIA Jetson — Full Setup

```bash
# Clone the repo (or copy the publish output + Scripts folder to the Jetson)
git clone https://github.com/Infosys/Infosys-Video-Analytics.git
cd Infosys-Video-Analytics

# Run the full setup script (installs .NET 8, builds OpenCV + wrapper, deploys)
chmod +x Scripts/jetson/jetson-setup-and-deploy.sh
sudo ./Scripts/jetson/jetson-setup-and-deploy.sh /path/to/app

# Or just build OpenCV + wrapper if .NET is already installed
chmod +x Scripts/jetson/jetson-build-opencv-and-wrapper.sh
sudo ./Scripts/jetson/jetson-build-opencv-and-wrapper.sh /path/to/app
```

The Jetson build script:
1. Installs build dependencies (cmake, GStreamer, V4L2, FFmpeg dev libs)
2. Builds OpenCV 4.11.0 with **FFmpeg + GStreamer + CUDA** support
3. Builds the `libOpenCvSharpExtern.so` native wrapper
4. Copies the `.so` files to your app's publish directory

### Raspberry Pi — Full Setup

```bash
# Clone the repo (or copy the publish output + Scripts folder to the RPi)
git clone https://github.com/Infosys/Infosys-Video-Analytics.git
cd Infosys-Video-Analytics

# Run the full setup script
chmod +x Scripts/rpi/rpi-setup-and-deploy.sh
sudo ./Scripts/rpi/rpi-setup-and-deploy.sh /path/to/app

# Or just build OpenCV + wrapper
chmod +x Scripts/rpi/rpi-build-opencv-and-wrapper.sh
sudo ./Scripts/rpi/rpi-build-opencv-and-wrapper.sh /path/to/app
```

> **Warning:** The OpenCV build takes **2–4 hours** on Raspberry Pi. If your device has limited RAM, create a swap file first:
> ```bash
> sudo fallocate -l 4G /swapfile
> sudo chmod 600 /swapfile && sudo mkswap /swapfile && sudo swapon /swapfile
> ```

## Manual Setup

If you prefer to set things up manually instead of using the scripts:

### 1. Build and Publish the Application

From a development machine or on the device itself:

```bash
cd IVA_Pipeline
dotnet publish ProcessLoader/ProcessLoader.csproj -c Release -r linux-arm64 --self-contained
```

Output: `ProcessLoader/bin/Release/net8.0/linux-arm64/publish/`

### 2. Build OpenCvSharp Native Library

See the platform-specific build scripts in `Scripts/jetson/` or `Scripts/rpi/` for the full build steps, or build manually:

```bash
# Install build dependencies
sudo apt install cmake build-essential libgtk-3-dev pkg-config \
    libavcodec-dev libavformat-dev libswscale-dev \
    libv4l-dev v4l-utils libjpeg-dev libpng-dev libtiff-dev

# Clone and build
git clone --depth 1 -b 4.11.0 https://github.com/opencv/opencv.git /tmp/opencv
git clone --depth 1 -b 4.11.0 https://github.com/opencv/opencv_contrib.git /tmp/opencv_contrib
git clone https://github.com/shimat/opencvsharp.git /tmp/opencvsharp

# Build OpenCV
mkdir -p /tmp/opencv/build && cd /tmp/opencv/build
cmake -D CMAKE_BUILD_TYPE=Release \
      -D OPENCV_EXTRA_MODULES_PATH=/tmp/opencv_contrib/modules \
      -D WITH_FFMPEG=ON -D WITH_V4L=ON ..
make -j$(nproc) && sudo make install

# Build OpenCvSharpExtern
cd /tmp/opencvsharp/src
mkdir build && cd build
cmake -D CMAKE_BUILD_TYPE=Release ..
make -j$(nproc)

# Copy to publish output
cp OpenCvSharpExtern/libOpenCvSharpExtern.so /path/to/published/app/
```

### 3. Deploy and Run

```bash
# Copy the publish output to the device (if built elsewhere)
scp -r publish/* user@jetson-ip:/home/user/app/

# On the device
cd /home/user/app
chmod +x ProcessLoader
./ProcessLoader
```

## Platform-Specific Behavior

IVA automatically detects `linux-arm64` at runtime and adjusts video capture behavior. No configuration flags are needed.

| Feature | Windows / Linux x64 | Linux ARM64 (Jetson / RPi) |
|---------|---------------------|----------------------------|
| **File video capture** | Default backend (auto) | FFmpeg backend (forced) |
| **RTSP / HTTP streams** | Default backend (auto) | FFmpeg backend (forced) |
| **Local camera** | Default backend (auto) | V4L2 (implicit via device index) |
| **Rendering** | OpenCvSharp | OpenCvSharp |
| **Color system** | `ColorHelper` (no System.Drawing) | `ColorHelper` (no System.Drawing) |

Detection is done at startup via:
```csharp
RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.Arm64
```

## Environment Variables

Set these environment variables to configure the pipeline on the edge device:

### Required

```bash
export DEVICE_ID="jetson-01"
export TENANT_ID="1"
export CAMERA_URL="rtsp://your-camera-ip/stream"   # or "0" for /dev/video0
export VIDEO_FEED_TYPE="LIVE"                        # LIVE, OFFLINE, or IMAGE
export FFMPEG_EXE_FILE="/usr/bin/ffmpeg"
```

### Python Inference

```bash
export PYTHONVIRTUALPATH="/home/user/venv"
export PYTHONVERSION="/home/user/venv/lib/libpython3.11.so"
```

> Use Python >= 3.10 on Linux. Python 3.9 requires additional Windows-style path setup that is not compatible with Linux.

### Optional

```bash
export OFFLINE_VIDEO_DIRECTORY="/home/user/videos"
export VIDEO_FORMATS_ALLOWED=".mp4,.avi,.mkv"
export DB_ENABLED="false"                            # true to use database for config
export PREDICTION_MODEL="your_model_name"
export FFMPEG_ARGUMENTS="-y -f rawvideo -pix_fmt bgr24 -s {0}x{1} -r 25 -i - -c:v libx264 -preset ultrafast -f flv output.flv"
```

For the full list of supported environment variables, see the `UpdateConfigValues()` method in `TaskRoute.cs`.

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `libOpenCvSharpExtern.so` not found | Ensure the `.so` file is in the same directory as `ProcessLoader` or in `LD_LIBRARY_PATH` |
| `libgdiplus` errors | Install via `sudo apt install libgdiplus`. Note: IVA v3.7 uses `ColorHelper` instead of `System.Drawing` but some transitive dependencies may still reference it |
| Video capture returns empty frames | Verify FFmpeg is installed and `/dev/video0` exists for local cameras. Check `VIDEO_FEED_TYPE` is set correctly |
| Python inference fails | Ensure `PYTHONVIRTUALPATH` and `PYTHONVERSION` point to valid paths. Use Python >= 3.10 |
| Slow OpenCV build on RPi | Create a swap file (see Quick Start section above). Build takes 2–4 hours on RPi 4 |
| CUDA not detected on Jetson | Ensure JetPack is fully installed. Run `nvcc --version` to verify CUDA toolkit |

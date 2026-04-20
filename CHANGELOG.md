# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [3.7] - 2026-04-20

### Added

#### ROI (Region of Interest) Processor
- New pipeline node (`ROI`) for extracting and processing specific regions from video frames before AI inference.
- Supports three shape types: Polygon, Circle, and Irregular (pixel-level masks).
- Three ROI processing modes:
  - **Type 1**: Extract individual ROIs as separate cropped images for independent prediction.
  - **Type 2**: Combine all ROI regions into a single composite image.
  - **Type 3**: Blackout non-ROI areas in the original frame.
- Debug image output support for ROI visualization.

#### Data Collector Framework
- Pluggable, file-based data collection system with factory pattern (`DataCollectorFactory`).
- JSONL writer with in-memory buffering and automatic flush on last frame.
- New `DataCollectorProcess` pipeline node (`LDCO`) for collecting inference data to local storage.
- Configurable DB provider and output filename via environment variables.

#### Cross-Platform Support (NVIDIA Jetson / Linux ARM64)
- Runtime detection of `linux-arm64` (NVIDIA Jetson) with automatic video backend selection.
- FFmpeg backend for file-based video capture and V4L2 for local cameras on ARM64.
- Dedicated helper methods: `CreateVideoCaptureForFile()`, `CreateVideoCaptureForLive()`.

#### Cross-Platform Color System
- New `ColorHelper` class replacing `System.Drawing.Color` with OpenCvSharp `Scalar` (BGR format).
- Contains 140+ named colors for cross-platform rendering without `System.Drawing` dependency.

#### Split Screen Rendering
- When using Multi-Model chaining, individual model inference results can now be viewed separately via split screen rendering within a single video output.
- Enables easier visual comparison, faster debugging, model A/B evaluation, and better operational monitoring.

#### Semi-Transparent Prediction Overlays
- Prediction overlays (labels, bounding boxes, descriptions) can now be rendered semi-transparent so the underlying video remains visible.
- Improves observability when overlays are large or dense.

### Changed

#### Enhanced Object Detection API
- `Base_64` now supports a list of images (multiple ROI images per frame).
- PCD (Point Cloud Data) support added to inference requests.
- XAI parameters (`Xai_ver`, `Xai_explainers`, `Xai_url`) included in prediction requests.
- Model chaining: forwards previous predictions (`Fs`) to the next model in the chain.
- ROI coordinates (`Roi_c`) and HyperParameters (`Hp`) support in inference requests.
- `TaskType` field added per prediction for multi-task tracking.

#### Enhanced TaskRoute & Device Configuration
- Device configurations can now be driven from environment variables, enabling flexible deployment across environments.
- Unified configuration assignment with environment variable injections into `DeviceDetails`.

### Fixed
- **Multi-Model Rendering**: Only the last model's predictions were shown in the rendered output when using multiple models. All model results are now rendered correctly.

### Architecture
- **System.Drawing → OpenCvSharp**: Full migration to OpenCvSharp for rendering, eliminating Windows-only dependency.
- **Factory Pattern for Data Collection**: `DataCollectorFactory` creates instances via reflection from configured provider name.
- **Environment Variable Injection Layer**: All configuration can be overridden via environment variables through LIF adapters.
- **Multi-Model Chaining**: Multiple FrameProcessor channels + DataAggregator enable sequential/parallel model execution.
- **Secret Management**: Both Azure Key Vault and AWS Secrets Manager supported for credential storage.

[3.7]: https://github.com/Infosys/Infosys-Video-Analytics/compare/3.5...3.7
[3.5]: https://github.com/Infosys/Infosys-Video-Analytics/releases/tag/3.5

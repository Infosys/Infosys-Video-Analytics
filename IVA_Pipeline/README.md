# Infosys Video Analytics (IVA)

**Infosys Video Analytics (IVA)** is an enterprise-grade video processing and AI inference platform that enables real-time and batch video analytics using configurable pipelines, Python-based model execution, and API-driven inference.

Developed by Infosys, IVA supports scalable deployment, container orchestration, and production-grade monitoring.

---

# Table of Contents

* Overview
* Key Features
* System Architecture
* Architecture Diagram
* Technology Stack
* Prerequisites
* Installation
* Configuration
* Configuration Examples
* Running IVA
* Testing
* Deployment Guide

  * Docker Deployment
  * Kubernetes Deployment
* Production Deployment Topology
* Load Balancing Architecture
* Monitoring & Logging Architecture
* Observability
* Scalability Strategy
* Environment Configuration (.env Strategy)
* CI/CD Pipeline
* Release Management Workflow
* Project Structure
* Security Best Practices
* Performance Optimization
* Troubleshooting
* Contributing
* License

---

# Overview

IVA provides:

* Video ingestion and frame capture
* AI model inference (local or API-based)
* Python execution pipeline
* Real-time processing capability
* Scalable deployment architecture
* Video output generation using FFmpeg
* Enterprise monitoring and observability

---

# Key Features

* Modular pipeline architecture
* Local Python inference support
* External API integration
* Containerized deployment
* Horizontal scaling support
* Production monitoring integration
* Configurable processing workflows

---

# System Architecture

## Core Components

### FrameGrabber

* Captures frames from input video streams.

### Process Loader

* Manages pipeline execution.
* Loads configuration.
* Controls inference workflow.

### Prediction Engine

* Executes AI models locally or through APIs.

### Python Model Executor

* Entry point for Python-based inference.

### Media Processing Layer

* Uses FFmpeg for encoding and output generation.

---

# Architecture Diagram

```
Input Video
     |
     v
FrameGrabber
     |
     v
Process Loader
     |
+----+----------------+
|                     |
v                     v
Local Python      External API
Inference         Inference
     |                 |
     +--------+--------+
              |
              v
      Prediction Engine
              |
              v
           FFmpeg
              |
              v
        Output Video
```

---

# Technology Stack

* **Backend Runtime** — .NET 8 SDK from Microsoft
* **Inference** — Python ≥ 3.9
* **Video Processing** — FFmpeg
* **Database** — PostgreSQL (Npgsql)
* **Containerization** — Docker
* **Orchestration** — Kubernetes

---

# Prerequisites

## System Requirements

* Windows 10+ or Windows Server 2012 R2+
* .NET 8 SDK
* Python 3.9+
* FFmpeg installed and added to PATH

## Required Tools

* Git — [https://git-scm.com](https://git-scm.com)
* 7-Zip — [https://www.7-zip.org](https://www.7-zip.org)
* VLC — [https://www.videolan.org/vlc](https://www.videolan.org/vlc)

---

# Installation

```bash
git clone <repository-url>
cd <repository-folder>
dotnet restore
dotnet build
```

---

# Configuration

Configuration files are located in:

```
ProcessLoader/Configuration
```

| File             | Purpose                        |
| ---------------- | ------------------------------ |
| LiSettings.json  | LIF adapter configuration      |
| Device.json      | Device and model settings      |
| Process.Config   | Pipeline execution rules       |
| config.ini       | Python execution configuration |
| ModelType.xml    | Model API configuration        |
| Appsettings.json | Environment settings           |

---

# Configuration Examples

## Device.json

```json
{
  "deviceId": "camera01",
  "predictionModel": "object_detection",
  "inputPath": "/data/input",
  "outputPath": "/data/output"
}
```

---

## Appsettings.json

```json
{
  "ConfigurationPath": "/config/device.json",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=iva"
  }
}
```

---

## ModelType.xml

```xml
<ModelTypes>
  <Model name="object_detection">
    <ApiEndpoint>http://localhost:8000/predict</ApiEndpoint>
  </Model>
</ModelTypes>
```

---

# Running IVA

```bash
processloader.exe
```

Output:

```
pd.flv
```

---

# Testing

Model inference library:

[https://github.com/Infosys/Infosys-Model-Inference-Library](https://github.com/Infosys/Infosys-Model-Inference-Library)

Supports:

* Local Python inference
* API-based inference

---

# Deployment Guide

---

## Docker Deployment

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build --configuration Release
ENTRYPOINT ["dotnet","ProcessLoader.dll"]
```

### Build Image

```bash
docker build -t iva .
```

### Run Container

```bash
docker run -p 5000:5000 iva
```

---

## Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: iva
spec:
  replicas: 3
  selector:
    matchLabels:
      app: iva
  template:
    metadata:
      labels:
        app: iva
    spec:
      containers:
      - name: iva
        image: iva:latest
        ports:
        - containerPort: 5000
```

---

# Production Deployment Topology

Typical enterprise deployment:

```
Users → Load Balancer → API Gateway → IVA Pods
                                 |
                          Inference Services
                                 |
                           Database + Storage
```

### Components

* Load balancer
* API gateway
* Kubernetes cluster
* Distributed storage
* Monitoring services
* Logging services

---

# Load Balancing Architecture

IVA supports multiple load balancing strategies:

### Layer 7 Load Balancing

* HTTP routing
* API gateway routing
* Session management

### Reverse Proxy Load Balancing

* NGINX / ingress controller
* Traffic distribution across pods

### Benefits

* High availability
* Fault tolerance
* Traffic distribution
* Zero downtime deployments

---

# Monitoring & Logging Architecture

IVA integrates enterprise monitoring and logging pipelines.

## Monitoring Stack

* Metrics collection via Prometheus
* Dashboard visualization via Grafana

```
IVA Services → Metrics Exporter → Prometheus → Grafana
```

---

## Logging Stack (ELK)

* Elasticsearch — log storage
* Logstash — log processing
* Kibana — visualization

```
IVA Services → Logstash → Elasticsearch → Kibana
```

---

# Observability

IVA provides full observability through:

### Metrics

* CPU usage
* Memory usage
* Frame processing rate
* Inference latency

### Logs

* Pipeline execution logs
* Error tracking
* Model execution logs

### Tracing

* Request lifecycle tracking
* Performance bottleneck detection

---

# Scalability Strategy

IVA supports horizontal and vertical scaling.

## Horizontal Scaling

* Multiple process loader instances
* Kubernetes auto-scaling
* Distributed inference services

## Vertical Scaling

* Increase compute resources
* GPU acceleration
* Memory optimization

## Performance Scaling Techniques

* Batch inference
* Caching predictions
* Distributed processing

---

# Environment Configuration (.env Strategy)

Sensitive configuration should be stored in environment variables.

## Example `.env`

```
DB_HOST=localhost
DB_USER=postgres
DB_PASSWORD=password
MODEL_API_URL=http://localhost:8000
INPUT_PATH=/data/input
OUTPUT_PATH=/data/output
```

## Best Practices

* Never commit `.env` files.
* Use secret managers.
* Use environment-specific configs.

---

# CI/CD Pipeline

## Example GitHub Actions Workflow

```
.github/workflows/build.yml
```

```yaml
name: IVA Build Pipeline

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3
    - uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - run: dotnet restore
    - run: dotnet build --configuration Release
```

---

# Release Management Workflow

IVA follows semantic versioning.

## Versioning Strategy

```
MAJOR.MINOR.PATCH
```

### Release Process

1. Feature development branch
2. Code review
3. CI validation
4. Version tagging
5. Release deployment
6. Production rollout

### Deployment Strategies

* Blue/Green deployment
* Rolling updates
* Canary releases

---

# Project Structure

```
IVA/
├── FrameGrabber/
├── ProcessLoader/
├── Prediction/
├── Configuration/
├── Models/
├── Scripts/
└── Tests/
```

---

# Security Best Practices

* Use secure API endpoints
* Protect credentials via environment variables
* Validate inputs
* Enable logging and monitoring
* Restrict access to inference APIs

---

# Performance Optimization

* GPU-enabled inference
* FFmpeg encoding optimization
* Distributed deployment
* Efficient frame processing

---

# Troubleshooting

## Build Issues

* Verify .NET installation.
* Run `dotnet restore`.

## FFmpeg Issues

* Ensure FFmpeg added to PATH.

## Python Errors

* Verify Python ≥ 3.9.
* Install dependencies.

---

# Contributing

## Development Workflow

1. Fork repository.
2. Create feature branch.
3. Implement changes.
4. Add tests.
5. Submit pull request.

## Pull Request Requirements

* Clear description
* Tests included
* Pass CI pipeline
* Follow coding standards

---

# License

See LICENSE file.

# Infosys Video Analytics

### Introduction

Infosys Video Analytics(IVA) is constructed using .NET 8.0. It reads frames from a video file and makes an API call to the Python Inference API pipeline by sending a JSON request (refer to [References](#-References) section). The response from the Python Inference pipeline is then used to render the output video. This tool is designed to facilitate video analysis and processing using advanced machine learning models.

IVA(Infosys IP) offers a flexible, vendor-neutral approach for end-to-end Computer Vision services. It supports a range of AI models, including cutting-edge next-gen Generative AI, pre-configured for diverse vision tasks. Additionally multiple hardware like NVidia Jetson, Raspberry, Drone etc. are supported with IVA. IVA seamlessly blends on-premise, Cloud, Edge, AI & Vision technologies.

The current release of Infosys Video Analytics (IVA) is version 3.5, which has been upgraded to use .NET 8.0, whereas the previous release was based on .NET 6.0. This upgrade brings enhanced performance, improved compatibility, and access to the latest features of the .NET 8.0 framework. The documentation reflects this change, highlighting the transition to the newer framework as part of the ongoing improvements in IVA.

## Table of Contents
- [Architecture](#-Architecture)
- [Workflow](#-Workflow)
- [Features](#-New-Features)
- [New Features in Release IVA-3.5](#-New-Features-in-Release-IVA-3.5)
- [Technical Enhancements with .NET 8.0](#-Technical-Enhancements-with-.NET-8.0)
- [References](#-References)

### Architecture

![alt text](Docs/Architecturepic.png)

- **Framework**: IVA is constructed using .NET 8.0.
- **Data Flow**:

  - Reads frames from a video/image file.
  - Sends a JSON request to the Python Inference API pipeline. (refer to [References](#-References) section for sample JSON request and response structures)
  - Receives the response and renders the output video.
- **Components**:

  - **Frame Grabber**: Captures frames from the input video/image.
  - **Frame Predictor**: Processes frames using the Python model API.
  - **Data Collector**: Collects data for further processing.
  - **Frame Renderer**: Renders the processed frames into a video.
    Data Collector
  - This module stores AI prediction metadata to the database. It can be used for downstream applications, analytics and notification purposes.
  - It comprises of SQL Server database (for persistent storage), Blob storage (for temporary storage), Queue- MSMQ, Kafka (for transient storage of messages), disk storage. SQL stores structured data like prediction results from AI models, Blob stores image frames, Queue stores messages which are transmitted through various modules of pipeline, disks can store image, videos.
    Rendering:
  - The rendering part is modified as per the new schema for the models and conditions for executing specific models are specified in the configuration file based on that model is executed.
  - Multiple attributes are set for the models based on that it will render model and generate the output video.

- **Workflow**:

  1. Upload the image/video to IVA.
  2. The Frame Grabber captures frames and transmits them to the Frame Predictor.
  3. The Frame Predictor forwards the data to the Data Collector and Frame Renderer.
  4. The rendered video output can be viewed using VLC media player.


### Features

#### Real-Time Video Processing

- **Live Video Feed**: IVA can process live video feeds from a camera. Configure `CAMERA_URL` to the camera's URL or set it to `0` for the default camera.
- **Streaming Options**: Supports various streaming options, including RTSP and HTTP streams.

#### Model Integration

- **Custom Models**: Integrate custom machine learning models by updating the `ModelType.xml` and `PythonModelExecutor.py` files.
- **Model Switching**: Easily switch between different models by modifying the `PREDICTION_MODEL` attribute in `device.json`.

#### Advanced Configurations

- **Frame Rate Control**: Adjust the frame rate for processing by setting the `FRAMETOPREDICT` attribute in `device.json`.
- **Output Formats**: Supports multiple output formats, including FLV, MP4, and AVI. Configure the output format in `appsettings.json`.

#### Logging and Debugging

- **Verbose Logging**: Enable verbose logging for detailed debugging information. Modify the logging level in `appsettings.json`.
- **Error Handling**: Comprehensive error handling mechanisms to ensure robust performance. Check log files for error details.

#### Performance Optimization

- **GPU Acceleration**: Leverage GPU acceleration for faster processing. Ensure the necessary CUDA libraries are installed and configured.
- **Batch Processing**: Process multiple videos or images in batch mode. Configure batch processing settings in `device.json`.

#### User Interface

- **Web Dashboard**: IVA includes a web-based dashboard for monitoring and controlling the processing pipeline. Access the dashboard via the specified URL in `appsettings.json`.

#### Security

- **Authentication**: Implement authentication mechanisms to secure the API endpoints. Configure authentication settings in `appsettings.json`.
- **Data Encryption**: Ensure data privacy by enabling encryption for data transmission. Modify encryption settings in the configuration files.

### New Features in Release IVA-3.5

#### IVA Core pipeline features -

#### IMIL Library 

This library will helps in testing the local python inference with IVA for different models. 
- IMIL library is used to test local python inference testing with IVA, as its easy to deploy, single library for local invocation with provision for adding custom models, we need the library folder with the help of that the iva will run the local python execution.

#### PCD Handling 

This component is independent and also helps in reading PCD – Point Cloud Data files in any format.
- Prompt handler enables usage of prompts in IVA pipeline. IVA will be able to use models that can accept prompt as input with prompt handler. Prompts can be provided before the start of the process or at runtime in a text file. 
- Prompt Handler node is independent and can read prompts from a text file giving the ability to allow other inputs with prompt like video or image. Using prompt templates, Prompt handler will structure the prompt in a format which can be accepted/read by the model as input. 
- Prompt Injector is used to read the prompt from an external source, and this is done using LIF adapters. An external source will send a prompt to Kafka server which prompt injector is listens to via adapters. It reads the prompt from Kafka server and injects it into prompt handler. The prompt handler further uses the prompt to process, format it is using a prompt template defined.  

#### Explainability 

This component that is responsible for handling Explainability for the predicted frames from AI Predictor and Save the data into database. 
- Explainability support is provided for the predicted frame from AI predictor node, based on the template configured in the Database and Device.json. It supports explainability based on the confidence score, label etc., based on the template configured in the database.

#### Data Aggregator 

This component handles the messages from multiple frame processor nodes and aggregates the predicted data as per the frame id and forwards it to the next node in the pipeline. 

#### Frame Viewer: 

Frame Viewer is a new component added in IVA to perform any kind of operation on raw data using ffmpeg. 

#### Prompt Handling: 
- Prompt handler enables usage of prompts in IVA pipeline. IVA will be able to use models that can accept prompt as input with prompt handler. Prompts can be provided before the start of the process or at runtime in a text file. 
- Prompt Handler node is independent and can read prompts from a text file giving the ability to allow other inputs with prompt like video or image. Using prompt templates, Prompt handler will structure the prompt in a format which can be accepted/read by the model as input. 
- Prompt Injector is used to read the prompt from an external source, and this is done using LIF adapters. An external source will send a prompt to Kafka server which prompt injector is listens to via adapters. It reads the prompt from Kafka server and injects it into prompt handler. The prompt handler further uses the prompt to process, format it is using a prompt template defined. 

#### Multi Model Chaining: 
- Model chaining feature lets us use IVA to run multiple AI models that can be configured in the pipeline either in sequence or in parallel. In this release only frame processor supports model chaining feature. 
- With model chaining, IVA has the capability to merge the inference results from multiple models configured in the pipeline. Inference data of one model can be sent as input to another model. Data can be merged and utilized to increase consistency and accuracy.

#### LIF features -

#### Secret Configurations: 
- IVA configurations which are sensitive or need to be secured can be stored in Azure or AWS services. These stored configurations or information can be retrieved using LIF adapters which include the support for both Azure and AWS. 

#### Rabbit MQ:  
- Rabbit MQ is an open-source message/event streaming platform. Configured to send messages across components of IVA. Can also be configured to send messages out of IVA as they are stored in topics and accessed by the applications/components or external sources. 

#### Azure IOT:  
- Azure Iot Adapter is configured to send messages across components of IVA. It uses Azure IoT Hub as a medium to receive messages and store in Azure Queue which can be accessed by the applications/components or external sources.

#### Kafka:  
- Kafka is an open-source message/event streaming platform. Configured to send messages across components of IVA. Can also be configured to send messages out of IVA as they are stored in topics and accessed by the applications/components or external sources. 

### Storage Services 

- Windows MSMQ [works only on windows] 
- IIS 8.5 and above [works only on windows] 
- Kafka 3.1.x and above 
- Java 7.1.x and above 
- Microsoft SQL Server database [optional, required only when metadata store is needed] 
- Elasticsearch 8.8.1 
- PostgreSQL 14 

### Technical Enhancements with .NET 8.0

- **Performance Improvements**:
  - .NET 8.0 introduces significant performance optimizations, including faster Just-In-Time (JIT) compilation and reduced memory overhead, which enhance IVA's video processing capabilities.

- **Native AOT (Ahead-of-Time Compilation)**:
  - IVA leverages .NET 8.0's Native AOT feature to produce highly optimized, self-contained executables, reducing startup time and runtime memory usage.

- **Enhanced JSON Serialization**:
  - The System.Text.Json library in .NET 8.0 offers improved performance and new features, such as support for polymorphic serialization, which simplifies handling complex JSON structures in IVA's API interactions.

- **Improved Asynchronous Programming**:
  - .NET 8.0 enhances asynchronous programming with better task scheduling and reduced thread contention, ensuring smoother handling of concurrent video frame processing.

- **Span<T> and Memory<T> Enhancements**:
  - IVA benefits from .NET 8.0's improved support for `Span<T>` and `Memory<T>` types, enabling efficient memory management and faster frame data manipulation.

- **Cross-Platform Compatibility**:
  - .NET 8.0 extends cross-platform support, ensuring IVA runs seamlessly on Windows, Linux, and macOS environments, enabling deployment flexibility.

- **Cloud-Native Features**:
  - With .NET 8.0's integration of cloud-native libraries, IVA can easily integrate with Azure services for storage, AI model hosting, and analytics.

- **Improved Diagnostics and Observability**:
  - .NET 8.0 introduces enhanced diagnostic tools, such as better logging, tracing, and metrics collection, enabling easier debugging and performance monitoring of IVA.

- **Support for Modern C# Features**:
  - IVA utilizes the latest C# 12 features available in .NET 8.0, such as primary constructors and collection expressions, to simplify code and improve maintainability.

- **Security Enhancements**:
  - .NET 8.0 includes updated cryptographic APIs and improved TLS support, ensuring secure communication between IVA and the Python Inference API pipeline.

- **Native Support for ARM64**:
  - .NET 8.0 provides native support for ARM64 architecture, enabling IVA to run efficiently on devices like NVidia Jetson and Raspberry Pi.

- **Improved Dependency Injection**:
  - The dependency injection (DI) container in .NET 8.0 has been optimized for better performance and flexibility, simplifying IVA's modular architecture.

- **Blazor Integration**:
  - .NET 8.0 enhances Blazor capabilities, allowing IVA to integrate with web-based dashboards for real-time video analytics visualization.

These enhancements make IVA more robust, efficient, and scalable, leveraging the full potential of .NET 8.0. This IVA is ideal for both new users and experienced developers seeking specific technical details or support.

📚 ### References:

For sample input schema examples, please read[Docs/IVA-Input_Schema.md](Docs/IVA-Input_Schema.md)
For sample output schema, please read [Docs/IVA-Output_Schema.md)](Docs/IVA-Output_Schema.md)


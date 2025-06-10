# Infosys Video Analytics

**Infosys Video Analytics (IVA)** is a next-generation platform designed to deliver intelligent media and sensor data processing through seamless integration with **AI, Cloud, Edge, and Vision technologies**. It supports a wide range of input types—including **video, images, sensor data, and point cloud data**—making it a versatile solution for real-world applications.

Infosys Video Analytics seamlessly blends Cloud, Edge, AI & Vision technologies. Positioned at convergence of these cutting -edge advancements, IVA offers a flexible, vendor-neutral approach to manage hardware and software resources for end-to-end Computer Vision App services. The enterprise-ready Reference implementations caters to industries from retail to entertainment. 

## Why IVA?

IVA sits at the **crossroads of Cloud, Edge, AI, and Vision**, helping traditional enterprises transition into the digital age. Despite the transformative potential of these technologies, they remain underutilized. IVA bridges this gap with a **comprehensive, enterprise-ready solution**. Elevate your work with IVA - the bridge from traditional to digital.

## Table of Contents
- [New Features in Release IVA-3.5](#-New-Features-in-Release-IVA-3.5)
- [References](#-References)

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

📚 ### References:

For sample input schema examples, please read[Docs/IVA-Input_Schema.md](Docs/IVA-Input_Schema.md)
For sample output schema, please read [Docs/IVA-Output_Schema.md)](Docs/IVA-Output_Schema.md)


# LIF

## Introduction
This project provides a set of adapters for integrating various messaging and storage systems with the IVA core application. Each adapter facilitates seamless communication between the core pipeline and the respective external system.

## Adapters

| Adapter | Description |
|---------|-------------|
| **Kafka** | Open-source message/event streaming platform for inter-component communication. |
| **RabbitMQ** | Open-source message broker for reliable message delivery. |
| **MSMQ** | Microsoft Message Queuing (Windows only). |
| **Azure IoT** | Azure IoT Hub integration for message ingestion and Azure Queue access. |
| **AWS S3** | Amazon S3 document storage adapter. |
| **AWS Secrets Manager** | Retrieves sensitive configuration from AWS Secrets Manager. |
| **Azure Key Vault** | Retrieves secrets and configuration from Azure Key Vault. |
| **Environment Adapter** | Injects environment variables as configuration into the pipeline. |
| **Memory Doc Adapter** | In-memory document cache adapter. |
| **Memory Queue Adapter** | In-memory queue adapter for local/testing scenarios. |

## Secret Management
IVA configurations that are sensitive or need to be secured can be stored in cloud services:
- **Azure Key Vault** — via `AzureVaultAdapter`
- **AWS Secrets Manager** — via `AWSSecretsAdapter`

These adapters retrieve stored secrets at runtime, aligning with enterprise best practices for credential management.

# Azure IoT Hub Swiss Army Knife

This repository an application that exercies a number of different concepts within the Azure IoT suite including IoT Hub, Device Provisioning Services, Azure Functions integrations, and more.

## Developing

This project is build to run in Visual Studio Code and has launch tasks pre-configured for starting the Device, Service, or both.  There's no Dev Container just yet but it will be coming in the near future.

## Layout

There are two main Projects that are a part of this repository, the Device and Service.  You can find them in the folder structure as follows:

```
src/
   Device
   Service
```

There are a few other projects, such as `Common` that are used in various capacities throughout the project.


## Base Startup Services

Each project leverages .NET Core Dependency Injection and Configuration quite heavily to configure the objects that will be needed by each project.  These certainly aren't mandated for projects trying to reuse the conecpts provided, but they allow for a strong separation of concerns and testability.

Some categories of registered services in each project are as follows:

#### Options

.NET Core Configuration options are registered and used heavily in both projects to achieve strongly-typed Configuration options to use in the various services.

#### Device Project
Inisde the Device project there are a number of registered classes in `Startup.cs`.  Many different classes are available to represent different ways and Azure IoT can be configured.  Based on the Configuration loaded from local.settings.json the appropriate set of classes will be registered.  

The basic category breakdown is as follows:

###### Authentication

Security providers represent how prove our identity to IoT Hub or DPS.  Authentication providers represent the union of Device Registration Information and Security and are what the IoT Device Client expects to recieve on instantiation.  Options for Symmetric Key and X509 are available for both and chosen based on the provided configuration.

###### Device Registration

Device Registration Providers give a way to handle device registration.  There are two options availble, `Simple` and `DeviceProvisioningService`.  Simple simply loads the device information (Device ID, IoT Hub URI, etc) from configuration.  DPS will reach out to the Azure IoT Device Provisioning Service with the designated ISecurityProviderFactory generated credentials to find out it's device Id and appropriate IoT Hub URI.

###### Devices

`DeviceClientFactory` only has a single implementation at the moment, which builds a `DeviceClient` and connects to IoT hub based on the above provided services.

 

#### Service Project

###### Service Client

All cloud-side services communicate with IoT Devices by sending message through IoT Hub.  The Connect to IoT hub with the Service Client SDK.

---

## Topics

Effective means of using several topics are shown by this project.

#### Remote Procedure Calls

Often time we're in a situation where we need to make a call from a device to the cloud and expect a response.  Implementing this on our own is quite difficult and there are a potentially a lot of moving pieces to manage (threading, asynchronicity, etc).  Because of this, this project provided examples using a few libraries to perform asyc Remote Procedure Calls through Direct Methods and Telemetry Messages.

As RPC is a common pattern, this project chooses to leverage the `json-rpc` standard for communication between device and server, and uses an Azure Function listening to an EventGrid trigger to handle RPC messages in the cloud.  The Visual Studio team produces a great stream-based json-rpc library called `StreamJsonRpc` which has been used to handle all the hard parts of the implementation.  A light weight message handler (`DispatchingClientMessageHandler`) has been added to provide an easy method to map RPC calls to messages coming ang going through the IoT SDK.



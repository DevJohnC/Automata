# Automata Design

## Host Server

- Define a standardized, RESTful API that's easy to host/implement in other languages
- Operates as a standalone service
- Discoverable via SSDP
- RESTful API for resources defined via the kind system
- Mostly responds to API requests
- Sends out event notifications over HTTP
- Interfaced with a standarized client

APIs to host:
- Query for resources
- Make requests to state controllers (response must be authoritative on newly established state)
- Publish device events to the API server (via HTTP?)
    - Have the API server enable/disable event logging to an HTTP endpoint

Hosted resources:
- State controllers
- Sensors?
- Kinds
- Devices

*To turn on a light:*

```
POST /stateChanges

{
  "stateControllerId": "uuid",
  "deviceId": "uuid",
  "stateField": "powerState",
  "stateValue": "on"
}
```

*Get kinds:*
```
GET /resources/core/v1/kinds

[
  {
    "kind": "core/v1/kind",
    "links": {
      "self": "/resources/core/v1/devices/...url encoded resource id..."
    },
    "resourceId": "core/v1/kind",
    "schema": {
      "root": ...root schema type id...
      "schema": ...openapi schema...
    }
  }
]
```

*Get devices:*
```
GET /resources/core/v1/devices

[
  {
    "kind": "lighting/v1/light",
    "links": {
      "self": "/resources/lighting/v1/lights/uuid"
    },
    "resourceId": "uuid",
    "state": {
      "powerState": "on",
      "powerLevel": 0.5
    }
  }
]
```

*Get state controllers:*

```
GET /resources/core/v1/stateControllers

[
  {
    "kind": "core/v1/stateController",
    "links": {
      "self": "/resources/core/v1/stateControllers/uuid"
    },
    "resourceId": "uuid",
    "capabilities": {
      "supportedStates": [
        {
          "device": {
            "links": {
              "self": "/resources/lighting/v1/lights/uuid"
            },
            "kind": "lighting/v1/light",
            "deviceId": "uuid"
          },
          "stateFields": {
            "powerState": ``openApiSchemaForEnum on|off``
            "powerLevel": ``openApiSchemaForRangeValue``
          } 
        }
      ]
    }
  },
  {
    "kind": "core/v1/stateController",
    "links": {
      "self": "/resources/core/v1/stateControllers/uuid"
    },
    "resourceId": "uuid",
    "capabilities": {
      "supportedStates": [
        {
          "device": {
            "links": {
              "self": "http://NODE IP:PORT/resources/computers/v1/desktop/uuid",
              "cache": "/resourcesCache/computers/v1/desktop/uuid"
            },
            "kind": "computers/v1/desktop",
            "deviceId": "uuid"
          },
          "stateFields": {
            "powerState": ``openApiSchemaForEnum on``
          } 
        }
      ]
    }
  }
]
```

**State controllers can specify support for change the state of a device that is hosted on another node.**

**If the node in question is offline or unavailable the caller could query the state controller host for a cached
version of the device, indicated by the cache link**

## API Server

- Will be the HomeCtl project side of things
- Will introduce the concept of users and preferences
  - ie, when John says "turn on the lights" it'll turn them on to John's preference levels unless a level is specified (ie, "turn on the lights to full")
- Will be the integration point for external services like Google Home, Alexa, Siri etc.
- Enhances services for clients
- Exposes a gRPC API to interfacing with Host Server instances on the network
- Discoverable via SSDP
- Use standardized client to interface with Host Server instances

## Resources

- Need to change up approach a bit:
    - Resources should be considered as almost references to object instances, can't include too much behavior because it makes
        sharing the code harder
    - Resources should use GUIDs for IDs and have them generated when a resource is "installed" into the host
    - Host should preserve resource instances (or have a loader type mechanic at least so compiled-in resources can be exposed without being persisted)
    - Host should have a way to check on the status of a resource (if it's still installed/available), especially during startup to avoid
        providers installing the same resources over and over
    - Kinds should derive their resourceId from their names: concat sinular and plural name and compute a 128bit hash so that it can be stored as a GUID
      (despite not being a real guid)(probably SHA-128?).
    - We need to do things this way so that resource installations/references are immutable,
        ie: if you have a reference to a resource with a specific ID it'll continue to work across requessts and time
  

- Resources are object instances.
- Kinds are like Types, but for resources.
- In C#, Kinds are declared as metadata on resource types (ie, are static info associated with a type)
- Clients will need to support declaring resources for which the kind/type isn't available in code and must be declared dynamically

## Device/Device State APIs
- Devices should be immutable declaration of hardware/software devices, with metadata/details that rarely/never change
- DeviceStates should be the mutable state of a device
- DeviceState updatable via a client interface `IDeviceStateClient<T>` or similar?
- Maybe a `DeviceContext` similar to a `DbContext` from EF that would allow device types and their state types to be
   strongly typed
- When a device state is updated, broadcast a notification
- When device added or removed, broadcast a notification

## State Controllers
- Respond to (local?) events that announce new device availability
- Use a simple filter method from a base class to handle this behavior to update the resource in an IResourceProvider
- StateController resource will need to keep the same id, which makes the resource mutable but not the API
- 
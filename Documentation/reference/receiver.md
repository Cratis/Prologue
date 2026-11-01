---
title: Receiver API
description: The Receiver's HTTP endpoints, how captures are stored in MongoDB, and what it needs to host the Workbench.
---

The Receiver is the service the Extractor posts captures to directly instead of writing JSON files — set `prologue.output.kind` to `"Api"` (see [Configuration](configuration.md#prologueoutput)) and point it at a running Receiver. It also hosts the [Workbench](../guides/running-the-workbench.md), the web interface for the whole path from defining systems to an event model, so besides MongoDB it needs a Chronicle kernel. See [Receiver configuration](receiver-configuration.md) for every setting.

## Endpoints

| Endpoint | Purpose |
|---|---|
| `POST /captures` | Stores a capture with no Prologue association |
| `POST /prologues/{prologueId}/captures` | Stores a capture stamped with the given Prologue id, overriding whatever id the Extractor sent |
| `POST /prologues/{prologueId}/parity` | Takes a parity observation from an Extractor that is [shadowing](shadowing.md) commands |
| `GET /prologues/{prologueId}/model.play` | Returns the prologue's accepted model as a Screenplay `.play` file. Provided by the Model step |

Both capture endpoints accept a single `Capture` (the shape defined in `Cratis.Prologue.Contracts`) and respond `202 Accepted` once it's stored. After storing, the Receiver records a summary of the capture for the [Workbench](../guides/running-the-workbench.md); a failure to record it is logged and does not change the response. A capture posted without a prologue is collected under an unassigned prologue.

The parity endpoint takes a `ParityObservation` (see [Extractor shadowing](shadowing.md#the-parity-endpoint)) and responds `202 Accepted`. Any other path that is not a file is served the Workbench's client-side router.

A capture carries the `service` it was correlated within — empty for the default service — so one Receiver collects the captures of every [named service](configuration.md#prologueservices), and stored captures keep that attribution. Captures stored before named services existed read back as the default service.

## Storage

The Receiver stores every capture in the capture collection of a MongoDB database via `Cratis.Prologue.Storage`, configured under the same `prologue` section other tools use:

| Property | Type | Default | Purpose |
|---|---|---|---|
| `prologue.mongo.connectionString` | `string` | `"mongodb://localhost:27017"` | MongoDB connection string |
| `prologue.mongo.database` | `string` | `"Prologue"` | Database name |
| `prologue.mongo.collection` | `string` | `"captures"` | Collection name |

This is the same storage a service-mode Interpreter session reads from — the Receiver is how captures get *into* MongoDB in the first place, whether you're feeding a Studio session or just want captures centralized instead of scattered across JSON files on disk.

## The Workbench's settings

The Chronicle connection, the MongoDB server the Workbench reads from, the clone directory and the secrets repository connections name are documented in [Receiver configuration](receiver-configuration.md). Cloning is described in [Repository connections](repository-connections.md).

## When to use it instead of JSON files

| | JSON files (`output.kind: "Json"`) | Receiver (`output.kind: "Api"`) |
|---|---|---|
| Setup | None — just a mounted folder | A running Receiver + MongoDB |
| Consumed by | Batch Interpreter, `cratis prologue interpret` | Service-mode Interpreter, Studio |
| Good for | A single capture-and-interpret pass | An ongoing or shared capture session, or a Studio-driven review |

## Next

- **[Running the Workbench](../guides/running-the-workbench.md)** — start the Receiver with Chronicle and MongoDB.
- **[Running the Interpreter](../guides/running-the-interpreter.md)** — batch mode (JSON files) versus service mode (Receiver/MongoDB).
- **[Configuration](configuration.md)** — the full `output` schema.

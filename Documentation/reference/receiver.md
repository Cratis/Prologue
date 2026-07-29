---
title: Receiver API
description: The Receiver's two HTTP endpoints and how captures are stored in MongoDB.
---

The Receiver is a thin HTTP endpoint the Extractor can post captures to directly instead of writing JSON files — set `prologue.output.kind` to `"Api"` (see [Configuration](configuration.md#prologueoutput)) and point it at a running Receiver.

## Endpoints

| Endpoint | Purpose |
|---|---|
| `POST /captures` | Stores a capture with no Prologue association |
| `POST /prologues/{prologueId}/captures` | Stores a capture stamped with the given Prologue id, overriding whatever id the Extractor sent |

Both endpoints accept a single `Capture` (the shape defined in `Cratis.Prologue.Contracts`) and respond `202 Accepted` once it's stored.

## Storage

The Receiver stores every capture in MongoDB via `Cratis.Prologue.Storage`, configured under the same `prologue` section other tools use:

| Property | Type | Default | Purpose |
|---|---|---|---|
| `prologue.mongo.connectionString` | `string` | `"mongodb://localhost:27017"` | MongoDB connection string |
| `prologue.mongo.database` | `string` | `"Prologue"` | Database name |
| `prologue.mongo.collection` | `string` | `"captures"` | Collection name |

This is the same storage a service-mode Interpreter session reads from — the Receiver is how captures get *into* MongoDB in the first place, whether you're feeding a Studio session or just want captures centralized instead of scattered across JSON files on disk.

## When to use it instead of JSON files

| | JSON files (`output.kind: "Json"`) | Receiver (`output.kind: "Api"`) |
|---|---|---|
| Setup | None — just a mounted folder | A running Receiver + MongoDB |
| Consumed by | Batch Interpreter, `cratis prologue interpret` | Service-mode Interpreter, Studio |
| Good for | A single capture-and-interpret pass | An ongoing or shared capture session, or a Studio-driven review |

## Next

- **[Running the Interpreter](../guides/running-the-interpreter.md)** — batch mode (JSON files) versus service mode (Receiver/MongoDB).
- **[Configuration](configuration.md)** — the full `output` schema.

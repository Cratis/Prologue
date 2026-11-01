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

Both endpoints accept a single `Capture` (the shape defined in `Cratis.Prologue.Contracts`) and respond `202 Accepted` once it's stored. A capture carries the `service` it was correlated within — empty for the default service — so one Receiver collects the captures of every [named service](configuration.md#prologueservices), and stored captures keep that attribution. Captures stored before named services existed read back as the default service.

## Storage

The Receiver stores every capture in MongoDB via `Cratis.Prologue.Storage`, configured under the same `prologue` section other tools use:

| Property | Type | Default | Purpose |
|---|---|---|---|
| `prologue.mongo.connectionString` | `string` | `"mongodb://localhost:27017"` | MongoDB connection string |
| `prologue.mongo.database` | `string` | `"Prologue"` | Database name |
| `prologue.mongo.collection` | `string` | `"captures"` | Collection name |

This is the same storage a service-mode Interpreter session reads from — the Receiver is how captures get *into* MongoDB in the first place, whether you're feeding a Studio session or just want captures centralized instead of scattered across JSON files on disk.

## Cloning system repositories

The Workbench's Systems step connects GitHub, Azure DevOps and GitLab and clones each system's repository so Discover can read its code. A local folder needs no connection: the Receiver reads it where it is.

| Setting | Default | Purpose |
|---|---|---|
| `Prologue:Repositories:CloneDirectory` | `"/var/prologue/repos"` | Where repositories are cloned |
| `Prologue:Secrets:<name>` | none | The value of the secret a connection names, such as `Prologue:Secrets:github/prologue-read` |

A connection stores only the *name* of the secret that holds its access token. When a clone runs, the Receiver resolves the name from `Prologue:Secrets:<name>` and then from an environment variable of the same name with every character that is not a letter or digit replaced by an underscore, in upper case (`github/prologue-read` is `GITHUB_PROLOGUE_READ`). The token reaches the `git` process only through its environment; it is never stored, logged, returned to the browser or written into a failure message.

Clones are shallow, fetch only the system's branch and, when systems live in sub paths, check out only those paths. Systems in the same repository and branch share one clone, and **Pull again** brings it up to date. The Receiver needs the `git` command line on its path.

## When to use it instead of JSON files

| | JSON files (`output.kind: "Json"`) | Receiver (`output.kind: "Api"`) |
|---|---|---|
| Setup | None — just a mounted folder | A running Receiver + MongoDB |
| Consumed by | Batch Interpreter, `cratis prologue interpret` | Service-mode Interpreter, Studio |
| Good for | A single capture-and-interpret pass | An ongoing or shared capture session, or a Studio-driven review |

## Next

- **[Running the Interpreter](../guides/running-the-interpreter.md)** — batch mode (JSON files) versus service mode (Receiver/MongoDB).
- **[Configuration](configuration.md)** — the full `output` schema.

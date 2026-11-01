---
title: Receiver configuration
description: Every configuration key the Receiver reads, and how secrets are resolved.
---

The Receiver uses standard ASP.NET Core configuration, so every key below can be set in `appsettings.json` or as an environment variable with `__` in place of `:` (`Cratis:MongoDB:Server` is `Cratis__MongoDB__Server`). The defaults are those in `Source/Receiver/appsettings.json` and the options classes. [Running the Workbench](../guides/running-the-workbench.md) shows them used in `Source/Receiver/docker-compose.yml`.

## Chronicle

| Key | Default | Purpose |
|---|---|---|
| `Cratis:Chronicle:ConnectionString` | `chronicle://chronicle-dev-client:chronicle-dev-secret@localhost:35000` | The Chronicle kernel the Workbench records its events in. The default holds the development credentials of the development image; use your own for a real deployment |

The Workbench uses an event store named `Prologue`, which is not configurable.

## Workbench read models

| Key | Default | Purpose |
|---|---|---|
| `Cratis:MongoDB:Server` | `mongodb://localhost:27017` | The MongoDB server the Workbench reads its read models from |
| `Cratis:MongoDB:Database` | `Prologue` | The database those read models live in |

Chronicle projects the Workbench's read models into MongoDB, and the Receiver reads them from there. Both have to use the **same** MongoDB server and database. The development image of Chronicle bundles a replica set, which is why the compose file points both at the `chronicle` container with `?directConnection=true`. For a durable deployment, run Chronicle against your own replica set and give the Receiver that server here.

## Captures

| Key | Default | Purpose |
|---|---|---|
| `Prologue:Mongo:ConnectionString` | `mongodb://localhost:27017` | The MongoDB server captures are stored in |
| `Prologue:Mongo:Database` | `Prologue` | The database name |
| `Prologue:Mongo:Collection` | `captures` | The collection name |

These are the keys the capture endpoints and the Interpreter's service mode use; see [Receiver API](receiver.md#storage). Point `Prologue:Mongo:ConnectionString` at the same server as `Cratis:MongoDB:Server`.

## Repositories

| Key | Default | Purpose |
|---|---|---|
| `Prologue:Repositories:CloneDirectory` | `/var/prologue/repos` | Where repositories are cloned |
| `Prologue:Secrets:<name>` | none | The value of the secret a connection names |

The image installs the `git` command line, which cloning needs. Mount `CloneDirectory` on a volume if you want clones to survive a restart.

### How secrets are resolved

A repository connection stores only the *name* of a secret, such as `github/prologue-read`. When a clone needs the token, the Receiver looks the name up in this order and uses the first non-empty value:

1. The configuration key `Prologue:Secrets:<name>`, for example `Prologue:Secrets:github/prologue-read`.
2. An environment variable named after the secret: every character that is not an ASCII letter or digit becomes an underscore, and the result is upper case. `github/prologue-read` is `GITHUB_PROLOGUE_READ`.

If neither is set the clone fails with the message `The secret '<name>' is not configured on the Receiver`. The token is passed to `git` through its process environment and is removed from any failure message. It is never stored, logged or returned to the browser.

Environment variable names cannot contain `/` or `-`, which is why a secret named `github/prologue-read` is easiest to supply as `GITHUB_PROLOGUE_READ`.

## Other settings

The Receiver listens on port `8080` in the image (`ASPNETCORE_URLS=http://0.0.0.0:8080`). AI providers, pools and agents are not configured here; they are set up in the Workbench. See [AI setup](ai-setup.md).

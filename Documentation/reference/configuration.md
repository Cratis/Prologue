---
title: Configuration
description: Configuration used by the Extractor, Receiver, and batch or service Interpreter modes.
---

The Extractor and Interpreter add `cratis-prologue.json` through the shared
`Cratis.Prologue.Configuration` package. The Extractor binds capture settings under `prologue` and YARP's
`reverseProxy` section. Batch interpretation binds `llm`; service interpretation also binds `prologue.mongo`.
The Receiver uses standard ASP.NET configuration, including `appsettings.json` and environment variables, and
binds `prologue.mongo`.

## Where the file comes from

- The Extractor and Interpreter look for `cratis-prologue.json` in their working directory. Override the path with
  `PROLOGUE_CONFIG`, or pass another base path when calling `AddPrologueConfiguration()` in a custom host.
- **The file is the baseline; environment variables override it.** Properties have double-underscore environment
  equivalents such as `Prologue__Output__Kind`, `Prologue__SqlServer__0__ConnectionString`, and
  `ReverseProxy__Clusters__monitored__Destinations__primary__Address`.
- The Receiver follows standard ASP.NET precedence instead. Configure MongoDB in its `appsettings.json` or with
  `Prologue__Mongo__ConnectionString`, `Prologue__Mongo__Database`, and `Prologue__Mongo__Collection`.
- A missing `cratis-prologue.json` is allowed; type defaults and environment variables still apply.

## Root shape

| Property | Type | Purpose |
|---|---|---|
| `prologue` | [`PrologueOptions`](#prologue) and storage options | Capture sources, correlation, output, and MongoDB storage for the applicable host |
| `llm` | [`LlmOptions`](#llm) | Optional language-model refinement bound by the Interpreter |
| `reverseProxy` | YARP configuration | Routes and clusters for the HTTP capture source — see [YARP's configuration docs](https://microsoft.github.io/reverse-proxy/articles/config-files.html) |

## `prologue`

| Property | Type | Default | Purpose |
|---|---|---|---|
| `prologueId` | `guid` | empty guid | Associates captures with one interpretation session when explicitly configured |
| `output` | [`OutputOptions`](#prologueoutput) | — | Where captures are written |
| `correlation` | [`CorrelationOptions`](#prologuecorrelation) | — | The correlation time window |
| `sqlServer` | array of [`SqlServerOptions`](#prologuesqlserver) | `[]` | SQL Server databases of the default service to watch via CDC |
| `postgres` | array of [`PostgresOptions`](#prologuepostgres) | `[]` | PostgreSQL databases of the default service to watch via logical replication |
| `openTelemetry` | [`OpenTelemetryOptions`](#prologueopentelemetry) | — | OTLP proxy configuration, shared by every service |
| `services` | map of service name to [`ServiceOptions`](#prologueservices) | `{}` | Named services of a system made of several services |
| `shadowing` | array of [shadowing rules](shadowing.md) | `[]` | Commands the Extractor mirrors to a new system to compare decisions; requires `prologueId` and the `Api` output |
| `mongo` | [`MongoOptions`](#prologuemongo) | — | Capture and service-session storage used by the Receiver and service Interpreter |

### `prologue.output`

| Property | Type | Default | Purpose |
|---|---|---|---|
| `kind` | `"Api"` \| `"Json"` | `"Api"` | Whether captures are posted to a [Receiver](receiver.md) or written to JSON files |
| `api.endpoint` | `string` | `"http://localhost:5005"` | Receiver base address, used when `kind` is `"Api"` |
| `json.directory` | `string` | `"./captures"` | Folder to write rolling `.jsonl` capture files to, used when `kind` is `"Json"` |
| `json.maxEntriesPerFile` | `int` | `10000` | Entries per file before the Extractor rolls to a new one |

### `prologue.correlation`

| Property | Type | Default | Purpose |
|---|---|---|---|
| `windowMilliseconds` | `int` | `2000` | How long after a command the correlator keeps grouping database transactions and telemetry into the same capture — see [How Prologue works](../concepts.md#how-captures-get-correlated) |

### `prologue.sqlServer[]`

| Property | Type | Default | Purpose |
|---|---|---|---|
| `name` | `string` | `"sqlserver"` | Label for this database in captures |
| `connectionString` | `string` | — | Connection string; needs permission to enable CDC |
| `enableChangeDataCapture` | `bool` | `true` | Whether the Extractor enables CDC on the database itself |
| `tables` | array of `string` | `[]` | Table allowlist; empty captures every user table |
| `pollIntervalMilliseconds` | `int` | `500` | How often the Extractor polls for new CDC changes |

### `prologue.postgres[]`

| Property | Type | Default | Purpose |
|---|---|---|---|
| `name` | `string` | `"postgres"` | Label for this database in captures |
| `connectionString` | `string` | — | Connection string; needs permission to create a replication slot and publication, and `wal_level = logical` on the server |
| `slot` | `string` | `"prologue_slot"` | Replication slot name |
| `publication` | `string` | `"prologue_publication"` | Publication name |

### `prologue.openTelemetry`

| Property | Type | Default | Purpose |
|---|---|---|---|
| `enabled` | `bool` | `false` | Whether the OTLP proxy is active |
| `serviceNames` | array of `string` | `[]` | Service name allowlist for the default service; empty captures telemetry from every service no [named service](#prologueservices) claims |
| `attributeKeys` | array of `string` | `[]` | Attribute keys whose values are captured; all observed keys remain visible while other values are dropped |
| `upstream.http` | `string` | — | Upstream OTLP/HTTP collector to forward telemetry to |
| `upstream.grpc` | `string` | — | Upstream OTLP/gRPC collector to forward telemetry to |

Leave both `upstream` addresses empty to make the Extractor a terminal collector rather than a forwarding proxy.

### `prologue.services`

A system captured as one service needs no `services` at all: the top-level `sqlServer`, `postgres`, every
reverse-proxy cluster and all telemetry belong to the **default service**. For a system made of several services,
name each one under `services`. Every observation is attributed to exactly one service, and observations from
different services are never correlated into the same capture.

| Property | Type | Default | Purpose |
|---|---|---|---|
| `sqlServer` | array of [`SqlServerOptions`](#prologuesqlserver) | `[]` | SQL Server databases belonging to the service |
| `postgres` | array of [`PostgresOptions`](#prologuepostgres) | `[]` | PostgreSQL databases belonging to the service |
| `clusters` | array of `string` | the service name | Ids of the `reverseProxy` clusters whose HTTP commands belong to the service |
| `telemetryServiceNames` | array of `string` | the service name | OpenTelemetry `service.name` values whose telemetry belongs to the service; matched case-insensitively |

```json
{
    "prologue": {
        "sqlServer": [ { "name": "legacy", "connectionString": "..." } ],
        "services": {
            "orders": {
                "sqlServer": [ { "name": "orders", "connectionString": "..." } ]
            },
            "billing": {
                "postgres": [ { "name": "billing", "connectionString": "..." } ],
                "clusters": [ "billing-api" ],
                "telemetryServiceNames": [ "Billing.Api" ]
            }
        }
    }
}
```

- Clusters and telemetry service names no named service claims stay with the default service. Telemetry claimed by
  a named service is captured even when the default service's `openTelemetry.serviceNames` filter would exclude it.
- A cluster or telemetry service name claimed by two services is a configuration error; the Extractor refuses to
  start.
- Captures and observations carry a `service` property — empty for the default service. JSON output keeps the
  `prologue-{kind}-{index}.jsonl` file names for the default service and writes `prologue-{service}-{kind}-{index}.jsonl`
  for a named one. Capture files and stored captures written before named services existed read back as the
  default service.
- Environment variables reach a named service the usual way, for example
  `Prologue__Services__billing__Postgres__0__ConnectionString`.

### `prologue.mongo`

The Receiver and service-mode Interpreter bind these settings through `Cratis.Prologue.Storage`. The service also
stores interpretation checkpoints in the configured database.

| Property | Type | Default | Purpose |
|---|---|---|---|
| `connectionString` | `string` | `"mongodb://localhost:27017"` | MongoDB connection used for captures and service checkpoints |
| `database` | `string` | `"Prologue"` | Database containing Prologue collections |
| `collection` | `string` | `"captures"` | Capture collection used by the Receiver and Interpreter |

## `llm`

| Property | Type | Default | Purpose |
|---|---|---|---|
| `enabled` | `bool` | `false` | Whether the Interpreter refines the heuristic model with a language model |
| `kind` | `"Ollama"` \| `"OpenAI"` \| `"AzureOpenAI"` \| `"OpenAICompatible"` \| `"Anthropic"` | `"Ollama"` | Which provider to call |
| `endpoint` | `string` | `"http://llm:11434"` | Provider endpoint; the hosted providers (OpenAI, Azure OpenAI, Anthropic) default to their public endpoint if you don't set one |
| `accessToken` | `string` | — | API key for the hosted providers; not needed for a local Ollama endpoint |
| `modelId` | `string` | an Ollama model tag | The model to call — for `AzureOpenAI`, this is the **deployment name**, not a model name |
| `refinementTimeout` | duration | 2 minutes | How long the Interpreter waits for a refinement response before giving up and keeping the heuristic result |

See [Running the Interpreter](../guides/running-the-interpreter.md#configure-llm-refinement) for a worked example.

## Next

- **[Capture sources](capture-sources.md)** — what triggers a capture per source.
- **[Point Prologue at your system](../guides/point-prologue-at-your-system.md)** — a complete worked `cratis-prologue.json`.

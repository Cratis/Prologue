---
title: Configuration
description: The full cratis-prologue.json schema — every property, its default, and its environment-variable override name.
---

Every Prologue tool binds the same `cratis-prologue.json`, via the shared `Cratis.Prologue.Configuration` package — never `appsettings.json`. The Extractor binds the `prologue` section; the Interpreter binds `llm`; the HTTP capture source binds the top-level `reverseProxy` section (a plain [YARP](https://microsoft.github.io/reverse-proxy/) configuration block, not a Prologue-specific schema).

## Where the file comes from

- Each tool looks for `cratis-prologue.json` in its working directory by default. Override the path with the `PROLOGUE_CONFIG` environment variable, or point `--config`-style hosting at a different `basePath` if you're calling `AddPrologueConfiguration()` yourself.
- **The file is the baseline; environment variables override it.** Every property below has a double-underscore environment variable equivalent — `Prologue__Output__Kind`, `Prologue__SqlServer__0__ConnectionString`, `ReverseProxy__Clusters__monitored__Destinations__primary__Address` — so a deployed tool can be configured by its host (a container, an orchestrator, an Aspire composition) without rewriting the file. If you're hosting a Prologue tool yourself, `AddPrologueConfiguration()` from `Cratis.Prologue.Configuration` wires up that precedence for you.
- The file is missing? That's fine — every property below has a default, and environment variables still apply.

## Root shape

| Property | Type | Purpose |
|---|---|---|
| `prologue` | [`PrologueOptions`](#prologue) | Capture sources, correlation, and output — bound by the Extractor |
| `llm` | [`LlmOptions`](#llm) | Optional language-model refinement — bound by the Interpreter |
| `reverseProxy` | YARP configuration | Routes and clusters for the HTTP capture source — see [YARP's configuration docs](https://microsoft.github.io/reverse-proxy/articles/config-files.html) |

## `prologue`

| Property | Type | Default | Purpose |
|---|---|---|---|
| `prologueId` | `guid` | a fresh id | Identifies this Prologue's captures, so a folder or Receiver can hold more than one Prologue's data without mixing them |
| `output` | [`OutputOptions`](#prologueoutput) | — | Where captures are written |
| `correlation` | [`CorrelationOptions`](#prologuecorrelation) | — | The correlation time window |
| `sqlServer` | array of [`SqlServerOptions`](#prologuesqlserver) | `[]` | SQL Server databases to watch via CDC |
| `postgres` | array of [`PostgresOptions`](#prologuepostgres) | `[]` | PostgreSQL databases to watch via logical replication |
| `openTelemetry` | [`OpenTelemetryOptions`](#prologueopentelemetry) | — | OTLP proxy configuration |

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
| `serviceNames` | array of `string` | `[]` | Service name allowlist; empty captures telemetry from every service |
| `attributeKeys` | array of `string` | `[]` | Span/log attribute keys whose *values* are captured; everything else is dropped |
| `upstream.http` | `string` | — | Upstream OTLP/HTTP collector to forward telemetry to |
| `upstream.grpc` | `string` | — | Upstream OTLP/gRPC collector to forward telemetry to |

Leave both `upstream` addresses empty to make the Extractor a terminal collector rather than a forwarding proxy.

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

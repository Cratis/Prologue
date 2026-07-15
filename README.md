# Cratis Prologue

Prologue captures what an existing system *actually does* — its HTTP commands, database changes, and telemetry — and interprets those
captures into an event model that can be brought into [Cratis Studio](https://github.com/Cratis/Studio) or used from the Cratis CLI.

Prologue is self contained: it has no dependency on Studio and does not require Orleans or any other hosting model.
Everything runs either as a downloadable CLI (the Extractor) or as containers (the Extractor, Interpreter, and Receiver).

## Projects

| Project | Package / Image | Purpose |
|---|---|---|
| `Source/Contracts` | `Cratis.Prologue.Contracts` (NuGet) | The capture contract — `Capture`, `Observation`, payload types, and the canonical JSON (`CaptureSerialization`) and capture-file (`CaptureFiles`) formats. |
| `Source/Configuration` | `Cratis.Prologue.Configuration` (NuGet) | Well-defined configuration types for `cratis-prologue.json` plus serialization helpers to read and write it. |
| `Source/Storage` | `Cratis.Prologue.Storage` (NuGet) | MongoDB persistence of captures — used by the Receiver and by consumers such as Studio. |
| `Source/Extractor` | `cratis/prologue-extractor` (Docker) | Runs next to the system being captured. Captures SQL Server CDC, Postgres logical replication, HTTP commands (reverse proxy), and OpenTelemetry — writes capture files or posts to the Receiver. |
| `Source/Interpreter.Contracts` | `Cratis.Prologue.Interpreter.Contracts` (NuGet) | The extraction result contract — `ExtractionResult` and the `Extracted*` model tree, plus serialization helpers for the result file. |
| `Source/Interpreter` | `cratis/prologue-interpreter` (Docker) | Run-to-completion job that reads capture files from a mounted folder, interprets them into an event model, and writes the extraction result to a mounted output folder. |
| `Source/Receiver` | `cratis/prologue-receiver` (Docker) | HTTP endpoint the Extractor can post captures to directly, instead of capturing to file. |

## Configuration — `cratis-prologue.json`

All tools are configured with a dedicated `cratis-prologue.json` file — not `appsettings.json`.
The `Cratis.Prologue.Configuration` package contains the configuration types and does the serialization,
so any consumer (Studio, the CLI, or your own tooling) can write configuration files in the exact format the tools expect.

```json
{
    "prologue": {
        "output": { "kind": "Json", "json": { "directory": "./captures" } },
        "correlation": { "windowMilliseconds": 2000 },
        "sqlServer": [ { "name": "main", "connectionString": "..." } ],
        "postgres": [],
        "openTelemetry": { "enabled": true }
    },
    "llm": {
        "enabled": true,
        "kind": "Anthropic",
        "accessToken": "sk-...",
        "modelId": "claude-opus-4-6"
    }
}
```

- The **Extractor** looks for `cratis-prologue.json` in its working directory (override with the `PROLOGUE_CONFIG` environment variable) and binds the `prologue` section.
- The **Interpreter** reads the same file (mounted into its container at `/config/cratis-prologue.json`) and binds the `llm` section for optional LLM-based refinement. Supported `kind` values: `Ollama` (default, native chat API), `OpenAI`, `AzureOpenAI` (the `modelId` is the deployment name), `OpenAICompatible` (any `/v1` endpoint), and `Anthropic` — each configured with an `endpoint` and an `accessToken` as needed; the hosted providers default to their public endpoints and models.

## Data flow

```
Extractor ──(capture .jsonl files)──▶ mounted folder ──▶ Interpreter ──▶ extraction result .json
    └──────(HTTP)──▶ Receiver ──▶ MongoDB
```

The Extractor emits one `CapturedEntry` per line (JSON lines), partitioned per source kind.
The Interpreter reads a folder of those files, reconstructs the correlated captures, and produces an `ExtractionResult`.

## Building

```shell
dotnet build                # Debug — includes inline specs
dotnet test                 # run specs
dotnet build -c Release     # Release — treat warnings as errors
```

Container images are built from the repository root:

```shell
docker build -f Source/Extractor/Dockerfile -t cratis/prologue-extractor .
docker build -f Source/Interpreter/Dockerfile -t cratis/prologue-interpreter .
docker build -f Source/Receiver/Dockerfile -t cratis/prologue-receiver .
```

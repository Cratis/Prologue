---
title: Running the Interpreter
description: Batch mode versus service mode, the Interpreter's arguments and environment variables, and configuring LLM refinement.
---

Turn a folder of captures into an `ExtractionResult` and a Screenplay `.play` file, and optionally configure a language model to refine the names.

## Prerequisites

- A folder of `.jsonl` capture files, produced by the Extractor (see [Point Prologue at your system](point-prologue-at-your-system.md)).
- Docker, to run `cratis/prologue-interpreter`.

## Run batch mode

Batch mode is the image's default — no flags needed:

```bash
docker run --rm \
  -v "$(pwd)/captures:/captures" \
  -v "$(pwd)/output:/output" \
  cratis/prologue-interpreter
```

It reads every capture file under `/captures`, reconstructs the correlated captures, and writes `/output/extraction-result.json` plus a generated `.play` file, then exits.

Every input and output path is overridable, as a CLI argument on the binary or an environment variable on the container:

| CLI argument | Environment variable | Default | Meaning |
|---|---|---|---|
| `--captures <folder>` | `PROLOGUE_CAPTURES` | `/captures` in the image | Folder to read `.jsonl` capture files from |
| `--output <file>` | `PROLOGUE_OUTPUT` | `/output/extraction-result.json` in the image | Where to write the `ExtractionResult` JSON |
| `--play-output <file>` | `PROLOGUE_PLAY_OUTPUT` | next to the output file, named after the derived system name | Where to write the generated `.play` file |
| `--prologue-id <guid>` | `PROLOGUE_ID` | — | Which Prologue's captures to interpret, when a folder holds more than one |

`cratis-prologue.json` (mounted at `/config/cratis-prologue.json`, or wherever `PROLOGUE_CONFIG` points) is optional in batch mode — it's only needed to configure LLM refinement, since the capture folder and output paths are already explicit.

## Run service mode

Service mode hosts the same interpretation as a resumable HTTP session, with state persisted in MongoDB, for Studio's interactive flow rather than a one-shot CLI run:

```bash
docker run --rm -p 5004:5004 \
  -e PROLOGUE_MODE=service \
  -v "$(pwd)/cratis-prologue.json:/config/cratis-prologue.json:ro" \
  cratis/prologue-interpreter
```

(`--serve` on the binary is equivalent to `PROLOGUE_MODE=service` on the container.) Three more environment variables shape service mode's lifecycle:

| Environment variable | Default | Meaning |
|---|---|---|
| `PROLOGUE_SERVICE_PORT` | `5004` | The HTTP API port |
| `PROLOGUE_GRACE_PERIOD` | `300` seconds | How long a session waits for an answer to a clarifying question before the container is allowed to exit |
| `PROLOGUE_IDLE_TIMEOUT` | `600` seconds | How long an idle session waits before the container is allowed to exit |

When the grace period or idle timeout elapses, the container exits cleanly — session state is already in MongoDB, so an orchestrator restarting it later resumes exactly where it left off. If you're not integrating with Studio, batch mode is what you want.

## Configure LLM refinement

Add an `llm` section to `cratis-prologue.json` to have the Interpreter rename the heuristic model into domain language and derive a system name, instead of stopping at mechanical names:

```json title="cratis-prologue.json (excerpt)"
{
    "llm": {
        "enabled": true,
        "kind": "Anthropic",
        "accessToken": "sk-...",
        "modelId": "claude-opus-4-6"
    }
}
```

`kind` is one of `Ollama` (the default, a local model over its native chat API — no `accessToken` needed), `OpenAI`, `AzureOpenAI` (where `modelId` is the deployment name, not a model name), `OpenAICompatible` (any `/v1`-compatible endpoint — set `endpoint` explicitly), or `Anthropic`. The hosted providers default to their public endpoint; override it with `endpoint` for a private or self-hosted deployment.

With refinement enabled and running in an interactive terminal, the Interpreter may ask a clarifying question when a decision would materially change the model — one at a time, with its background context and always an "Other" choice. Non-interactive runs (CI, piped output) never ask; they finalize with the model's best effort.

:::note
The [Cratis CLI](/cli/reference/prologue/)'s `cratis prologue interpret` wraps batch mode with the same configuration precedence and prints the same result — use it if you'd rather not hand-roll `docker run` for every capture folder.
:::

## Next

- **[Reference — Extraction result](../reference/extraction-result.md)** — the full shape of what gets written.
- Run the generated `.play` file with `cratis run` to boot it in a local Stage sandbox.

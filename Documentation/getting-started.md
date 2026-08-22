---
title: Getting started
description: Run the bundled sample system, inspect its captures, then produce a provisional extraction result and .play file for review.
---

By the end of this page you'll have watched Prologue capture an ordinary sample system and used batch mode to
produce your own provisional `extraction-result.json` and `.play` file from a folder of captures.

## Part A — watch it work

Prologue ships with [`Samples/Library`](https://github.com/Cratis/Prologue/tree/main/Samples/Library) — an ordinary ASP.NET + Entity Framework Core library system with authors, members, a catalog, and lending, built with **no Cratis constructs at all**. It's exactly the kind of system Prologue gets pointed at, and its Aspire composition wires the whole capture pipeline around it for you.

```bash
git clone https://github.com/Cratis/Prologue.git
cd Prologue/Samples/Library
aspire run                        # PostgreSQL
```

Open the dashboard at **<http://localhost:18880>**. On the `core` resource, run the **Simulate load** command — pick a transaction count and the library starts behaving like a system in real use: authors get registered, books get reserved and returned, some of it gets rejected (no copies left, an author with an outstanding loan). Watch observations accumulate in MongoDB as the Extractor's reverse proxy sees HTTP commands, its database
watcher sees transactions, and its OTLP proxy sees telemetry. The Extractor correlates those enabled signals
heuristically as the simulation runs.

That's the full pipeline running end to end, feeding the Receiver so a resumable Interpreter session (the path Studio uses) can pick the captures up later. See the [sample's README](https://github.com/Cratis/Prologue/blob/main/Samples/Library/README.md) for what each resource does.

## Part B — turn captures into a Screenplay

The sample above stores captures in MongoDB for Studio's interactive session. To get a `.play` file yourself, with nothing but the Extractor and Interpreter containers, point the Extractor at **JSON file output** instead and run the Interpreter's batch mode against the resulting folder — the exact path the [Cratis CLI](/cli/reference/prologue/)'s `cratis prologue interpret` automates for you.

### Configure the Extractor

Save this as `cratis-prologue.json` — it's the same minimal, HTTP-only configuration the Extractor itself ships for local testing (`Source/Extractor/cratis-prologue.json` in the repo), with the destination pointed at your own system:

```json title="cratis-prologue.json"
{
    "prologue": {
        "output": {
            "kind": "Json",
            "json": {
                "directory": "/captures",
                "maxEntriesPerFile": 10000
            }
        },
        "correlation": {
            "windowMilliseconds": 2000
        },
        "sqlServer": [],
        "postgres": [],
        "openTelemetry": {
            "enabled": false
        }
    },
    "reverseProxy": {
        "routes": {
            "monitored": {
                "clusterId": "monitored",
                "match": { "path": "{**catch-all}" }
            }
        },
        "clusters": {
            "monitored": {
                "destinations": {
                    "primary": { "address": "http://host.docker.internal:5000/" }
                }
            }
        }
    }
}
```

Replace `http://host.docker.internal:5000/` with the address of whatever system you want to capture — it's the only thing in this file that's specific to your system. Everything else — SQL Server, Postgres, and OpenTelemetry — is switched off, so the only capture source active is the HTTP reverse proxy.

### Run the Extractor

```bash
mkdir captures
docker run --rm -p 8080:8080 \
  -v "$(pwd)/cratis-prologue.json:/config/cratis-prologue.json:ro" \
  -v "$(pwd)/captures:/captures" \
  cratis/prologue-extractor
```

The container reads `cratis-prologue.json` from `/config` (that's `PROLOGUE_CONFIG`'s default in the image) and now sits in front of your system on port 8080. Send it a few state-changing requests — `POST`, `PUT`, or `DELETE` through `http://localhost:8080/...` instead of straight at your system — and watch `.jsonl` capture files appear under `./captures`.

:::note
In a real deployment you'd run the Extractor as a sidecar next to your system and point your traffic (or your reverse proxy) at it, rather than curling it by hand. See [Point Prologue at your system](guides/point-prologue-at-your-system.md).
:::

### Run the Interpreter

```bash
mkdir output
docker run --rm \
  -v "$(pwd)/captures:/captures" \
  -v "$(pwd)/output:/output" \
  cratis/prologue-interpreter
```

Batch mode is the Interpreter image's default: it reads every `.jsonl` file under `/captures`, analyzes the
correlated evidence, and writes `/output/extraction-result.json` plus a generated `.play` file to `/output`.
Configure a language model in `cratis-prologue.json`'s `llm` section if you want names and descriptions refined;
see [Running the Interpreter](guides/running-the-interpreter.md). Review the candidate structure in either mode.

### What you'll see

The exact candidates depend on the observations. The result has this shape, with names and structure proposed by
the heuristics and any configured language-model refinement:

```json title="output/extraction-result.json (excerpt)"
{
  "prologueId": "00000000-0000-0000-0000-000000000000",
  "systemName": "",
  "modules": [
    {
      "name": "Catalog",
      "features": [
        {
          "name": "Books",
          "slices": [
            {
              "name": "Reservation",
              "type": "StateChange",
              "commands": [
                {
                  "name": "ReserveBook",
                  "properties": [{ "name": "Isbn", "type": "string", "isRequired": true, "maxLength": 0 }],
                  "validations": []
                }
              ],
              "events": [
                { "name": "BookReserved", "properties": [{ "name": "Isbn", "type": "string", "isRequired": false, "maxLength": 0 }] }
              ],
              "readModels": [],
              "projections": [],
              "constraints": []
            }
          ]
        }
      ]
    }
  ]
}
```

alongside a `.play` file expressing the same provisional model in [Screenplay](/screenplay/)'s declarative
language. Validate and review it before treating it as authored intent. See
[The extraction result](reference/extraction-result.md) for the full shape.

## What's next

- **[cratis prologue interpret](/cli/reference/prologue/)** — the CLI wraps everything in Part B into two commands, including a wizard that writes `cratis-prologue.json` for you.
- **[Reference — Configuration](reference/configuration.md)** — every `cratis-prologue.json` property, not just the ones used here.
- Validate and review the resulting `.play` file, then use the supported [Screenplay](/screenplay/) workflow for the constructs it contains.

---
title: Point Prologue at your system
description: Configure the Extractor for a real system — pick your capture sources, write cratis-prologue.json, and run it as a sidecar.
---

Turn on the capture sources your system actually has, wire the Extractor in as a sidecar, and confirm captures are landing — before you worry about interpretation.

## Prerequisites

- The system you're capturing is reachable over HTTP, or has a SQL Server / PostgreSQL database you can connect to (or both).
- Docker, to run `cratis/prologue-extractor`.
- For SQL Server capture: a connection string with permission to enable Change Data Capture.
- For PostgreSQL capture: a connection string with permission to create a replication slot and publication, and `wal_level = logical` on the server.

## Pick your capture sources

You don't need all four. A system with no database access still gets useful HTTP captures; a batch system with no HTTP surface still gets database captures. Decide which of `sqlServer`, `postgres`, `openTelemetry`, and the HTTP reverse proxy apply, and leave the rest at their empty/disabled defaults. See [Reference — Capture sources](../reference/capture-sources.md) for what each one needs.

## Write `cratis-prologue.json`

```json title="cratis-prologue.json"
{
    "prologue": {
        "output": {
            "kind": "Json",
            "json": { "directory": "/captures" }
        },
        "correlation": { "windowMilliseconds": 2000 },
        "sqlServer": [
            { "name": "main", "connectionString": "Server=...;Database=...;", "tables": ["Orders", "OrderLines"] }
        ],
        "postgres": [],
        "openTelemetry": { "enabled": false }
    },
    "reverseProxy": {
        "routes": {
            "monitored": { "clusterId": "monitored", "match": { "path": "{**catch-all}" } }
        },
        "clusters": {
            "monitored": { "destinations": { "primary": { "address": "http://your-system:8081/" } } }
        }
    }
}
```

- Leave `tables` out (or empty) to capture every user table's changes instead of an allowlist.
- Omit the `reverseProxy` block entirely if you're not capturing HTTP — the Extractor just won't proxy anything.
- Switch `output.kind` to `"Api"` with an `output.api.endpoint` instead of `"Json"` to post captures straight to a [Receiver](../reference/receiver.md) rather than writing files.

The full property list, with every default, is in [Reference — Configuration](../reference/configuration.md).

## Run the Extractor as a sidecar

```bash
docker run -d --name prologue-extractor \
  -p 8080:8080 -p 4317:4317 -p 4318:4318 \
  -v "$(pwd)/cratis-prologue.json:/config/cratis-prologue.json:ro" \
  -v "$(pwd)/captures:/captures" \
  cratis/prologue-extractor
```

Port `8080` is the reverse proxy your system's traffic should go through instead of hitting your system directly; `4317`/`4318` are the OTLP gRPC/HTTP endpoints if you're forwarding telemetry through the Extractor. Point your own reverse proxy, load balancer, or client traffic at `8080` — anything that reaches your system without going through it produces no HTTP captures.

If you'd rather not mount a config file, every property has a double-underscore environment variable equivalent (`Prologue__SqlServer__0__ConnectionString`, `ReverseProxy__Clusters__monitored__Destinations__primary__Address`, and so on) — env vars win over the file, which is handy for injecting secrets from your orchestrator instead of writing them into `cratis-prologue.json`.

## Confirm captures are landing

Exercise the system — a real request, or your own smoke test — through port `8080`, then check the mounted `captures` folder for new `.jsonl` files (or your Receiver's MongoDB collection, if that's where you pointed output). No files after a state-changing request usually means the reverse proxy destination is wrong, or the request didn't use `POST`/`PUT`/`DELETE` — see [Troubleshooting](../reference/faq.md).

## Next

- **[Running the Interpreter](running-the-interpreter.md)** — turn the folder of captures into an event model.
- **[Reference — Capture sources](../reference/capture-sources.md)** — the full property list per source.

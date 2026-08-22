---
title: Capture sources
description: What triggers a capture for each source the Extractor watches, and the configuration specific to it.
---

The Extractor can run four source families at once. SQL Server and PostgreSQL observations exclude row values,
and HTTP observations exclude request and response bodies. Capture output can still contain query strings,
identifiers, names, and explicitly allowlisted OpenTelemetry attribute values. See
[Why Prologue](../why-prologue.md#what-prologue-captures--and-doesnt) for the complete boundary.

## SQL Server

| | |
|---|---|
| Enabled by | Adding an entry to `prologue.sqlServer[]` |
| Mechanism | Change Data Capture. The Extractor enables CDC on the database itself — no manual setup needed unless you set `enableChangeDataCapture: false` |
| Captures | Which tables and columns changed, per transaction |
| Never captures | Row values |
| Config | `name`, `connectionString`, `enableChangeDataCapture`, `tables` (allowlist), `pollIntervalMilliseconds` — see [Configuration](configuration.md#prologuesqlserver) |

## PostgreSQL

| | |
|---|---|
| Enabled by | Adding an entry to `prologue.postgres[]` |
| Mechanism | Logical replication. The Extractor creates its own replication slot and publication on the configured database — the server just needs `wal_level = logical` |
| Captures | Which tables and columns changed, per transaction |
| Never captures | Row values |
| Config | `name`, `connectionString`, `slot`, `publication` — see [Configuration](configuration.md#prologuepostgres) |

## HTTP

| | |
|---|---|
| Enabled by | Adding a `reverseProxy` section — the Extractor becomes a [YARP](https://microsoft.github.io/reverse-proxy/) reverse proxy in front of your system |
| Mechanism | Every request that reaches the proxy passes through to your system unchanged; state-changing requests are also captured |
| Captures | Method, path including query string, response status, and the W3C `traceparent` trace id if present, for `POST`, `PUT`, and `DELETE` requests only |
| Never captures | `GET`/read traffic, request or response bodies |
| Config | Plain YARP `routes`/`clusters` configuration — see [Configuration](configuration.md#root-shape) |

## OpenTelemetry

| | |
|---|---|
| Enabled by | `prologue.openTelemetry.enabled: true` |
| Mechanism | An OTLP proxy (HTTP and gRPC). Telemetry is captured, then forwarded on to `upstream.http`/`upstream.grpc` unchanged — or nowhere, if you leave both empty, making the Extractor a terminal collector |
| Captures | Span, metric, and log names and identifiers; every observed attribute key; values only for configured `attributeKeys`; service names matching `serviceNames`, or all services when empty |
| Never captures | Log bodies, metric measurements, or values for attributes not in `attributeKeys` |
| Config | `enabled`, `serviceNames`, `attributeKeys`, `upstream.http`, `upstream.grpc` — see [Configuration](configuration.md#prologueopentelemetry) |

## How they combine

Any source family can run alone. When several are enabled, the correlator uses the
[correlation window](../concepts.md#how-captures-get-correlated) and trace identifiers where available to
associate currently unclaimed observations. The association is provisional: database transactions do not carry
trace identifiers in the current contract and therefore correlate with HTTP commands by time.

## Next

- **[Configuration](configuration.md)** — every property, with defaults.
- **[How Prologue works](../concepts.md)** — the correlation heuristic in depth.

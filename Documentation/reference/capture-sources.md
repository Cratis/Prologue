---
title: Capture sources
description: What triggers a capture for each source the Extractor watches, and the configuration specific to it.
---

The Extractor runs up to four capture sources at once. Every one of them captures **metadata, not data** — see [Why Prologue](../why-prologue.md#what-prologue-captures--and-doesnt).

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
| Captures | Method, path, response status, and the W3C `traceparent` trace id if present, for `POST`, `PUT`, and `DELETE` requests only |
| Never captures | `GET`/read traffic, request or response bodies |
| Config | Plain YARP `routes`/`clusters` configuration — see [Configuration](configuration.md#root-shape) |

## OpenTelemetry

| | |
|---|---|
| Enabled by | `prologue.openTelemetry.enabled: true` |
| Mechanism | An OTLP proxy (HTTP and gRPC). Telemetry is captured, then forwarded on to `upstream.http`/`upstream.grpc` unchanged — or nowhere, if you leave both empty, making the Extractor a terminal collector |
| Captures | Span, metric, and log metadata: service name (if it matches `serviceNames`, or all services if that's empty) and the values of an allowlisted set of `attributeKeys` |
| Never captures | Attribute keys not in the allowlist, full span/log payloads |
| Config | `enabled`, `serviceNames`, `attributeKeys`, `upstream.http`, `upstream.grpc` — see [Configuration](configuration.md#prologueopentelemetry) |

## How they combine

None of these run in isolation — the correlator groups whatever fires within the same [correlation window](../concepts.md#how-captures-get-correlated) into one capture, regardless of which sources contributed. A typical write request produces an HTTP capture *and* one or more database-change captures for the same act; OpenTelemetry adds the trace context that ties them together even when the timing alone wouldn't.

## Next

- **[Configuration](configuration.md)** — every property, with defaults.
- **[How Prologue works](../concepts.md)** — the correlation heuristic in depth.

# Prologue Extractor

Prologue Extractor points at an existing ("brownfield") system and captures **what changes**, from two angles
at once, to help reverse-engineer the commands a system supports and the data changes they cause:

1. **Database change capture** — watches SQL Server *and* PostgreSQL and records, **per transaction**,
   which tables and columns changed. **Metadata only** — never the actual data values.
2. **HTTP command capture** — sits in front of the system as a **YARP reverse proxy** and records the
   `POST` / `PUT` / `DELETE` operations passing through (method, path, status, timing).
3. **OpenTelemetry capture** — acts as an **OTLP proxy** (HTTP on `4318`, gRPC on `4317`). Point the monitored
   system's OTLP exporter at the extractor: it captures span **metadata** (name, kind, timing, trace/span ids,
   attribute keys, and the values of an allowlisted set of attributes) and forwards the telemetry unchanged to the
   upstream collector when one is configured. Traces carry the **intent** (commands) and the events they produce.

The streams are **correlated** by a time-window heuristic and by shared **trace id**, then sent to the configured
output (the **Prologue Receiver** or rolling JSON files).

## Architecture

```
[SqlServerChangeSource] ─┐
[PostgresChangeSource]  ─┼─► IObservationChannel ─► TimeWindowCorrelator ─► ApiCaptureStore ─► Prologue Receiver ─► Mongo
[CommandCaptureTransform]┘        (per source)        (groups by time)       (POST /captures)
```

- **`Observation`** — the common unit every source emits (`Source`, `Occurred`, polymorphic `Payload`).
- **`ICaptureSource`s** publish observations to **`IObservationChannel`** — the extension seam. A new kind
  of source only needs to publish observations here; nothing downstream changes.
- **`ICorrelator`** (`TimeWindowCorrelator`) groups observations into a **`Capture`** — a command plus the
  database transactions committed within the window become one capture.
- **`ICaptureStore`** (`ApiCaptureStore`) sends captures to the Prologue Receiver. `ObservationPayload`s are
  polymorphic, so new source kinds add new entry types without breaking the schema.

The capture contract (`Capture`, `Observation`, payloads, `SourceKind`) lives in **`Cratis.Prologue.Contracts`**, shared
with the Prologue Receiver.

## Configuration (`appsettings.json` → `Prologue` section)

- `Api.Endpoint` — the base address of the Prologue Receiver captures are posted to.
- `Correlation.WindowMilliseconds` — the correlation window.
- `SqlServer[]` / `Postgres[]` — one entry per database to watch (connection string + source name).
- `ReverseProxy` (standard YARP section) — the catch-all route/cluster pointing at the monitored system.

## Prerequisites on the target databases

- **SQL Server** — CDC must be enabled (`sql/enable-sqlserver-cdc.sql`); requires SQL Server Agent.
- **PostgreSQL** — `wal_level = logical`; the connecting role needs the `REPLICATION` attribute. The extractor
  creates the publication and replication slot it consumes if they do not already exist.

## Running locally

`Source/docker-compose.yml` provides `prologue-sqlserver` (CDC-ready) and `prologue-postgres`
(`wal_level=logical`) alongside MongoDB. Run the Prologue Receiver, point the proxy's destination at your target
system, enable CDC on the SQL Server tables, run the extractor, then issue `POST`/`PUT`/`DELETE` requests through
the proxy and inspect the `captures` collection in MongoDB.

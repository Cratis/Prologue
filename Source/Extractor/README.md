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
  `SqlServer[].EnableChangeDataCapture` (default `true`) lets the extractor turn CDC on itself; `SqlServer[].Tables`
  narrows it to named tables (`schema.table` or bare `table`) instead of every user table.
- `PrologueId` — the Prologue captures belong to. When set, captures are stamped with it and posted to the
  Receiver's Prologue-scoped endpoint so they can later be interpreted on their own.
- `ReverseProxy` (standard YARP section) — the catch-all route/cluster pointing at the monitored system.

## Preparing the target databases

**The extractor prepares the databases itself.** A system being captured was built without knowing Prologue
exists, so it must not have to carry setup code for the tool watching it — that is the whole point of being able
to point Prologue at software that already exists. What is left for an operator is only what a database connection
genuinely cannot do:

| | The extractor does | You must do |
|---|---|---|
| **SQL Server** | Enables CDC on the database and on every user table with a primary key, skipping what is already enabled (`SqlServer[].EnableChangeDataCapture`, on by default; narrow it with `SqlServer[].Tables`). Needs `sysadmin`. | Have **SQL Server Agent running** — CDC captures nothing without it. |
| **PostgreSQL** | Creates the publication and replication slot it consumes, and checks the server is actually usable before it starts. | Run the server with **`wal_level = logical`** (needs a restart) and give the connecting role the **`REPLICATION`** attribute. |

Neither of the remaining two can be changed over a normal connection, so the extractor detects them and says
exactly what is wrong rather than idling silently. If CDC cannot be enabled — no `sysadmin`, or a DBA has done it
already — the extractor logs a warning and watches whatever capture instances it finds;
`sql/enable-sqlserver-cdc.sql` is there for that case.

## Running locally

The quickest way to see the extractor working is the **Library sample** in [`Samples/Library`](../../Samples/Library):
an ordinary ASP.NET + EF Core system with an Aspire composition that already wires everything up — the database
(PostgreSQL with `wal_level=logical`, or SQL Server with the Agent enabled for CDC), the extractor in front of the
system, the Receiver, and MongoDB. It also generates realistic load on demand.

```shell
cd Samples/Library && aspire run                     # PostgreSQL
cd Samples/Library && aspire run -- --database mssql # SQL Server
```

To wire it up by hand instead: run the Prologue Receiver, point the proxy's destination at your target system,
enable CDC on the SQL Server tables (`sql/enable-sqlserver-cdc.sql`), run the extractor, then issue
`POST`/`PUT`/`DELETE` requests through the proxy and inspect the `captures` collection in MongoDB.

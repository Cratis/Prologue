# Prologue Extractor

Prologue Extractor points at an existing ("brownfield") system and captures **what changes**, from several
signals, to propose commands and the data changes they may have caused:

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

```text
SQL Server / PostgreSQL hosted services ─┐
HTTP reverse-proxy transform             ├─► IObservationChannel ─► CorrelationWorker
OTLP HTTP and gRPC endpoints             ┘                              │
                                                                        ▼
                                                              TimeWindowCorrelator
                                                                        │
                                                                        ▼
                                                             BufferedCaptureStore
                                                                        │
                                                                        ▼
                                                                  CaptureBuffer
                                                                        │
                                                                        ▼
                                                             CaptureOutputWorker
                                                               ├─► JSONL files
                                                               └─► Receiver API
```

- **`Observation`** is the common unit each producer publishes: a source, timestamp, and polymorphic payload.
- **`IObservationChannel`** is the in-process producer/consumer boundary between the configured producers and
  `CorrelationWorker`.
- **`ICorrelator`** is implemented by `TimeWindowCorrelator`, which groups settled observations into a
  `Capture` using trace identifiers where available and a configured time window otherwise.
- **`ICaptureStore`** is implemented by `BufferedCaptureStore`. It enqueues captures so output I/O does not block
  observation capture.
- **`ICaptureOutput`** writes rolling JSONL files through `JsonFileCaptureOutput` or posts captures through
  `ApiCaptureOutput`, as selected by configuration.

The capture contract (`Capture`, `Observation`, payloads, and `SourceKind`) lives in
**`Cratis.Prologue.Contracts`**, shared by the Extractor, Receiver, storage, and Interpreter.

### Adding another observation source

`IObservationChannel` is a useful in-process seam, but Prologue does not currently provide binary plug-in discovery
or an external source SDK. Source composition is explicit in `Program.cs` because the existing producers have
several hosting shapes: background services, a YARP transform, and mapped OTLP HTTP/gRPC endpoints.

A new source must be reviewed across the complete pipeline:

1. define configuration and startup registration;
2. publish bounded, classified metadata as `Observation` instances;
3. add any payload type to the canonical JSON discriminator set;
4. define correlation behavior and failure/recovery semantics;
5. verify JSON and MongoDB persistence compatibility;
6. teach the Interpreter how to use or explicitly ignore the new evidence; and
7. add denied-data, serialization, correlation, and interpretation specifications.

Do not force a new source into a relational table/column payload when its native semantics differ. Captured output
is evidence for a provisional model, not automatic domain truth.

## Configuration

The Extractor reads `cratis-prologue.json` from its working directory. Set `PROLOGUE_CONFIG` to use another path.
The file provides the baseline and environment variables override it using the normal double-underscore form.

- `Prologue.Output.Kind` selects `Json` or `Api` output.
- `Prologue.Output.Json.Directory` selects the JSONL capture directory.
- `Prologue.Output.Api.Endpoint` selects the Receiver base address.
- `Prologue.Correlation.WindowMilliseconds` sets the correlation window.
- `Prologue.SqlServer[]` / `Prologue.Postgres[]` configure databases to watch.
  `SqlServer[].EnableChangeDataCapture` defaults to `true`; `SqlServer[].Tables` narrows capture to named tables.
- `Prologue.PrologueId` associates captures with one interpretation session.
- `Prologue.OpenTelemetry` configures OTLP capture, filtering, allowlisted attribute values, and optional forwarding.
- `ReverseProxy` is the standard YARP route/cluster configuration for the monitored HTTP application.

HTTP observations include the query string, and OpenTelemetry observations can include values for explicitly
allowlisted attributes. Treat configuration and capture output as sensitive, minimize the allowlist, and exclude
credentials, personal data, and other unrestricted values.

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

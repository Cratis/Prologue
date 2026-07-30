# Library — a system for Prologue to capture

An ordinary library system: authors, members, a book catalog, inventory, reservations, and lending. Plain
ASP.NET Core MVC controllers, Entity Framework Core, incrementing integer primary keys — **no Cratis constructs
anywhere in it**. That is the point. Prologue exists to be pointed at systems that were built without knowing
Prologue would ever exist, so the sample it ships with has to be one of those.

The Aspire composition puts the whole capture pipeline around that system and can drive realistic load through it,
so you can watch the Extractor work end to end without finding a legacy system of your own first.

## Running it

```shell
cd Samples/Library
aspire run                        # PostgreSQL (the default)
aspire run -- --database mssql    # SQL Server
```

The dashboard is always at **<http://localhost:18880>**, with no login token — a fixed address you can keep open in
a tab across restarts, rather than a fresh port and a fresh token every run. That is a deliberate convenience for a
sample that is meant to be started and stopped constantly; it is not how you would configure a dashboard anyone
else can reach.

Not Aspire's usual 18888: the Cratis Studio docker-compose stack publishes its own dashboard there, and two things
on one port means whichever starts second fails to bind. 18881–18883 alongside it carry OTLP and the resource
service. Change them in `Composition/Properties/launchSettings.json` if they clash with something of yours. On the **core** resource there is a **Simulate load** command — click it, say how
many transactions you want (10 000 by default), and the library starts behaving like a system in real use. Watch
the captures accumulate in MongoDB.

Everything is a project reference; nothing is pulled as a Docker image except the databases and MongoDB.

## Configuring a Prologue with the CLI or Studio

`aspire run` already configures and starts the Extractor for you — `Composition/AppHost.cs` hands it whichever
database is running, connection string and all. Reach for this section only when you want to point the
**standalone Cratis CLI** or **Cratis Studio** at this sample's database directly, instead of the one Aspire
already wired — useful for trying the interactive tooling against a real target without standing up your own
system first.

Both tools ask for the same thing: a connection string to the database to watch. `Composition/AppHost.cs` pins the
password and host port for `postgres`/`sqlserver` rather than letting Aspire assign them, so these stay correct
for as long as the sample runs on `localhost` — whichever engine `aspire run` starts is reachable at:

```text
# PostgreSQL — database name is `library`
Host=localhost;Port=15432;Username=postgres;Password=PrologueSample1!;Database=library
```

```text
# SQL Server — database name is `LibraryDb`
Server=localhost,11433;User ID=sa;Password=PrologueSample1!;TrustServerCertificate=true;Database=LibraryDb
```

Neither engine needs any extra preparation for this sample specifically: `AppHost.cs` already starts PostgreSQL
with `wal_level=logical` and SQL Server with the Agent enabled, and the default accounts (`postgres`, `sa`)
already hold the permissions their capture method needs — `REPLICATION` for logical replication, `sysadmin` for
CDC. A system you point Prologue at yourself won't have those for free; see the Extractor's own
[prerequisites](../../Source/Extractor/README.md#preparing-the-target-databases) for what a real target has to
satisfy.

### Cratis CLI

```shell
cratis prologue start
```

An interactive wizard: pick **PostgreSQL** or **SQL Server** as a source, paste the connection string above, and
choose where captures go — a folder of `.jsonl` files is the simplest starting point. It writes a
`cratis-prologue.json` and prints the `docker run` command that starts the Extractor as a sidecar. See the
[CLI reference](https://cratis.io/cli/reference/prologue/) for every option.

### Cratis Studio

The Prologue setup wizard in [Cratis Studio](https://github.com/Cratis/Studio) walks the same decisions
visually: name the Prologue, choose **Database** as a source, paste the connection string, and select **Connect**
— Studio reads the schema and lets you narrow the capture to specific tables before generating
`cratis-prologue.json` for you to download.

Either way, the full property list behind `cratis-prologue.json` — including how to route output straight to a
Receiver instead of files — is in
[Point Prologue at your system](../../Documentation/guides/point-prologue-at-your-system.md).

## What runs

```text
 browser / simulation ──▶ Extractor (proxy :8080) ──▶ Core ──▶ PostgreSQL or SQL Server
                              ▲                        │                │
                 OTLP :4317 / :4318                    └─── telemetry ───┘
                              │
                         Receiver ──▶ MongoDB
```

| Resource | What it is |
|---|---|
| `postgres` / `sqlserver` | The library's database. PostgreSQL runs with `wal_level=logical`; SQL Server runs with the Agent enabled so CDC works. |
| `core` | The library system — REST API, the Razor frontend, the simulation engine. |
| `web` | The React frontend, served by Vite. |
| `extractor` | `Source/Extractor` — reverse proxy in front of Core, plus the OTLP receiver. |
| `receiver` | `Source/Receiver` — takes correlated captures and stores them. |
| `mongo` | Where the captures land. |

All three capture sources are live at once:

- **HTTP commands** — everything reaches Core through the extractor's reverse proxy, so state-changing requests are
  captured. Traffic sent straight at Core is invisible to Prologue; that is why the simulation and the React app
  are both pointed at the proxy.
- **Database changes** — CDC on SQL Server, logical replication on PostgreSQL. **The library system knows nothing
  about any of this.** The extractor enables CDC on the database itself and creates its own PostgreSQL publication
  and replication slot; the composition only supplies what a connection cannot change — the SQL Server Agent being
  on, and `wal_level=logical`. Nothing in `Core` mentions Prologue, which is exactly the point: it stands in for a
  system that already existed before Prologue did.
- **Telemetry** — Core exports OTLP to the extractor, which captures trace, metric, and log metadata and forwards
  everything on to the Aspire dashboard, so the dashboard still shows what it always shows.

Captures are stamped with a fixed Prologue id, so repeated runs accumulate against the same Prologue and can be
interpreted together.

## Two frontends, on purpose

| | Razor (`Core/Pages`) | React (`Web`) |
|---|---|---|
| Rendering | Server-rendered, full page load per navigation | SPA, client-side routing |
| Mutations | `<form method="post">` → page handler → redirect | `fetch` to the REST API |
| What Prologue captures | the browser's form POST to the page | the SPA's XHR to `/api/...` |

They look identical — one shared stylesheet, one shared markup and `data-testid` contract (see
[`Shared/FRONTEND-CONTRACT.md`](Shared/FRONTEND-CONTRACT.md)) — and behave completely differently. That contrast is
the interesting part: the same business behavior produces two quite different capture shapes, which is exactly the
variety the Interpreter has to cope with in the wild.

The shared stylesheet lives once at `Shared/library.css`. Core links it into `wwwroot`; the Vite app imports it
from the same file. Neither keeps a copy, so they cannot drift.

## Projects

| Project | What it is |
|---|---|
| `Core` | The library system: entities, EF Core mapping, controllers, the Razor frontend, seed data, and the simulation engine. |
| `Web` | The React + TypeScript + Vite frontend. |
| `ServiceDefaults` | Standard Aspire service defaults, plus the library's own tracing and metrics sources. |
| `Composition` | The Aspire AppHost — every resource above, and the dashboard commands. |
| `Tests` | Aspire Testing spins up the whole composition; one Playwright suite drives both frontends. |

## The business rules worth watching

Two of them exist to produce rejections, because a capture of a real system is full of them and a simulation
without any would be misleadingly tidy:

- Deleting an author who still has books in the catalog is refused — **409**.
- Reserving or lending a title with no copies on the shelf is refused — **422**.

Both show up as non-2xx HTTP commands with an error span and **no** database transaction, which is precisely what a
captured rejection looks like.

## Tests

```shell
dotnet test Samples/Library/Tests/Tests.csproj --filter "Category=Integration"
```

These need Docker and Playwright browsers, so CI does not run them — it builds them and runs the framework's own
specs instead. The suite starts the full composition, drives both frontends through the shared `data-testid`
contract, runs a simulation, and then asserts the captures actually reached MongoDB.

## Keys

The catalog is keyed the way a system of this vintage usually is: an incrementing integer surrogate primary key on
every table, with the ISBN kept as a unique business identifier that the API routes use
(`/api/catalog/books/{isbn}/tags`). Integer keys are the norm in the systems Prologue gets pointed at, and they
give the Interpreter something more realistic to reason about than tidy GUIDs.

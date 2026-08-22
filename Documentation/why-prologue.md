---
title: Why Prologue
description: How Prologue adds observed evidence to existing-system modeling, what it cannot recover, and when it is the wrong tool.
---

## Without Prologue

You've decided to bring an existing system into Cratis — event-sourced, with Chronicle recording what happens and Arc exposing typed commands and queries. Before any of that, you need an event model: what are the commands, what facts do they produce, what read models does the system actually need? For a system you're building from scratch, that's a design conversation. For a system that already exists, it's an investigation.

So you read the code — controllers that have accreted logic for years, stored procedures nobody documented, a database schema shaped more by migrations than by intent. You interview whoever's still around, and take their answers on faith, because the system in front of you and the system they remember have quietly drifted apart. You produce a model. It's a reasonable model. It's also a guess — built from what the system's *authors* say it does, not from what it *actually does* under real traffic.

## With Prologue

Prologue adds observed evidence to that investigation. It stands beside the running system and watches selected
signals without requiring application code changes:

- **HTTP commands** — the state-changing requests (`POST`/`PUT`/`DELETE`) that reach it.
- **Database changes** — which tables and columns changed, in the same transaction, right after each command.
- **Telemetry** — the traces, metrics, and logs it already emits, if it emits any.

It correlates those streams into **captures** using trace identifiers where available and timing otherwise. The
Interpreter uses the accumulated evidence to propose modules, features, and slices with candidate commands,
events, read models, and projections. Timing can associate unrelated operations, observed traffic can omit valid
behavior, and implementation structure does not reveal domain intent. Treat the result as another input to human
modeling, not a recovered specification.

## What Prologue captures — and doesn't

Prologue excludes database row values and HTTP request and response bodies. It records changed table and column
names, endpoint paths and results, and telemetry metadata. Endpoint paths include query strings, and configured
OpenTelemetry attribute keys include their values. Identifiers and operation names can also be sensitive.

Minimize source access and attribute allowlists, exclude credentials and personal data, and protect capture files
and persisted observations. These signals provide partial structural evidence useful when proposing an event
model; they are not the complete intent an event model needs.

## When Prologue is the wrong fit

- **You're starting from nothing.** A greenfield system has no existing behavior to observe — design the event model directly with the [event-modeling](/screenplay/) approach instead of capturing traffic that doesn't exist yet.
- **You need business meaning Prologue never saw.** Prologue's heuristics — and the optional LLM refinement step — can only reshape and name what the captures actually contain. A rule the system enforces silently, without a distinguishing HTTP response or database write, leaves no evidence for Prologue to correlate; you'll still need a human to fill that in.
- **The system has no addressable write path.** Prologue's HTTP capture watches a reverse proxy in front of state-changing requests. A system that mutates state only through batch jobs, message queues, or direct database writes with no HTTP surface won't produce HTTP captures — database change capture alone can still work, but the picture is thinner.
- **You need the source data.** Prologue excludes database row values and HTTP bodies. If your goal is data migration rather than model discovery, use a tool designed for that responsibility.

## What you get out

An `ExtractionResult` — a structured tree rather than prose — plus a generated [Screenplay](/screenplay/) `.play`
file. Both are provisional starting points that require review, correction, compiler validation, and verification
against intended behavior before you rely on them.

## Next

- **[How Prologue works](concepts.md)** — the capture, correlation, and interpretation pipeline in depth.
- **[Getting started](getting-started.md)** — watch it work against the bundled sample system.

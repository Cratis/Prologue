---
title: Why Prologue
description: The problem Prologue solves — turning an existing system's real behavior into an event model without reverse-engineering it by hand — and when it isn't the right tool.
---

## Without Prologue

You've decided to bring an existing system into Cratis — event-sourced, with Chronicle recording what happens and Arc exposing typed commands and queries. Before any of that, you need an event model: what are the commands, what facts do they produce, what read models does the system actually need? For a system you're building from scratch, that's a design conversation. For a system that already exists, it's an investigation.

So you read the code — controllers that have accreted logic for years, stored procedures nobody documented, a database schema shaped more by migrations than by intent. You interview whoever's still around, and take their answers on faith, because the system in front of you and the system they remember have quietly drifted apart. You produce a model. It's a reasonable model. It's also a guess — built from what the system's *authors* say it does, not from what it *actually does* under real traffic.

## With Prologue

Prologue replaces the guess with evidence. It stands beside the running system and watches three things it already does, without you changing a line of its code:

- **HTTP commands** — the state-changing requests (`POST`/`PUT`/`DELETE`) that reach it.
- **Database changes** — which tables and columns changed, in the same transaction, right after each command.
- **Telemetry** — the traces, metrics, and logs it already emits, if it emits any.

It correlates those three streams into **captures** — a command plus the database transactions it caused, read as one act — and interprets the accumulated captures into an event model: modules, features, and slices, with the commands, events, read models, and projections a Cratis application would need to reproduce that same behavior. The model comes from what the system did, across however much real traffic you point Prologue at, not from what someone remembers it doing.

## What Prologue captures — and doesn't

Prologue captures **metadata, not data**: which tables and columns changed, which endpoint was called and with what result, which spans fired — never a row's actual contents, never a request body. That's a deliberate boundary, not a current limitation: a capture describes the *shape* of what happened, which is exactly what an event model needs and exactly what you don't want travelling through a tool that sits in front of production traffic.

## When Prologue is the wrong fit

- **You're starting from nothing.** A greenfield system has no existing behavior to observe — design the event model directly with the [event-modeling](/screenplay/) approach instead of capturing traffic that doesn't exist yet.
- **You need business meaning Prologue never saw.** Prologue's heuristics — and the optional LLM refinement step — can only reshape and name what the captures actually contain. A rule the system enforces silently, without a distinguishing HTTP response or database write, leaves no evidence for Prologue to correlate; you'll still need a human to fill that in.
- **The system has no addressable write path.** Prologue's HTTP capture watches a reverse proxy in front of state-changing requests. A system that mutates state only through batch jobs, message queues, or direct database writes with no HTTP surface won't produce HTTP captures — database change capture alone can still work, but the picture is thinner.
- **You need the exact data, not its shape.** Prologue is deliberately metadata-only. If your actual goal is a data migration rather than an event model, that's a different tool.

## What you get out

An `ExtractionResult` — a real, structured tree, not prose — plus a generated [Screenplay](/screenplay/) `.play` file you can run immediately in a local Stage sandbox or bring into Studio to keep shaping. It's a starting point evidenced by real traffic, meant to be reviewed and refined, not a finished model handed down from on high.

## Next

- **[How Prologue works](concepts.md)** — the capture, correlation, and interpretation pipeline in depth.
- **[Getting started](getting-started.md)** — watch it work against the bundled sample system.

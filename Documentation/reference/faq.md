---
title: Troubleshooting and FAQ
description: Common problems when running Prologue, and what usually causes them.
---

## No captures are appearing

- **HTTP captures missing.** Confirm your traffic is actually going through the Extractor's proxy port (`8080` by default), not straight at your system — anything that bypasses the proxy is invisible to Prologue. Also check the request method: only `POST`, `PUT`, and `DELETE` are captured, so a `GET`-only smoke test produces nothing to see.
- **SQL Server captures missing.** Confirm the connection string's account can enable Change Data Capture (or that CDC is already enabled, if you set `enableChangeDataCapture: false`), and that a `tables` allowlist, if you set one, actually includes the table you changed.
- **PostgreSQL captures missing.** Confirm the server has `wal_level = logical` — the Extractor can create its own replication slot and publication, but it can't change that server setting for you, and logical replication won't work without it.
- **OpenTelemetry captures missing.** Confirm `openTelemetry.enabled` is `true`, and that your service's name matches `serviceNames` (or that list is empty, to capture every service).

## The Interpreter says it found no capture files

Batch mode reads `.jsonl` files from the folder in `--captures`/`PROLOGUE_CAPTURES` (`/captures` by default in the image). This usually means the Extractor is configured for `output.kind: "Api"` (posting to a Receiver) rather than `"Json"`, or the mounted folder doesn't match where the Extractor is actually writing — see [Configuration](configuration.md#prologueoutput).

## Captures from two different Prologues are mixed together

Every capture is stamped with a `prologueId`. If a folder or a Receiver's MongoDB collection holds captures from more than one Prologue, pass `--prologue-id`/`PROLOGUE_ID` to the Interpreter to interpret just the one you want — see [Running the Interpreter](../guides/running-the-interpreter.md).

## The event model has mechanical, generic names

That is a provisional heuristic result with mechanical names. Configure the `llm` section in
`cratis-prologue.json` to have the Interpreter refine names and derive a system name; see
[Configure LLM refinement](../guides/running-the-interpreter.md#configure-llm-refinement). Without a configured
model, the Interpreter does not apply language-model refinement. The heuristics still infer candidate structure
from incomplete evidence, so review the result in either mode.

## A service-mode Interpreter container keeps exiting

That is by design, not a crash. A service-mode session waiting on your answer past `PROLOGUE_GRACE_PERIOD`, or
sitting idle past `PROLOGUE_IDLE_TIMEOUT`, exits cleanly. An orchestrator can restart the process and resume the
session from its last MongoDB-backed checkpoint. This lifecycle does not apply to batch mode.

## Where to ask

The [Cratis Discord](https://discord.gg/kt4AMpV8WV) is the fastest way to get unstuck if this page doesn't cover it.

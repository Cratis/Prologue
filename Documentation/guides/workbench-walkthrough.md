---
title: Workbench walkthrough
description: The seven steps of the Prologue Workbench, which ones are optional, what each needs and produces, and how they connect.
---

The Workbench has seven steps, shown in the sidebar in four groups. You work through them in order, and you can come back to any of them.

| # | Step | Group | Optional | What it produces |
|---|---|---|---|---|
| 1 | Systems | Setup | No | The definition of the prologue and its `cratis-prologue.json` |
| 2 | Discover | Evidence | Yes | A map of what the code does, the decisions it makes and what talks to what |
| 3 | Capture | Evidence | Yes | Captures, command coverage, business decisions and scenario scripts |
| 4 | Explore | Interpret | No | The scope the interpretation works from |
| 5 | Events | Interpret | No | Reviewed event suggestions and an accepted model |
| 6 | Model | Interpret | No | The provisional event model and its Screenplay |
| 7 | Rebuild | Modernize | Yes | A board of slices, each moving from observed to cut over |

## Optional steps

Discover, Capture and Rebuild can be turned off for a prologue under Systems. A step that is off shows a banner saying what you lose: without Discover, events come from runtime capture only; without Capture, events come from the code alone and none of them is backed by observed behavior; without Rebuild, nothing tracks the slices you build from the model.

Discover and Capture are the two kinds of evidence, so **at least one of them has to stay on**. The Workbench refuses to turn off the last one.

## 1. Systems

*Needs:* nothing.
*Produces:* the definition everything else reads.

Add each system that makes up the prologue and say where its code lives: a GitHub, Azure DevOps or GitLab repository with a branch and an optional sub path, or a folder on the Receiver's disk. Say which evidence to capture: databases (SQL Server or PostgreSQL, with the secret name of the connection string), HTTP clusters, telemetry services and the attribute keys to allow. Choose whether the Extractor posts to the Receiver or writes JSON files, and the correlation window. The page shows the resulting `cratis-prologue.json`.

Connections to code hosts are covered in [Repository connections](../reference/repository-connections.md).

## 2. Discover

*Needs:* a system with a code location, and AI providers set up (see [AI setup](../reference/ai-setup.md)).
*Produces:* code findings.

The Receiver clones the system's repository, or reads the local folder, and reads its source files. Three agents work on them:

- The **Dramaturg** maps the code into modules and capabilities.
- The **Prompter** finds the decision points: where the code applies a rule, refuses, or changes state, with file and line.
- The **Usher** traces dependencies between services, tables and endpoints.

The result is an application map. Routes found in the code, such as `{id:int}` or `{isbn}`, are written in the same command shape format that captures use, so a finding can be matched to what the running system did. **Share map** creates a read-only link to the map.

**Analyze folder** analyzes any folder the Receiver can read, without a system; see [Security and exposure](running-the-workbench.md#security-and-exposure).

## 3. Capture

*Needs:* the Extractor running with an `Api` output pointed at the Receiver.
*Produces:* a live feed of captures and what is derived from them.

Each capture is one HTTP command plus the database transactions and spans correlated with it. The Receiver stores it as before and records a summary of it for the Workbench: the command, its shape, the table changes (schema, table, operation, columns), the span count and whether the spans were linked by trace. No row values are recorded.

From the captures the page derives:

- **Aggregates and a live feed** of the most recent captures with their outcomes.
- **Shape coverage** per command shape, set against the shapes Discover found in the code.
- **Business decisions**: each command shape's accepted and refused outcomes. When Discover has found a decision point for the shape, its reason is attached, with the file and line it came from. Where there is none, the decision stands as observed, without a reason.
- **Scenarios**: label what you are doing and the captures made meanwhile carry the label. A scenario script lists the scenarios you intend to run.

Database transactions that happen with no HTTP command (a job, a trigger) are recorded too, with an empty command.

## 4. Explore

*Needs:* captures, code findings, or both.
*Produces:* the interpretation scope.

Captures are grouped into command shapes. The matrix shows, per shape, which tables each command writes, with the statuses it answered and the columns it touched. A partial cell means the write happens only in some runs. Explore also shows journeys across commands and the uncorrelated writes.

You decide what the interpreter works from: **exclude** a shape as noise, or **split** a shape by status so accepted and refused outcomes are interpreted separately. The next step uses exactly this scope.

## 5. Events

*Needs:* evidence, and an AI provider for the Interpreter if you want refined names.
*Produces:* an accepted model.

Run the interpretation. The Interpreter proposes past-tense events from the evidence in scope. Mechanical names come from tables and routes, refined names from the language model. When it cannot decide it asks you a question, and your answer is recorded and applied. For each suggestion you accept, reject or rename it; nothing becomes part of the model until you accept it. You can also mark boundaries between features and set aside things that are not events.

Not every suggestion rests on captured commands. Two kinds of evidence the captures cannot give are suggested too, and they go through the same accept, rename, reject and restore review:

- **Code only.** A capability Discover mapped in the code whose command was never captured becomes a suggestion marked **Code only**, with low confidence. Its evidence is the place in the code (file and line when Discover found a decision point there) and the note "Found only in code; no capture backs this yet." The name is derived from the route, for example `POST /api/loans/{id}/renew` gives `LoanRenewed`. When the command is captured later, the captured suggestion takes over and keeps your decision and any name you chose; when the capability disappears from the code, the suggestion is withdrawn but not lost.
- **No command.** Tables written by transactions with no HTTP command (a nightly job, for example) are grouped in Explore. When Discover found a job that mentions the table, the Events step suggests an event tied to that job and marked **No command**, named from the job (`InventoryReconciled`). When no code explains the writes, the Interpreter asks, for example "Reminders changes 4 times with no command, mostly around 03:00. What is that?" You choose *A scheduled job* (suggest an event), *Staff correcting data* (suggest a correction event) or *Ignore*. Your answer is recorded and the interpretation runs again with it.

Both kinds count in the filters, link their evidence to Discover (code) or Explore (table), and appear on the Model board as dashed, open events until you accept them.

## 6. Model

*Needs:* accepted events.
*Produces:* the provisional event model and its Screenplay.

Accepted events are laid out as slices along the story of the system. Dashed outlines are still open. Select a slice to see its evidence and the Screenplay it produces. The model can be copied or downloaded as a `.play` file, and `GET /prologues/{prologueId}/model.play` returns the same text (see the [Receiver API](../reference/receiver.md#endpoints)).

The file holds accepted slices and the slices that still have open suggestions. A slice with open suggestions is marked as a draft: its description starts with `Draft: not yet accepted.` followed by the names of the open suggestions, so a reader of the `.play` file can tell what is settled. The download is served as `text/plain; charset=utf-8` with `Content-Disposition: attachment; filename="model.play"` and `Cache-Control: no-store`, so it always reflects the current model. When nothing is accepted or open yet, the response is still `200` with an empty body.

**Accept model for rebuild** hands the model to the Rebuild step. It creates a Rebuild slice for each accepted slice that replaces at least one captured command (the slice's command shapes), and holds back slices that still have open suggestions, so nothing half-decided becomes work to cut over. A slice that Rebuild already tracks by name is left alone, so accepting again creates nothing twice. It also gives drift detection the accepted model to compare new captures with.

## 7. Rebuild

*Needs:* an accepted model, and the Extractor configured for [shadowing](../reference/shadowing.md) when you reach that stage.
*Produces:* a board of slices that move to cut over one at a time.

A slice replaces one or more legacy commands and moves through five stages: **Observed**, **Modeled**, **Built**, **Shadowing**, **Cut over**. Moving it back a stage is allowed. While a slice is shadowing, the Extractor mirrors the legacy commands it replaces to the new system and reports whether both decided the same way. Each disagreement becomes a difference that you either accept or mark to be matched to the legacy behavior. A slice cannot be cut over while differences are open, or before it has been compared with the legacy system at least once.

Drift detection compares new captures with the accepted model and lists shapes and columns the model does not know, so you can acknowledge them and have events suggested for them.

## How the steps connect

- **Code findings are matched to captures** by command shape. Discover writes a route as the same `METHOD /path` shape that a capture has, so the Capture step can set the shapes the code is known to have against the shapes that were actually observed.
- **Decisions keep their reasons.** A decision observed in captures is paired with the decision point Discover found in the code. The Events step uses both.
- **Scope feeds interpretation.** What you exclude or split in Explore is exactly what the Interpreter reads in Events.
- **The accepted model feeds Screenplay and Rebuild.** Accepted events become the Model; the model becomes a `.play` file and, once accepted for rebuild, the slices on the Rebuild board.

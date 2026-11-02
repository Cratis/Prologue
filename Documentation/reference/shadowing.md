---
title: Extractor shadowing
description: Mirror legacy commands to a new system and report whether both decided the same way, for the Workbench's Rebuild step.
---

Shadowing lets the Extractor try a rebuilt slice against live traffic. After the legacy system has answered a command, the Extractor sends a copy of the request to the new system, compares the two answers and reports the result to the Receiver as **parity**. The Rebuild step shows it. The legacy system's answer is the one the caller gets; the copy never affects the real request.

## Configure it

Shadowing is a list of rules under `prologue.shadowing` in `cratis-prologue.json`. It is off while the list is empty.

```json
{
    "prologue": {
        "prologueId": "6d7a0b1e-0000-0000-0000-000000000000",
        "output": { "kind": "Api", "api": { "endpoint": "http://receiver:8080" } },
        "shadowing": [
            {
                "shape": "POST /api/loans/{id}/return",
                "destination": "http://new-system:5000/",
                "forwardHeaders": [ "Authorization" ]
            }
        ]
    }
}
```

| Property | Type | Default | Purpose |
|---|---|---|---|
| `shape` | `string` | empty | The command shape to mirror, in the format the Workbench shows (`METHOD /path`). Identities in the request path are generalized to `{id}` before comparing |
| `route` | `string` | empty | An ASP.NET route template matched against the request path, such as `/api/loans/{id}/return`. With both `shape` and `route`, a request has to match both |
| `destination` | `string` | empty | The base address of the new system. Required, and must be an absolute `http` or `https` address |
| `forwardHeaders` | `string[]` | `[]` | Headers to forward in addition to the allowlist (see [Headers and bodies](#headers-and-bodies)) |

Each rule needs a `shape` or a `route`. The first rule that matches a request is used. The Extractor refuses to start when a rule is invalid, or when shadowing is configured without `prologue.prologueId` or without the `Api` output, because parity is reported to the Receiver.

## What is mirrored

- Only commands: `POST`, `PUT`, `PATCH` and `DELETE`. Reads are never mirrored.
- Only when the legacy answer is a decision: a success or a refusal. An answer that is neither (for example an informational response) is not mirrored.
- The copy is sent after the legacy system has answered, so it never delays the real request.

### Headers and bodies

The copy carries only the headers on an allowlist: `Content-Type`, `Content-Encoding`, `Accept`, `Accept-Language`, `Accept-Encoding`, `traceparent`, `tracestate`, `baggage` and `User-Agent`. Every other header is dropped, including `Authorization`, cookies and any API key or token header, because the new system is not the system the caller signed in to and an unknown header can carry anything. A rule lists the further headers it wants sent in `forwardHeaders`; connection-level headers (`Connection`, `Host`, `Content-Length`, `Transfer-Encoding`, `Upgrade` and similar) are never copied, even when listed. Every copy gets the header `X-Prologue-Shadow: true`, so the new system can tell a copy from a real request.

The destination receives the request bodies of the commands it mirrors, so only shadow to a system you control. The path and query are sent exactly as the caller sent them, percent-encoding included.

Request bodies are copied when they are small and their size is known up front. A body larger than 1 MiB, or sent with a chunked transfer encoding, is not buffered, and the request is reported as **Skipped**.

### Limits

| Limit | Value |
|---|---|
| Largest body copied | 1 MiB |
| Mirrored requests in flight | 8 |
| Mirrored requests waiting for a slot | 512; beyond that a request is not mirrored and nothing is reported |
| Request-body bytes waiting for a slot | 64 MiB in total (`prologue.shadowQueueBytes`); a request that would go past it is not copied and is reported as **Skipped** |
| Time the new system has to answer | 10 seconds |
| Time the Receiver has to take an observation | 5 seconds |

## Parity outcomes

The Extractor classifies each answer as an acceptance (2xx and 3xx) or a refusal (4xx and 5xx), and the systems match when they decided the same way, whatever the exact status.

| Outcome | Meaning |
|---|---|
| Matched | Both accepted the command or both refused it |
| Mismatched | They decided differently |
| Unavailable | The new system did not answer, timed out, or answered 502, 503 or 504. This says nothing about its decisions and is never counted as a mismatch |
| Skipped | The command was not mirrored, for example because its body was too large |

## The parity endpoint

The Extractor posts each observation to the Receiver:

```text
POST /prologues/{prologueId}/parity
```

The body is a `ParityObservation`: an id, when the legacy system answered, the method, the path without its query string, the command shape, the trace id (or empty), the legacy status, the shadow status (`0` when it did not answer) and the outcome. It never holds a request or response body. The Receiver answers `202 Accepted` before the observation is recorded; a bounded background queue records it, and an observation that cannot be recorded is logged and dropped, never retried by the Extractor. An id the Receiver has already recorded recently (a retry) is not counted again. See [Receiver API](receiver.md#endpoints).

## Using it from Rebuild

A slice enters the **Shadowing** stage from **Built**. While it is there, parity observations for the commands it replaces are compared against the legacy answers, and each disagreement becomes a difference you accept or mark to be matched. A slice cannot be cut over while differences are open, or before it has been compared at least once. See the [walkthrough](../guides/workbench-walkthrough.md#7-rebuild).

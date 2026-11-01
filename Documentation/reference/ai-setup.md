---
title: AI setup
description: Configure AI providers, pools and agents for the Workbench through Cratis.AI, and what the network posture means.
---

The Workbench does not call any model vendor directly. Every model call goes through [Cratis.AI](https://github.com/Cratis/AI), and you set it up in the Workbench: providers, pools of providers and the four agents that draw on them. Discover and the Interpreter use these agents. Nothing in the Workbench needs AI to start, and a prologue that never runs Discover or asks for refined names works without any.

## Providers

A provider is one connection to a model vendor. Add them in the Workbench's AI settings.

| Vendor | You provide |
|---|---|
| Anthropic | An API key |
| OpenAI | An API key |
| Azure OpenAI | The resource endpoint and key |
| OpenAI-compatible | An endpoint, and optionally a key. Use it for Ollama, vLLM or LM Studio |
| Z.ai | An API key; the endpoint defaults to the Z.ai endpoint |
| OpenAI Codex | Add the provider, then **Connect** it to sign in with ChatGPT |
| GitHub Copilot | Add the provider, then **Connect** it to sign in with GitHub |

Each provider also has a concurrency limit (how many pieces of work run on it at once; zero means no limit) and a usage capacity (a token ceiling within its reporting window; zero means none). Pool selection can rank providers by remaining capacity.

## Pools

A pool is a named group of providers that agents draw from. Each pool has:

- **Tier**: Fast, Balanced, Powerful or Premier.
- **Order**: the members in the sequence you put them.
- **Ordering**: *Most capacity left first*, or *In priority order*.
- **Fallback**: *Continue on the next provider*, or *Pause and ask* when the first choice cannot take the work.

**The Workbench records a pool's tier, order, ordering and fallback, and derives the network posture from them. Cratis.AI dispatch does not enforce the ordering or the fallback.** Cratis.AI's dispatch does not act on them, so treat these settings as a statement of your intent and as the input to the posture, not as a guarantee of which provider serves a request. If the guarantee matters, keep a pool's members all inside your network.

## Agents

There are four agents, each assigned to a pool.

| Agent | Mode | Used by | What it does |
|---|---|---|---|
| Dramaturg | Job | Discover | Maps the code into modules and capabilities |
| Prompter | Job | Discover | Finds the decision points: rules, refusals and state changes, with file and line |
| Usher | Chat | Discover | Traces dependencies between services, tables and endpoints, and confirms them against captured spans |
| Interpreter | Chat | Explore, Events | Names events, refines suggestions and asks you questions |

The agents are known to Cratis.AI by the purposes `prologue-dramaturg`, `prologue-prompter`, `prologue-usher` and `prologue-interpreter`. The mode describes what the Workbench asks of the agent and is shown, not changed.

## Network posture

The Workbench shows, from your pools, how far AI work can leave your network. A provider is **inside** your network when you mark it so, or, with no mark, when it is OpenAI-compatible and its endpoint is private: `localhost`, a loopback or private address (`10.x`, `172.16-31.x`, `192.168.x`, `169.254.x`), a name ending in `.local`, `.lan`, `.internal`, `.intranet`, `.home` or `.corp`, or a bare host name. Every other provider counts as outside until you mark it.

| Posture | Meaning |
|---|---|
| Local only | Every provider in every pool is inside your network, so code and captures stay with you |
| Local first | A pool starts on a provider inside your network, is in priority order, and pauses and asks before falling back to a provider outside it |
| Leaves network | At least one pool can send work to a provider outside your network without asking |

The overall posture is the weakest of the pools that have members. Because ordering and fallback are recorded rather than enforced (see [Pools](#pools)), *Local first* describes the configuration you chose. Only *Local only* is a property of the providers alone.

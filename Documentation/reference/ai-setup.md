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

**The Workbench enforces a pool's order and fallback whenever an agent runs.** Cratis.AI picks the least-used member of a pool and has no notion of your network, so the Workbench puts its own dispatch in front of it. For each call an agent's pool is read, the members are put in order, and each is tried through Cratis.AI as one provider, so Cratis.AI's concurrency limits, rate-limit cooldowns and usage recording still apply.

- **In priority order** tries the members in the order you put them.
- **Most capacity left first** asks Cratis.AI how much of its allowance each member has left and tries the one with the most first. A provider whose capacity Cratis.AI cannot measure counts as fully available, and members that tie keep the order the pool lists them in.
- A member that cannot take the work (unavailable, rate limited, out of capacity or failing) is passed over. With **Continue on the next provider** the pool moves on to the next member.
- With **Pause and ask** the pool moves on from a provider inside your network to another one inside it, but never to a provider outside it. The call stops instead, and the agent's status says which provider would have been next, for example "The Code analysis pool paused before sending work outside your network". Allow the provider on the Systems page, or switch the pool to continue, to carry on.
- A pool that starts on a provider outside your network uses it straight away; the posture shows that as *Leaves network*.

### Daily ceiling

A pool can have a ceiling: the most tokens its agents may use in a UTC day. Cratis.AI has no pool-wide ceiling, so the Workbench keeps it. It counts the tokens each answer reports (providers that report no usage add nothing) and, once the day's use reaches the ceiling, the pool's agents stop with "The Code analysis pool reached its ceiling" until midnight UTC or until you raise or remove it. The pool card shows the ceiling and how much of it is used. The count trails the answers by a moment, so calls already in flight when the ceiling is reached can take a pool slightly over it.

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

The overall posture is the weakest of the pools that have members. The dispatch described under [Pools](#pools) follows the same rules, so the posture describes what actually happens: *Local only* never reaches a provider outside your network, *Local first* stops before it does, and *Leaves network* can use one without asking.

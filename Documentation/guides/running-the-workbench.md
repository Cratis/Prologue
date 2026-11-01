---
title: Running the Workbench
description: Start the Prologue Workbench with Docker Compose, understand what it needs from Chronicle and MongoDB, and point the Extractor at it.
---

The Workbench is the web interface the Receiver serves. It walks one system, or several, from a definition of where the code lives and what to capture through to a provisional event model and a piece-by-piece rebuild. This page starts it; the [walkthrough](workbench-walkthrough.md) explains each step.

## What you need

- Docker with Compose.
- A checkout of this repository, because the Receiver image is built from it.

## Start it

From the repository root:

```bash
docker compose -f Source/Receiver/docker-compose.yml up --build
```

This starts two containers:

| Service | Image | Port | Purpose |
|---|---|---|---|
| `chronicle` | `cratis/chronicle:latest-development` | `35000` | The Chronicle kernel, bundled with a MongoDB replica set |
| `receiver` | built from `Source/Receiver/Dockerfile` | `8080` | The Workbench, the capture endpoints and the parity endpoint |

Open `http://localhost:8080`. The Workbench opens on the Systems step. The Receiver keeps the repositories it clones in the `repositories` volume, mounted at `/var/prologue/repos`.

## Why Chronicle and MongoDB

The Workbench is an event-sourced application. Everything you decide in it, such as systems, connections, scenarios, accepted events and rebuild stages, is recorded as an event in a Chronicle event store named `Prologue`. Its pages read projections of those events, stored as read models in MongoDB.

That gives the Receiver two dependencies that the capture endpoints alone did not have:

- **The Chronicle kernel**, reached through `Cratis:Chronicle:ConnectionString`.
- **MongoDB**, reached through `Cratis:MongoDB:Server`. The Receiver reads the read models from the same database Chronicle writes them to.

The Receiver also keeps writing every capture to its capture collection (`Prologue:Mongo`), exactly as before, because the Interpreter reads captures from there. See [Receiver configuration](../reference/receiver-configuration.md) for every key.

### One replica set for both

The compose file points `Cratis__MongoDB__Server` and `Prologue__Mongo__ConnectionString` at the MongoDB replica set inside the `chronicle` container, with `?directConnection=true`. The development image of Chronicle bundles that replica set, which makes the single container the whole of the setup. Chronicle and the Receiver have to use the same MongoDB server, so that what Chronicle projects is what the Receiver reads.

The development image is for trying the Workbench out, and its data lives inside that container. For a durable deployment:

1. Run Chronicle against your own MongoDB replica set.
2. Point `Cratis:MongoDB:Server` and `Cratis:MongoDB:Database` at that same replica set.
3. Point `Prologue:Mongo:ConnectionString` at it as well, so captures land in the same server.
4. Set `Cratis:Chronicle:ConnectionString` to your Chronicle kernel, with credentials that are not the development ones.

In the compose file these keys are the environment variables `Cratis__Chronicle__ConnectionString`, `Cratis__MongoDB__Server`, `Prologue__Mongo__ConnectionString` and so on.

## Point the Extractor at the Receiver

The Extractor posts captures to the Receiver when its output kind is `Api`. In `cratis-prologue.json`:

```json
{
    "prologue": {
        "prologueId": "00000000-0000-0000-0000-000000000000",
        "output": {
            "kind": "Api",
            "api": { "endpoint": "http://localhost:8080" }
        }
    }
}
```

- Replace `prologueId` with the id of the prologue you selected in the Workbench. With an id set, the Extractor posts to `POST /prologues/{prologueId}/captures` and the captures appear in that prologue's Capture step. Without one they go to `POST /captures` and are collected under an unassigned prologue.
- `api.endpoint` defaults to `http://localhost:5005`, which is not where the compose file serves the Receiver. The configuration the Workbench generates sets it for you; if you write the file yourself, set it to the Receiver's address.
- The Systems step shows the `cratis-prologue.json` it generates from your definition, with the id, the output and the correlation window filled in. With the **Api** output, `output.api.endpoint` is the address you opened the Workbench at (its scheme, host and port), so an Extractor started from the file posts back to this Receiver. If the browser reaches the Receiver at an address the Extractor cannot (for example `localhost` while the Extractor runs in a container), change the endpoint to one it can. Database connection strings in the generated file appear as `${secret:<name>}` placeholders, naming the secret rather than holding its value. The Extractor expands each placeholder when it starts: give it the connection string as the configuration value `Prologue:Secrets:<name>`, or as the environment variable `PROLOGUE_SECRET_<NAME>` (every character that is not a letter or digit becomes an underscore, in upper case), and it fails at startup naming any secret it cannot find. See [Secret placeholders](../reference/configuration.md#secret-placeholders).

See [Point Prologue at your system](point-prologue-at-your-system.md) for the capture sources themselves.

## First steps

1. Create a prologue in the Workbench and open **Systems**.
2. Add a system. Say where its code lives, a repository or a local folder, and which databases, HTTP clusters and telemetry services to capture. If the repository needs a token, connect the host first; see [Repository connections](../reference/repository-connections.md).
3. Set up AI under **Settings** if you want Discover and the Interpreter to use a language model; see [AI setup](../reference/ai-setup.md).
4. Run **Discover** on the code, start the Extractor for **Capture**, or both.

The [walkthrough](workbench-walkthrough.md) continues from there.

## Security and exposure

The Workbench is designed to run inside your network, next to the system it studies.

- **There is no sign-in.** The Workbench does not authenticate people. Anyone who can reach the Receiver can use every page and every endpoint, including the capture endpoints that accept posts. Put it behind your own access control and do not publish it to the internet. Bind the Receiver to an internal interface (for example publish the port as `127.0.0.1:8080:8080`, or put it on a private network), and set `AllowedHosts` (the ASP.NET Core host filter, `AllowedHosts` in configuration or `AllowedHosts` as an environment variable) to the host names people use to reach it, so requests for any other host name are refused.
- **Analyze folder reads any path the Receiver can read, unless you limit it.** Discover's **Analyze folder** form, and a system whose location is a local folder, read source files from the Receiver's filesystem: only files with source extensions (skipping generated, build and dependency folders), at most 3,000 files of up to 200,000 bytes each. Set `Prologue:Discover:AllowedRoots` to the folders that may be analyzed; a path outside them, or one that climbs out with `..`, is refused. Without it, whoever can use the Workbench can point it at any folder the Receiver process can read, so mount into the container only what you are willing to expose that way. See [Receiver configuration](../reference/receiver-configuration.md#discover).
- **The images run as a non-root user.** The Receiver and the Extractor images run as the `app` user (uid 1654), not root. A volume you mount for clones (`/var/prologue/repos`) or for capture files has to be writable by that user.
- **Credentials are referenced by secret name only.** A repository connection records the name of a secret, never its value. The Receiver looks the name up only in `Prologue:Secrets:<name>` or the environment variable `PROLOGUE_SECRET_<NAME>`, never in an arbitrary environment variable, so a name typed into the Workbench cannot read the Receiver's own connection strings or keys. **This is a breaking change:** a secret that was supplied as a bare environment variable (for example `GITHUB_TOKEN`) must be renamed with the `PROLOGUE_SECRET_` prefix, such as `PROLOGUE_SECRET_GITHUB_PROLOGUE_READ`, or set as `Prologue:Secrets:<name>`. See [How secrets are resolved](../reference/receiver-configuration.md#how-secrets-are-resolved).
- **A token only goes to its own host, over https.** The Receiver attaches a connection's token to a clone only when the repository address is https and on the host of that connection (github.com, gitlab.com or the server you gave). A repository given as a full address on another host is cloned without credentials or refused, and local paths and `file://` addresses are refused for hosted repositories. Git itself runs with https as its only transport and without the Receiver host's git configuration. The token travels in the git process environment and is never stored, logged or returned.
- **Shadow destinations receive request bodies.** With [shadowing](../reference/shadowing.md), the Extractor sends a copy of each mirrored command, body included, to the new system you name. Only an allowlist of headers is copied (content and trace headers, `Accept` headers and `User-Agent`) plus those a rule lists in `forwardHeaders`; credentials such as `Authorization` and cookies are not sent unless you list them. Shadow only to a system you control.
- **Captures hold metadata, not row values or bodies.** The Workbench records command shapes, table and column names, statuses, trace ids and span names. Query strings are removed from the paths it records. Treat the data as sensitive anyway, and protect the MongoDB it lives in.
- **Analyzed code goes to the AI providers you configure.** Discover sends the source it reads, and the Interpreter sends capture evidence, to the language models behind the Dramaturg, Prompter, Usher and Interpreter agents. What leaves your network depends on the providers you configure; see the [network posture](../reference/ai-setup.md#network-posture).
- **Share links are not an access boundary.** Discover can share the application map through a read-only link. The link opens a view that changes nothing, but anyone who has the link can open it, and it is not tied to a person. Stop sharing to invalidate it.

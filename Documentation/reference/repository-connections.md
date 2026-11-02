---
title: Repository connections
description: How the Workbench connects to GitHub, Azure DevOps and GitLab, and clones code read-only for Discover.
---

Discover reads each system's source code. The Systems step records where a system's code lives; a repository connection says how the Receiver may reach the host. A connection is per host: one for GitHub, one for Azure DevOps, one for GitLab. A local folder needs none.

## Hosts

| Host | Connect with | Notes |
|---|---|---|
| GitHub | A secret name and a server URL, or a GitHub App installation | The server URL also covers GitHub Enterprise; without a connection, `owner/name` resolves to github.com |
| Azure DevOps | A secret name and the organization URL | A repository is written `project/repository`; one part alone means a repository named like the project |
| GitLab | A secret name and a server URL | gitlab.com is the default |
| Local folder | Nothing | The Receiver reads the folder where it is. It has to exist on the Receiver's filesystem |

A connection needs the address of the server, starting with `https://`, and a secret name. The secret is the *name* of an access token; the Workbench refuses a value that looks like a token itself (a known token prefix, whitespace, or a long run of letters and digits).

The token itself is supplied to the Receiver as described in [How secrets are resolved](receiver-configuration.md#how-secrets-are-resolved).

### GitHub App installations

Instead of a personal access token, GitHub can be connected through a GitHub App installation. Record:

- the **app id** and the **installation id**;
- the **name of the secret** that holds the app's private key in PEM form (resolved like any other secret, see [How secrets are resolved](receiver-configuration.md#how-secrets-are-resolved)); a key whose line breaks were written as `\n` is accepted;
- optionally the **API address**, for GitHub Enterprise. It defaults to `https://api.github.com`. It has to be `https` and on the connection's host (`api.<host>` or `<host>/api/v3`).

Each time it clones, the Receiver signs a short JWT (RS256, issuer the app id, valid from 60 seconds ago for nine minutes) with the private key and exchanges it at `POST {api}/app/installations/{id}/access_tokens` for an installation token. The token is used for that clone only, through the same git environment as an access token, and only for repository addresses on the connection's host over `https`. It is kept in memory until five minutes before it expires, so clones in quick succession mint one token; it is never stored in an event or read model, logged or returned. The private key, the JWT and the token are redacted from every failure message.

A clone fails with a message that says which step failed: the secret is not configured, the secret does not hold a PEM RSA private key, GitHub rejected the sign-in (401, check the app id and key), refused the installation (403) or does not know it (404, check the installation id and API address), or could not be reached. A GitHub connection recorded earlier with only an installation id has no key to sign with and asks you to connect again.

## What a clone does

Clones are read-only and kept small:

- They are shallow, with a depth of one commit, and fetch only the system's branch.
- When systems live in sub paths of a repository, only those paths are checked out (sparse checkout in cone mode). A system with no sub path checks out the whole repository.
- Systems in the same repository and branch share one clone, with the sub paths of all of them checked out.
- **Pull again** fetches the branch again and checks out its latest commit. The commit is recorded with the clone.
- A sub path has to stay inside the repository, and a repository or branch name cannot start with a dash.
- Each `git` command is stopped after ten minutes.

Credentials reach `git` through its process environment only, as an authorization header for the one command. They do not appear in the clone URL, and a credential in a repository URL you type is removed. Failure messages are shortened and the token is redacted from them.

## Where clones go

Under `Prologue:Repositories:CloneDirectory`, which defaults to `/var/prologue/repos`. See [Receiver configuration](receiver-configuration.md#repositories).

## Local folders

For a local-folder system, the repository is the folder's path on the Receiver and the sub path is appended to it. If the folder is a Git working tree, the clone status shows its `HEAD`; otherwise it shows `working tree`. In the compose setup, mount the folder into the `receiver` container first. See [Security and exposure](../guides/running-the-workbench.md#security-and-exposure) for what this means for who can read what.

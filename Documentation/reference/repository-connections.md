---
title: Repository connections
description: How the Workbench connects to GitHub, Azure DevOps and GitLab, and clones code read-only for Discover.
---

Discover reads each system's source code. The Systems step records where a system's code lives; a repository connection says how the Receiver may reach the host. A connection is per host: one for GitHub, one for Azure DevOps, one for GitLab. A local folder needs none.

## Hosts

| Host | Connect with | Notes |
|---|---|---|
| GitHub | A secret name and a server URL, or an app installation | The server URL also covers GitHub Enterprise; without a connection, `owner/name` resolves to github.com |
| Azure DevOps | A secret name and the organization URL | A repository is written `project/repository`; one part alone means a repository named like the project |
| GitLab | A secret name and a server URL | gitlab.com is the default |
| Local folder | Nothing | The Receiver reads the folder where it is. It has to exist on the Receiver's filesystem |

A connection needs the address of the server, starting with `https://`, and a secret name. The secret is the *name* of an access token; the Workbench refuses a value that looks like a token itself (a known token prefix, whitespace, or a long run of letters and digits).

The token itself is supplied to the Receiver as described in [How secrets are resolved](receiver-configuration.md#how-secrets-are-resolved).

### GitHub App installations

You can record a GitHub connection as an app installation id instead of a token. **Cloning through an app installation is not implemented.** A clone for a system on such a connection fails with the message `Cloning through a GitHub App installation is not available yet; connect GitHub with an access token`. Connect GitHub with a token to clone.

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

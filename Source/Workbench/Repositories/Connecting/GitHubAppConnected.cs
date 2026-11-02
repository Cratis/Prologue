// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Repositories.Connecting;

/// <summary>
/// GitHub was connected through the installation of a GitHub App, without the app's id and private key. It is no longer recorded
/// (see <see cref="GitHubAppInstallationConnected"/>) but stays so the connections already in an event store replay.
/// </summary>
/// <param name="Url">The GitHub server.</param>
/// <param name="Installation">The installation.</param>
[EventType]
public record GitHubAppConnected(ServerUrl Url, InstallationId Installation);

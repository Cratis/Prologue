// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Repositories.Cloning;

/// <summary>
/// One call to git.
/// </summary>
/// <param name="Arguments">The arguments.</param>
/// <param name="InDirectory">Whether it runs inside the clone's directory.</param>
public record CloneStep(IReadOnlyList<string> Arguments, bool InDirectory);

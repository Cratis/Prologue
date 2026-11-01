// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Identities;

namespace Cratis.Prologue.Workbench.Identity;

/// <summary>
/// Resolves the identity Chronicle stamps onto appended events. The Workbench runs as a single local application, so
/// everything it appends is caused by the system until sign-in is introduced.
/// </summary>
public class WorkbenchIdentityProvider : BaseIdentityProvider;

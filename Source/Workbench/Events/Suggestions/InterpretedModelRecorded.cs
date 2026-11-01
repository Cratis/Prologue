// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// An interpretation found a different structure than the one before.
/// </summary>
/// <param name="Model">The structure found.</param>
[EventType]
public record InterpretedModelRecorded(InterpretedModel Model);

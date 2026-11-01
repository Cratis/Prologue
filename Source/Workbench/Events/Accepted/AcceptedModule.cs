// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Discover;

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// A module of the accepted model.
/// </summary>
/// <param name="Name">The module name.</param>
/// <param name="Features">The features.</param>
public record AcceptedModule(ModuleName Name, IEnumerable<AcceptedFeature> Features);

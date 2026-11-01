// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// A feature of the accepted model.
/// </summary>
/// <param name="Name">The feature name.</param>
/// <param name="Slices">The slices, producers first.</param>
public record AcceptedFeature(FeatureName Name, IEnumerable<AcceptedSlice> Slices);

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Explore.Observations;

/// <summary>
/// Something worth noticing about a command shape, worded for the person exploring it.
/// </summary>
/// <param name="Kind">What kind of observation it is.</param>
/// <param name="Text">The observation.</param>
public record ShapeObservation(ObservationKind Kind, string Text);

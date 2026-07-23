// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents one structured refinement returned by the language model — the system name it derived, the renames
/// it wants applied, the descriptions for the (renamed) modules, features, slices, and commands, and the questions
/// it needs answered before it can decide. The deterministic heuristics own the structure; this is everything the
/// language model is allowed to contribute.
/// </summary>
/// <param name="SystemName">The name the model derived for the captured system as a whole; empty when it offered none.</param>
/// <param name="Renames">The map from provisional name to refined name.</param>
/// <param name="Descriptions">The descriptions keyed by <c>module:&lt;module&gt;</c>, <c>feature:&lt;module&gt;/&lt;feature&gt;</c>, <c>slice:&lt;module&gt;/&lt;feature&gt;/&lt;slice&gt;</c>, and <c>command:&lt;module&gt;/&lt;feature&gt;/&lt;slice&gt;/&lt;command&gt;</c> — the keys refer to the renamed names.</param>
/// <param name="Questions">The questions the model asked; empty in the normal case.</param>
public record ModelRefinement(
    string SystemName,
    IReadOnlyDictionary<string, string> Renames,
    IReadOnlyDictionary<string, string> Descriptions,
    IReadOnlyList<RefinementQuestion> Questions);

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// Tells whether the language model could be reached while an interpretation ran. The interpretation never fails because it could not:
/// the heuristic model is the floor. The Workbench still has to say that nothing was refined, and why.
/// </summary>
public interface ILanguageModelWatch
{
    /// <summary>
    /// Gets why the language model could not be reached; empty when it was, or was not needed yet.
    /// </summary>
    string Failure { get; }
}

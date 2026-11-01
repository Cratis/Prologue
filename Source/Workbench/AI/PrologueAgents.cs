// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.LanguageModels;

namespace Cratis.Prologue.Workbench.AI;

/// <summary>
/// The catalog of agents the Workbench works with. Every AI call goes through Cratis.AI under one of these purposes.
/// </summary>
public static class PrologueAgents
{
    /// <summary>
    /// Maps a system's code into the pieces the Workbench can reason about.
    /// </summary>
    public static readonly PrologueAgent Dramaturg = new(new LanguageModelPurpose("prologue-dramaturg"), "Dramaturg", "Maps code into commands, handlers and persisted state.");

    /// <summary>
    /// Finds the decision points in a system that capture scenarios must exercise.
    /// </summary>
    public static readonly PrologueAgent Prompter = new(new LanguageModelPurpose("prologue-prompter"), "Prompter", "Finds the decision points a capture scenario must exercise.");

    /// <summary>
    /// Traces dependencies between services, tables and endpoints.
    /// </summary>
    public static readonly PrologueAgent Usher = new(new LanguageModelPurpose("prologue-usher"), "Usher", "Traces the dependencies between the parts of a system.");

    /// <summary>
    /// Interprets captured evidence into events and asks questions when it cannot decide.
    /// </summary>
    public static readonly PrologueAgent Interpreter = new(new LanguageModelPurpose("prologue-interpreter"), "Interpreter", "Interprets captured evidence into events and asks when unsure.");

    /// <summary>
    /// Gets every agent.
    /// </summary>
    public static readonly IReadOnlyList<PrologueAgent> All = [Dramaturg, Prompter, Usher, Interpreter];
}

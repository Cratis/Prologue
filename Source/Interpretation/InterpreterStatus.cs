// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents the stage an interpreter session is at — reported to hosts on every transition so a CLI can show
/// progress and Studio can reflect the session's state while it is parked awaiting answers.
/// </summary>
public enum InterpreterStatus
{
    /// <summary>
    /// The session has been created but has not started working yet.
    /// </summary>
    NotStarted,

    /// <summary>
    /// The session is reading the captures it will interpret.
    /// </summary>
    ReadingCaptures,

    /// <summary>
    /// The session is analyzing the evidence within the captures.
    /// </summary>
    AnalyzingEvidence,

    /// <summary>
    /// The session is building the deterministic heuristic event model from the evidence.
    /// </summary>
    BuildingModel,

    /// <summary>
    /// The session is refining the heuristic model with the language model.
    /// </summary>
    Refining,

    /// <summary>
    /// The language model asked questions and the session is parked until every pending question is answered.
    /// </summary>
    AwaitingAnswers,

    /// <summary>
    /// The hosting process is generating the Screenplay output from the completed model. The session itself never
    /// enters this stage — it transitions from <see cref="Refining"/> straight to <see cref="Completed"/>; hosts
    /// that generate output afterwards report this status while they emit it.
    /// </summary>
    GeneratingScreenplay,

    /// <summary>
    /// The session has completed and the extracted model is final.
    /// </summary>
    Completed,

    /// <summary>
    /// The session failed — the state's error describes why.
    /// </summary>
    Failed
}

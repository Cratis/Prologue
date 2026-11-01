// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// A place in the code of the observed system where a command is accepted or refused, found by Discover. Captures are matched
/// to it by command shape and outcome.
/// </summary>
/// <param name="Shape">The shape of the command, in the same format as <see cref="CommandShape"/>.</param>
/// <param name="Outcome">Whether the rule accepts or refuses the command.</param>
/// <param name="Rule">The rule in words, for example a loan is refused when no copy is available.</param>
/// <param name="File">The file the rule was found in.</param>
/// <param name="Line">The line in the file.</param>
/// <param name="Capability">The capability on the application map the rule belongs to.</param>
public record DecisionPoint(CommandShape Shape, DecisionOutcome Outcome, RuleText Rule, SourceFilePath File, LineNumber Line, CapabilityName Capability);

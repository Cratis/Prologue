// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Events.Accepted;

/// <summary>
/// A rule or constraint of the accepted model, in words.
/// </summary>
/// <param name="Kind">Where the rule comes from.</param>
/// <param name="Text">The rule.</param>
/// <param name="Shape">The command shape it is about; none when it is about an event only.</param>
public record AcceptedRule(AcceptedRuleKind Kind, string Text, CommandShape Shape);

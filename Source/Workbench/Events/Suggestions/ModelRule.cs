// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A validation rule the interpreter inferred for a command property.
/// </summary>
/// <param name="Property">The property the rule is about.</param>
/// <param name="Kind">The kind of rule.</param>
/// <param name="Argument">The rule's argument, for example the maximum length; empty when it has none.</param>
/// <param name="Message">What to tell the user when the rule is broken.</param>
public record ModelRule(string Property, ExtractedValidationRuleKind Kind, string Argument, string Message);

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpreter.Contracts;

/// <summary>
/// Represents a validation rule inferred for a command property — derived from the constraints the observed
/// database schema places on the column the property maps to.
/// </summary>
/// <param name="Property">The name of the command property the rule applies to.</param>
/// <param name="Kind">The kind of validation the rule expresses.</param>
/// <param name="Argument">The argument for the rule (for example the length for <see cref="ExtractedValidationRuleKind.MaxLength"/>); empty when the kind takes none.</param>
/// <param name="Message">The message shown when the rule is violated.</param>
public record ExtractedValidationRule(
    string Property,
    ExtractedValidationRuleKind Kind,
    string Argument,
    string Message);

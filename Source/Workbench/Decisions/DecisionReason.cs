// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Decisions;

/// <summary>
/// Why the observed system decided what it did.
/// </summary>
/// <param name="Source">Whether the reason was found in the code or observed.</param>
/// <param name="Text">The reason in words.</param>
/// <param name="File">The file of the rule; empty for an observed reason.</param>
/// <param name="Line">The line of the rule; zero for an observed reason.</param>
/// <param name="Capability">The capability the decision belongs to; empty until Discover has mapped one.</param>
public record DecisionReason(ReasonSource Source, RuleText Text, SourceFilePath File, LineNumber Line, CapabilityName Capability);

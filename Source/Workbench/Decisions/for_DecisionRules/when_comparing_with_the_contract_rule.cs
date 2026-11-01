// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Decisions.for_DecisionRules;

/// <summary>
/// The Extractor classifies the answers of the legacy and the shadow system with the contract rule; the Workbench classifies the
/// captured decisions with <see cref="DecisionRules"/>. They must agree for every status, or a parity "match" would mean two things.
/// </summary>
public class when_comparing_with_the_contract_rule : Specification
{
    List<int> _disagreements;

    void Because() =>
        _disagreements = [.. Enumerable.Range(-1, 702).Where(status => (int)DecisionRules.Classify(new HttpStatus(status)) != (int)HttpOutcomes.Classify(status))];

    [Fact] void should_agree_for_every_status() => _disagreements.ShouldBeEmpty();
}
#endif

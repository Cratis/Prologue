// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Scenarios.given;

namespace Cratis.Prologue.Workbench.Scenarios.Marking.for_MarkScenarioStarted;

public class when_no_scenario_is_running : a_script
{
    Result<ScenarioStarted, ValidationResult> _picked;
    Result<ScenarioStarted, ValidationResult> _typed;
    Result<ScenarioStarted, ValidationResult> _blank;
    Result<ScenarioStarted, ValidationResult> _first;
    ScenarioScript _afterTyped;

    void Because()
    {
        _picked = new MarkScenarioStarted(_prologue, "Lend a copy").Handle(_script);
        _typed = new MarkScenarioStarted(_prologue, "  Report copies lost ").Handle(_script);
        _blank = new MarkScenarioStarted(_prologue, "   ").Handle(_script);
        _first = new MarkScenarioStarted(_prologue, "Lend a copy").Handle(null);
        _afterTyped = ScenarioScript.Reduce(_typed.Appended()!, _script, _context);
    }

    [Fact] void should_start_a_scenario_from_the_script() => _picked.Appended()!.Name.Value.ShouldEqual("Lend a copy");
    [Fact] void should_start_a_typed_scenario_without_the_surrounding_spaces() => _typed.Appended()!.Name.Value.ShouldEqual("Report copies lost");
    [Fact] void should_refuse_a_blank_name() => _blank.Refusal().ShouldNotBeNull();
    [Fact] void should_start_when_the_prologue_has_no_script_yet() => _first.Appended().ShouldNotBeNull();
    [Fact] void should_make_the_typed_scenario_the_active_one() => _afterTyped.Active.Value.ShouldEqual("Report copies lost");
    [Fact] void should_add_the_typed_scenario_to_the_script() => _afterTyped.Scenarios.Select(name => name.Value).ShouldContain("Report copies lost");
}
#endif

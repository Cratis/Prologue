// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Scenarios.given;

namespace Cratis.Prologue.Workbench.Scenarios.Scripting.for_AddScenario;

public class when_adding : a_script
{
    Result<ScenarioAdded, ValidationResult> _new;
    Result<ScenarioAdded, ValidationResult> _repeated;
    Result<ScenarioAdded, ValidationResult> _repeatedInOtherCase;
    Result<ScenarioAdded, ValidationResult> _blank;

    void Because()
    {
        _new = new AddScenario(_prologue, " Report copies lost ").Handle(_script);
        _repeated = new AddScenario(_prologue, "Lend a copy").Handle(_script);
        _repeatedInOtherCase = new AddScenario(_prologue, "lend a COPY").Handle(_script);
        _blank = new AddScenario(_prologue, "").Handle(_script);
    }

    [Fact] void should_add_a_new_scenario_trimmed() => _new.Appended()!.Name.Value.ShouldEqual("Report copies lost");
    [Fact] void should_refuse_a_scenario_already_in_the_script() => _repeated.Refusal().ShouldNotBeNull();
    [Fact] void should_compare_names_without_regard_to_case() => _repeatedInOtherCase.Refusal().ShouldNotBeNull();
    [Fact] void should_refuse_a_blank_name() => _blank.Refusal().ShouldNotBeNull();
}
#endif

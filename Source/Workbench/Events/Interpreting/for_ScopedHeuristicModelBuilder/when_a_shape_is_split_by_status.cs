// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Explore.Scope;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_ScopedHeuristicModelBuilder;


public class when_a_shape_is_split_by_status : a_library_interpretation
{
    ExtractionResult _unsplit;
    ExtractionResult _split;

    void Because()
    {
        var included = ScopedCaptures.Included(_captures, _scope);
        _unsplit = new ScopedHeuristicModelBuilder(_heuristics, _scope).Build(_prologue.Value, included);
        var splitScope = Explore.Scope.InterpretationScope.Reduce(new Explore.Scope.Splitting.ShapeSplitByStatus("POST /api/loans/{id}"), _scope, _context);
        _split = new ScopedHeuristicModelBuilder(_heuristics, splitScope).Build(_prologue.Value, included);
    }

    static IEnumerable<string> Slices(ExtractionResult result) => result.Modules.SelectMany(module => module.Features).SelectMany(feature => feature.Slices).Select(slice => slice.Name);

    [Fact] void should_not_mention_the_status_when_not_split() => Slices(_unsplit).Any(name => name.Contains("Status")).ShouldBeFalse();
    [Fact] void should_make_a_slice_per_status() => Slices(_split).Where(name => name.Contains("Status")).ShouldContainOnly("CreateStatus201", "CreateStatus409");
    [Fact] void should_leave_the_other_shapes_alone() => Slices(_split).Where(name => !name.Contains("Status")).ShouldContain("Register");
    [Fact] void should_leave_the_excluded_shape_out_of_the_model() => Slices(_unsplit).Any(name => name.Contains("Simulation")).ShouldBeFalse();
}

#endif

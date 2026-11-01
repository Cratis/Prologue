// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;
using Cratis.Prologue.Workbench.Explore.Scope;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_ScopedCaptures;


public class when_scoping_the_captures : a_library_interpretation
{
    IReadOnlyList<Capture> _included;
    IReadOnlyList<CapturePartition> _everything;
    IReadOnlyList<CapturePartition> _splitLoans;

    void Because()
    {
        _included = ScopedCaptures.Included(_captures, _scope);
        _everything = ScopedCaptures.Partition(_captures, null);
        _splitLoans = ScopedCaptures.Partition(_captures, Explore.Scope.InterpretationScope.Reduce(new Explore.Scope.Splitting.ShapeSplitByStatus("POST /api/loans/{id}"), _scope, _context));
    }

    static bool IsSimulation(Capture capture) => ScopedCaptures.ShapeOf(capture).Value == "POST /api/simulation/start";

    [Fact] void should_leave_the_excluded_shape_out() => _included.Any(IsSimulation).ShouldBeFalse();
    [Fact] void should_keep_the_other_shapes() => _included.Count.ShouldEqual(_captures.Count - 1);
    [Fact] void should_keep_the_captured_schema() => _included.Any(capture => capture.Entries.Any(entry => entry.Payload is DatabaseSchemaObserved)).ShouldBeTrue();
    [Fact] void should_keep_everything_in_one_part_when_nothing_is_split() => _everything.Count.ShouldEqual(1);
    [Fact] void should_have_a_part_per_status_of_the_split_shape() => _splitLoans.Select(part => part.SliceSuffix).ShouldContainOnly(string.Empty, "Status201", "Status409");
    [Fact] void should_take_the_split_shape_out_of_the_part_with_the_rest() => _splitLoans[0].Captures.Any(capture => ScopedCaptures.ShapeOf(capture).Value == "POST /api/loans/{id}").ShouldBeFalse();
    [Fact] void should_give_every_part_the_schema() => _splitLoans.All(part => part.Captures.Any(capture => capture.Entries.Any(entry => entry.Payload is DatabaseSchemaObserved))).ShouldBeTrue();
    [Fact] void should_put_only_the_captures_of_the_status_in_its_part() => _splitLoans.Single(part => part.SliceSuffix == "Status409").Captures.Count(capture => !ScopedCaptures.ShapeOf(capture).IsNone).ShouldEqual(2);
}

#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Explore.given;
using Cratis.Prologue.Workbench.Explore.Observations;

namespace Cratis.Prologue.Workbench.Explore.Matrix.for_CommandMatrix;

public class when_status_mix_decides_what_is_written : Specification
{
    ShapeProfile _stock;

    void Because()
    {
        var insert = Captured.Transaction((ChangeOperation.Insert, "Inventory", ["TotalCount"]));
        var update = Captured.Transaction((ChangeOperation.Update, "Inventory", ["TotalCount"]));
        _stock = Captured.Matrix(
            Captured.Command("PUT", "/api/inventory/978-0-1234/stock", 201, "", "", insert),
            Captured.Command("PUT", "/api/inventory/978-0-5678/stock", 200, "", "", update),
            Captured.Command("PUT", "/api/inventory/978-0-5678/stock", 200, "", "", update)).Shapes.Single();
    }

    [Fact] void should_note_that_the_route_inserts_first_and_updates_afterwards() =>
        _stock.Observations.Single(observation => observation.Kind == ObservationKind.InsertsThenUpdates).Text.ShouldContain("inserts into Inventory on first use (201) and updates it afterwards (200)");
    [Fact] void should_not_also_call_the_table_partial() =>
        _stock.Observations.Any(observation => observation.Kind == ObservationKind.WritesInSomeRuns).ShouldBeFalse();
}
#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Scope;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Explore.given;

/// <summary>
/// A library whose captures show lending, a refused lend, a route that inserts and then updates, a load generator, and a nightly
/// job writing Inventory with no command.
/// </summary>
public class a_lending_library : Specification
{
    protected CommandMatrix _matrix;
    protected UncorrelatedWrites _uncorrelated;
    protected InterpretationScope _scope;

    void Establish()
    {
        var context = EventContext.EmptyWithEventSourceId(PrologueId.New());
        var lend = Captured.Transaction((ChangeOperation.Insert, "Loans", ["BookId", "MemberId"]), (ChangeOperation.Update, "Inventory", ["AvailableCount"]));
        var insert = Captured.Transaction((ChangeOperation.Insert, "Inventory", ["TotalCount"]));
        var update = Captured.Transaction((ChangeOperation.Update, "Inventory", ["TotalCount"]));
        var night = new DateTimeOffset(2026, 11, 1, 2, 0, 0, TimeSpan.Zero);

        var events = new List<CaptureRecorded>();
        for (var index = 0; index < 4; index++)
        {
            events.Add(Captured.Command("POST", "/api/loans", 201, "LoansController.Lend", "Lend a copy", lend));
        }

        events.Add(Captured.Command("POST", "/api/loans", 409, "LoansController.Lend", "Lend a copy"));
        events.Add(Captured.Command("PUT", "/api/inventory/978-0-1234/stock", 201, "InventoryController.Stock", "", insert));
        events.Add(Captured.Command("PUT", "/api/inventory/978-0-1234/stock", 200, "InventoryController.Stock", "", update));
        events.Add(Captured.Command("POST", "/api/simulation/start", 200, "Simulation.Start"));
        events.Add(RecordedCaptures.WithoutCommand(RecordedCaptures.Write(ChangeOperation.Update, "Inventory", "TotalCount")) with { Occurred = night });

        foreach (var @event in events)
        {
            _matrix = CommandMatrix.Reduce(@event, _matrix, context);
            _uncorrelated = UncorrelatedWrites.Reduce(@event, _uncorrelated, context);
        }

        _scope = InterpretationScope.Reduce(new Scope.Excluding.ShapeExcludedFromInterpretation("POST /api/simulation/start"), null, context);
    }
}
#endif

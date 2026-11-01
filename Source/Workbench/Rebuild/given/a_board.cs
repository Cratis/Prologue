// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Rebuild.Differences;
using Cratis.Prologue.Workbench.Rebuild.Parity;
using Cratis.Prologue.Workbench.Rebuild.Slicing;
using Cratis.Prologue.Workbench.Rebuild.Staging;

namespace Cratis.Prologue.Workbench.Rebuild.given;

/// <summary>
/// A prologue rebuilding LendBook (POST /api/loans) and ReturnBook (POST /api/loans/{id}/return), built by reducing the events that made it.
/// </summary>
public class a_board : Specification
{
    protected PrologueId _prologue;
    protected EventContext _context;
    protected RebuildBoard _board;
    protected RebuildDifferences? _differences;

    protected RebuildSlice Lend => RebuildRules.Find(_board, "LendBook")!;

    protected RebuildSlice Return => RebuildRules.Find(_board, "ReturnBook")!;

    protected static ParityObserved Observation(string shape, ParityOutcome outcome, int legacy = 201, int shadow = 201, string path = "/api/loans") =>
        new(Guid.NewGuid(), DateTimeOffset.UtcNow, shape.Split(' ')[0], path, shape, TraceId.None, legacy, outcome == ParityOutcome.Matched ? legacy : shadow, outcome);

    protected void Apply(ParityObserved observed)
    {
        _board = RebuildBoard.Reduce(observed, _board, _context);
        _differences = RebuildDifferences.Reduce(observed, _differences, _context);
    }

    protected void MoveTo(string name, RebuildStage stage)
    {
        if (stage >= RebuildStage.Modeled)
        {
            _board = RebuildBoard.Reduce(new RebuildSliceMarkedModeled(name), _board, _context);
        }

        if (stage >= RebuildStage.Built)
        {
            _board = RebuildBoard.Reduce(new RebuildSliceMarkedBuilt(name), _board, _context);
        }

        if (stage >= RebuildStage.Shadowing)
        {
            _board = RebuildBoard.Reduce(new ShadowingStarted(name), _board, _context);
        }

        if (stage >= RebuildStage.CutOver)
        {
            _board = RebuildBoard.Reduce(new SliceCutOver(name), _board, _context);
        }
    }

    void Establish()
    {
        _prologue = PrologueId.New();
        _context = EventContext.EmptyWithEventSourceId(_prologue);
        var lend = RebuildBoard.Reduce(new RebuildSliceAdded("LendBook", ["POST /api/loans"]), null, _context);
        _board = RebuildBoard.Reduce(new RebuildSliceAdded("ReturnBook", ["POST /api/loans/{id}/return"]), lend, _context);
    }
}
#endif

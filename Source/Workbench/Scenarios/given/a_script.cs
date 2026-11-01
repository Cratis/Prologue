// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues;
using Cratis.Prologue.Workbench.Scenarios.Scripting;

namespace Cratis.Prologue.Workbench.Scenarios.given;

/// <summary>
/// A prologue whose script holds Lend a copy and Return a copy, built by reducing the events that made it.
/// </summary>
public class a_script : Specification
{
    protected PrologueId _prologue;
    protected EventContext _context;
    protected ScenarioScript _script;

    void Establish()
    {
        _prologue = PrologueId.New();
        _context = EventContext.EmptyWithEventSourceId(_prologue);
        var lend = ScenarioScript.Reduce(new ScenarioAdded("Lend a copy"), null, _context);
        _script = ScenarioScript.Reduce(new ScenarioAdded("Return a copy"), lend, _context);
    }
}
#endif

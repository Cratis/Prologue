// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Prologues.Defining;
using Cratis.Prologue.Workbench.Systems;
using Cratis.Prologue.Workbench.Systems.Adding;

namespace Cratis.Prologue.Workbench.Prologues.given;

/// <summary>
/// A prologue called Library with two systems, built by reducing the events that created it.
/// </summary>
public class a_prologue : Specification
{
    protected PrologueId _id;
    protected SystemId _lending;
    protected SystemId _search;
    protected PrologueDefinition _prologue;

    void Establish()
    {
        _id = PrologueId.New();
        _lending = SystemId.New();
        _search = SystemId.New();

        var defined = PrologueDefinition.Reduce(new PrologueDefined(new PrologueName("Library")), null, EventContext.EmptyWithEventSourceId(_id));
        var withLending = PrologueDefinition.Reduce(new SystemAdded(_lending, new ServiceName("Lending")), defined);
        _prologue = PrologueDefinition.Reduce(new SystemAdded(_search, new ServiceName("Search")), withLending)!;
    }
}
#endif

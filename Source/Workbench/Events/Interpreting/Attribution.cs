// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Events.Interpreting;

/// <summary>
/// What the captures show about one event: which command shapes led to it, how often, and the table changes behind it.
/// </summary>
public class Attribution
{
    readonly Dictionary<CommandShape, int> _shapes = [];
    readonly Dictionary<(string Schema, string Table, string Operation), HashSet<string>> _tables = [];

    /// <summary>
    /// Gets the captures per command shape.
    /// </summary>
    public IReadOnlyDictionary<CommandShape, int> Shapes => _shapes;

    /// <summary>
    /// Gets the table changes with the columns they wrote.
    /// </summary>
    public IReadOnlyDictionary<(string Schema, string Table, string Operation), HashSet<string>> Tables => _tables;

    /// <summary>
    /// Counts a capture of a command shape that led to the event.
    /// </summary>
    /// <param name="shape">The shape.</param>
    public void Count(CommandShape shape) => _shapes[shape] = _shapes.GetValueOrDefault(shape) + 1;

    /// <summary>
    /// Adds a table change behind the event.
    /// </summary>
    /// <param name="change">The change.</param>
    public void Add(TableChange change)
    {
        var key = (change.Schema, change.Table, change.Operation.ToString());
        if (!_tables.TryGetValue(key, out var columns))
        {
            _tables[key] = columns = [];
        }

        columns.UnionWith(change.Columns);
    }
}

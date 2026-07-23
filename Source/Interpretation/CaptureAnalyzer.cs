// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Turns a single correlated <see cref="Capture"/> into the provisional slices it evidences. An HTTP command that
/// is followed by a database transaction reads as a state-change slice (command + events) plus a state-view slice
/// (read model + projection) for the entities it wrote; a database transaction with no command reads as a state
/// view; a lone telemetry span reads as an automation.
/// </summary>
public static class CaptureAnalyzer
{
    /// <summary>
    /// Derives the provisional slices evidenced by a capture.
    /// </summary>
    /// <param name="capture">The correlated capture to analyze.</param>
    /// <param name="schema">The observed schema to source types and constraints from.</param>
    /// <returns>The provisional slices the capture evidences.</returns>
    public static IEnumerable<SliceDraft> Analyze(Capture capture, SchemaIndex schema)
    {
        var payloads = capture.Entries.Select(entry => entry.Payload).ToList();
        var http = payloads.OfType<HttpCommandObserved>().FirstOrDefault();
        var transactions = payloads.OfType<DatabaseTransactionObserved>().ToList();
        var spans = payloads.OfType<TelemetryObserved>().ToList();

        var events = BuildEvents(transactions, schema);

        if (http is not null)
        {
            foreach (var draft in FromCommand(http, spans, events, transactions, schema))
            {
                yield return draft;
            }
        }
        else if (transactions.Count > 0)
        {
            foreach (var draft in FromTransactionsOnly(transactions, events, schema))
            {
                yield return draft;
            }
        }
        else if (spans.Count > 0)
        {
            yield return FromSpanOnly(spans[0]);
        }
    }

    static IEnumerable<SliceDraft> FromCommand(
        HttpCommandObserved http,
        List<TelemetryObserved> spans,
        IReadOnlyList<ExtractedEvent> events,
        List<DatabaseTransactionObserved> transactions,
        SchemaIndex schema)
    {
        var route = ServerRoute(spans) ?? http.Path;
        var (module, feature, resource, action) = Location.FromPath(route);

        var commandName = OperationName(spans)
            ?? (action.Length > 0 ? $"{action}{resource}" : Naming.CommandName(http.Method, resource));
        var properties = PropertyInference.ForCommand(transactions, spans, schema);
        var command = new ExtractedCommand(commandName, properties, ValidationInference.ForCommand(transactions, schema));
        var sliceName = action.Length > 0 ? action : SliceName(http.Method);

        yield return new SliceDraft(
            module,
            feature,
            sliceName,
            ExtractedSliceType.StateChange,
            [command],
            events,
            [],
            [],
            ConstraintInference.ForSlice(transactions, schema));

        foreach (var view in ViewsFor(module, feature, transactions, events, schema))
        {
            yield return view;
        }
    }

    // The templated route (for example /api/authors/{id}) recorded on the correlated server span, preferred over
    // the concrete request path so ids never bleed into names. Null when the capture did not record it.
    static string? ServerRoute(IReadOnlyList<TelemetryObserved> spans) =>
        spans
            .Where(span => span.Kind.Equals("Server", StringComparison.OrdinalIgnoreCase))
            .Select(span => span.Attributes.TryGetValue("http.route", out var route) ? route : null)
            .FirstOrDefault(route => !string.IsNullOrWhiteSpace(route));

    // A server span name is usable as a command name only when it is a clean operation name (for example a
    // custom-instrumented "RegisterAuthor"), not the default "METHOD /route" ASP.NET span name — which would bleed
    // the method and path into the name. Null falls the caller back to method/action + resource naming.
    static string? OperationName(IReadOnlyList<TelemetryObserved> spans)
    {
        var name = spans
            .Where(span => span.Kind.Equals("Server", StringComparison.OrdinalIgnoreCase))
            .Select(span => span.Name)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains(' ')
            ? null
            : Naming.Pascalize(name);
    }

    static IEnumerable<SliceDraft> FromTransactionsOnly(
        List<DatabaseTransactionObserved> transactions,
        IReadOnlyList<ExtractedEvent> events,
        SchemaIndex schema)
    {
        var module = Naming.Pascalize(transactions[0].Database);
        return ViewsFor(module, module, transactions, events, schema);
    }

    static SliceDraft FromSpanOnly(TelemetryObserved span)
    {
        var module = Naming.Pascalize(span.ServiceName);
        var name = Naming.Pascalize(span.Name);
        return new SliceDraft(module, module, name, ExtractedSliceType.Automation, [], [], [], [], []);
    }

    static IEnumerable<SliceDraft> ViewsFor(
        string module,
        string feature,
        List<DatabaseTransactionObserved> transactions,
        IReadOnlyList<ExtractedEvent> events,
        SchemaIndex schema)
    {
        foreach (var entity in EntitiesIn(transactions))
        {
            var properties = PropertyInference.ForEntity(transactions, entity, schema);
            var readModel = new ExtractedReadModel(entity, properties);
            var projection = new ExtractedProjection($"{entity}Projection", [.. events.Select(@event => @event.Name)]);
            yield return new SliceDraft(module, feature, $"All{entity}s", ExtractedSliceType.StateView, [], [], [readModel], [projection], []);
        }
    }

    static IReadOnlyList<ExtractedEvent> BuildEvents(IReadOnlyList<DatabaseTransactionObserved> transactions, SchemaIndex schema) =>
    [
        .. transactions
            .SelectMany(transaction => transaction.Tables.Select(table => (transaction.Database, Table: table)))
            .Select(scoped =>
            {
                var schemaTable = schema.Find(scoped.Database, scoped.Table.Schema, scoped.Table.Table);
                var entity = Naming.Singularize(Naming.Pascalize(scoped.Table.Table));
                return new ExtractedEvent(
                    Naming.EventName(entity, scoped.Table.Operation),
                    [.. scoped.Table.Columns.Select(column => PropertyInference.Property(column, schemaTable?.Column(column)))]);
            })
    ];

    static IEnumerable<string> EntitiesIn(IReadOnlyList<DatabaseTransactionObserved> transactions) =>
        transactions
            .SelectMany(transaction => transaction.Tables)
            .Select(table => Naming.Singularize(Naming.Pascalize(table.Table)))
            .Distinct();

    static string SliceName(string method) =>
        method.ToUpperInvariant() switch
        {
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Update",
            "DELETE" => "Delete",
            _ => "Handle"
        };
}

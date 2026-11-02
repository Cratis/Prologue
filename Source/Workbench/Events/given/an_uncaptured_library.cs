// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Discover;
using Cratis.Prologue.Workbench.Discover.Findings;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Uncaptured;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Uncorrelated;

namespace Cratis.Prologue.Workbench.Events.given;

/// <summary>
/// The lending library of the other specs, with what only the code knows (a reservation route, a renewal route, an overdue sweep and a nightly
/// inventory job) and writes that arrived with no command (inventory, which the job explains, and reminders, which nothing does).
/// </summary>
public class an_uncaptured_library : a_library_interpretation
{
    protected static readonly MappedCapability _reserve = Capability("Reservations", "Reserve a book", "POST /api/reservations");
    protected static readonly MappedCapability _renew = Capability("Lending", "Renew a loan", "POST /api/loans/{id}/renew");
    protected static readonly MappedCapability _lend = Capability("Lending", "Lend a copy", "POST /api/loans/{id}");
    protected static readonly MappedCapability _list = Capability("Lending", "List loans", "GET /api/loans");
    protected static readonly MappedCapability _sweep = Capability("Lending", "Overdue sweep");
    protected static readonly CodeJob _job = new("Nightly inventory reconciliation", string.Empty, new SourcePath("Jobs/InventoryJob.cs"), new Discover.LineNumber(14), new ModuleName("Stock"));

    protected EvidenceFacts _uncaptured;

    protected static MappedCapability Capability(string module, string capability, params string[] shapes) =>
        new(new AnalysisSubjectId(Guid.Empty), module, capability, [.. shapes.Select(shape => new RouteShape(shape))], $"{module}/{capability.Replace(' ', '-')}.cs");

    protected static UncorrelatedTable Written(string table, int transactions, int hour, ChangeOperation operation = ChangeOperation.Update, string schema = "dbo") =>
        new(new TableWrite(schema, table, [new OperationRuns(operation, transactions)], [new Captures.ColumnName("Quantity")]), transactions, [new HourCount(hour, transactions)]);

    protected virtual IEnumerable<MappedCapability> Capabilities() => [_lend, _list, _reserve, _renew, _sweep];

    protected virtual IEnumerable<CodeJob> Jobs() => [_job];

    protected virtual IEnumerable<UncorrelatedTable> Uncorrelated() => [Written("Inventory", 5, 2), Written("Reminders", 4, 3), Written("Holds", 1, 9)];

    void Establish() =>
        _uncaptured = _facts with { Capabilities = [.. Capabilities()], Jobs = [.. Jobs()], Uncorrelated = [.. Uncorrelated()] };
}
#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpretation;
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Decisions;
using Cratis.Prologue.Workbench.Events.Interpreting;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Explore.given;
using Cratis.Prologue.Workbench.Explore.Matrix;
using Cratis.Prologue.Workbench.Explore.Scope;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Events.given;

/// <summary>
/// A lending library that was captured: authors registered, loans lent and refused, an author removed and refused, and load-generator traffic.
/// The captures are interpreted with the real heuristics; nothing is refined, so every refined name is the mechanical one.
/// </summary>
public class a_library_interpretation : Specification
{
    protected static readonly DateTimeOffset _occurred = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    protected PrologueId _prologue;
    protected EventContext _context;
    protected List<Capture> _captures;
    protected IBuildHeuristicModel _heuristics;
    protected EvidenceFacts _facts;
    protected InterpretationScope _scope;
    protected IReadOnlyList<CapturePartition> _partitions;
    protected ExtractionResult _heuristic;
    protected IReadOnlyList<SuggestedEvent> _suggestions;
    protected InterpretedModel _model;
    protected EventReview _review;

    protected static SuggestionKey Key(string module, string mechanical) => $"{module}/{mechanical}";

    protected static Capture Command(string method, string path, int status, int second, params TableChange[] changes)
    {
        var at = _occurred.AddSeconds(second);
        var entries = new List<Observation> { new(SourceKind.Http, at, new HttpCommandObserved(method, path, status)) };
        if (changes.Length > 0)
        {
            entries.Add(new Observation(SourceKind.SqlServer, at.AddMilliseconds(20), new DatabaseTransactionObserved("sqlserver", "LibraryDb", $"tx{second}", changes)));
        }

        return new Capture(Guid.NewGuid(), at, entries);
    }

    protected virtual IEnumerable<Capture> MoreCaptures() => [];

    void Establish()
    {
        _prologue = PrologueId.New();
        _context = EventContext.EmptyWithEventSourceId(_prologue);
        _heuristics = new HeuristicModelBuilder();
        _scope = InterpretationScope.Reduce(new Explore.Scope.Excluding.ShapeExcludedFromInterpretation("POST /api/simulation/start"), null, _context);

        var tables = new List<SchemaTable>
        {
            Table("Authors", [Column("AuthorId"), Column("Name", 200)], ["AuthorId"], [], []),
            Table("Books", [Column("BookId"), Column("Isbn", 20), Column("Title", 200), Column("AuthorId")], ["BookId"], [new SchemaForeignKey("FK_Books_Authors", ["AuthorId"], "dbo", "Authors", ["AuthorId"])], [new SchemaUniqueConstraint("UQ_Books_Isbn", ["Isbn"])]),
            Table("Loans", [Column("LoanId"), Column("BookId"), Column("ReturnedOn", nullable: true)], ["LoanId"], [], []),
            Table("Inventory", [Column("BookId"), Column("AvailableCount"), Column("TotalCount")], ["BookId"], [], [])
        };
        var schema = new Capture(Guid.NewGuid(), _occurred, [new Observation(SourceKind.SqlServer, _occurred, new DatabaseSchemaObserved("sqlserver", "LibraryDb", "library-db", tables))]);

        _captures = [schema];
        for (var index = 0; index < 3; index++)
        {
            _captures.Add(Command("POST", "/api/authors/register", 201, index + 1, new TableChange("dbo", "Authors", ChangeOperation.Insert, ["AuthorId", "Name"])));
        }

        for (var index = 0; index < 4; index++)
        {
            _captures.Add(Command("POST", $"/api/loans/{index + 100}", 201, index + 10, new TableChange("dbo", "Loans", ChangeOperation.Insert, ["LoanId", "BookId"]), new TableChange("dbo", "Inventory", ChangeOperation.Update, ["AvailableCount"])));
        }

        _captures.Add(Command("POST", "/api/loans/200", 409, 20));
        _captures.Add(Command("POST", "/api/loans/201", 409, 21));
        _captures.Add(Command("DELETE", "/api/authors/7", 200, 30, new TableChange("dbo", "Authors", ChangeOperation.Delete, ["AuthorId"])));
        _captures.Add(Command("DELETE", "/api/authors/8", 409, 31));
        _captures.Add(Command("POST", "/api/books/return", 200, 40, new TableChange("dbo", "Loans", ChangeOperation.Update, ["ReturnedOn"]), new TableChange("dbo", "Inventory", ChangeOperation.Update, ["AvailableCount"])));
        _captures.Add(Command("POST", "/api/books/register", 201, 41, new TableChange("dbo", "Books", ChangeOperation.Insert, ["BookId", "Isbn", "Title", "AuthorId"])));
        _captures.Add(Command("POST", "/api/simulation/start", 200, 50));
        _captures.AddRange(MoreCaptures());

        var lend = Captured.Transaction((ChangeOperation.Insert, "Loans", ["LoanId", "BookId"]), (ChangeOperation.Update, "Inventory", ["AvailableCount"]));
        var recorded = new List<CaptureRecorded>();
        recorded.AddRange(Enumerable.Range(0, 3).Select(_ => Captured.Command("POST", "/api/authors/register", 201, "AuthorsController.Register", "Register an author", Captured.Transaction((ChangeOperation.Insert, "Authors", ["AuthorId", "Name"])))));
        recorded.AddRange(Enumerable.Range(0, 4).Select(_ => Captured.Command("POST", "/api/loans/100", 201, "LoansController.Lend", "Lend a copy", lend)));
        recorded.AddRange(Enumerable.Range(0, 2).Select(_ => Captured.Command("POST", "/api/loans/100", 409, "LoansController.Lend")));
        recorded.Add(Captured.Command("DELETE", "/api/authors/7", 200, "AuthorsController.Remove", "", Captured.Transaction((ChangeOperation.Delete, "Authors", ["AuthorId"]))));
        recorded.Add(Captured.Command("DELETE", "/api/authors/8", 409, "AuthorsController.Remove"));
        recorded.Add(Captured.Command("POST", "/api/books/return", 200, "BooksController.Return", "", Captured.Transaction((ChangeOperation.Update, "Loans", ["ReturnedOn"]), (ChangeOperation.Update, "Inventory", ["AvailableCount"]))));
        recorded.Add(Captured.Command("POST", "/api/books/register", 201, "BooksController.Register", "", Captured.Transaction((ChangeOperation.Insert, "Books", ["BookId", "Isbn", "Title", "AuthorId"]))));
        var matrix = Captured.Matrix([.. recorded]);

        var points = new List<DecisionPoint>
        {
            new("POST /api/loans/{id}", DecisionOutcome.Refused, "Member has 5 open loans", "LoanPolicy.cs", 22, "Lending")
        };
        _facts = new EvidenceFacts(tables, [.. matrix.Shapes], points, []);

        _partitions = ScopedCaptures.Partition(_captures, _scope);
        _heuristic = new ScopedHeuristicModelBuilder(_heuristics, _scope).Build(_prologue.Value, ScopedCaptures.Included(_captures, _scope));
        (_suggestions, _model) = SuggestionExtraction.From(_partitions, _heuristic, _heuristic, new Dictionary<string, string>(), _facts);
        _review = EventReview.Reduce(new EventSuggestionsInterpreted(_suggestions), null, _context);
        _review = EventReview.Reduce(new InterpretedModelRecorded(_model), _review, _context);
    }

    static SchemaTable Table(string name, IReadOnlyList<SchemaColumn> columns, IReadOnlyList<string> key, IReadOnlyList<SchemaForeignKey> foreignKeys, IReadOnlyList<SchemaUniqueConstraint> uniques) =>
        new("dbo", name, columns, key, foreignKeys, uniques);

    static SchemaColumn Column(string name, int length = 0, bool nullable = false) => new(name, length > 0 ? "nvarchar" : "uniqueidentifier", length, nullable, false);
}
#endif

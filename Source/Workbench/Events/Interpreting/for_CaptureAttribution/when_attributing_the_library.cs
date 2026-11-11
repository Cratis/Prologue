// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;

namespace Cratis.Prologue.Workbench.Events.Interpreting.for_CaptureAttribution;

public class when_attributing_the_library : a_library_interpretation
{
    IReadOnlyDictionary<SuggestionKey, Attribution> _attributions;

    static string Describe(IReadOnlyDictionary<SuggestionKey, Attribution> attributions) => string.Join(
        '\n',
        attributions.OrderBy(pair => pair.Key.Value, StringComparer.Ordinal).Select(pair =>
            $"{pair.Key.Value} | shapes: {string.Join(", ", pair.Value.Shapes.OrderBy(shape => shape.Key.Value, StringComparer.Ordinal).Select(shape => $"{shape.Key.Value}={shape.Value}"))}" +
            $" | tables: {string.Join(", ", pair.Value.Tables.OrderBy(table => table.Key.ToString(), StringComparer.Ordinal).Select(table => $"{table.Key.Schema}.{table.Key.Table}.{table.Key.Operation}({string.Join('+', table.Value.Order(StringComparer.Ordinal))})"))}"));

    void Because() => _attributions = CaptureAttribution.Attribute(_partitions);

    [Fact] void should_describe_what_the_captures_show() => Describe(_attributions).ShouldEqual(string.Join(
        '\n',
        "Authors/AuthorCreated | shapes: POST /api/authors/register=3 | tables: dbo.Authors.Insert(AuthorId+Name)",
        "Authors/AuthorDeleted | shapes: DELETE /api/authors/{id}=1 | tables: dbo.Authors.Delete(AuthorId)",
        "Books/BookCreated | shapes: POST /api/books/register=1 | tables: dbo.Books.Insert(AuthorId+BookId+Isbn+Title)",
        "Books/InventoryUpdated | shapes: POST /api/books/return=1 | tables: dbo.Inventory.Update(AvailableCount)",
        "Books/LoanUpdated | shapes: POST /api/books/return=1 | tables: dbo.Loans.Update(ReturnedOn)",
        "Loans/InventoryUpdated | shapes: POST /api/loans/{id}=4 | tables: dbo.Inventory.Update(AvailableCount)",
        "Loans/LoanCreated | shapes: POST /api/loans/{id}=4 | tables: dbo.Loans.Insert(BookId+LoanId)"));
    [Fact] void should_key_by_module_and_mechanical_name() => CaptureAttribution.KeyOf("Authors", "AuthorCreated").Value.ShouldEqual("Authors/AuthorCreated");
}

#endif

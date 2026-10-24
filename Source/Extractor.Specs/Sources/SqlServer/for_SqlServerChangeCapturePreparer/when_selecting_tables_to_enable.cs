// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Sources.SqlServer.for_SqlServerChangeCapturePreparer;

public class when_selecting_tables_to_enable : Specification
{
    static readonly (string Schema, string Table)[] _all =
    [
        ("dbo", "Authors"),
        ("dbo", "Books"),
        ("sales", "Orders")
    ];

    IReadOnlyList<(string Schema, string Table)> _withoutAllowlist;
    IReadOnlyList<(string Schema, string Table)> _byBareName;
    IReadOnlyList<(string Schema, string Table)> _byQualifiedName;
    IReadOnlyList<(string Schema, string Table)> _byUnknownName;
    IReadOnlyList<(string Schema, string Table)> _ignoringCasing;

    void Because()
    {
        _withoutAllowlist = SqlServerChangeCapturePreparer.Matching(_all, []);
        _byBareName = SqlServerChangeCapturePreparer.Matching(_all, ["Books"]);
        _byQualifiedName = SqlServerChangeCapturePreparer.Matching(_all, ["sales.Orders"]);
        _byUnknownName = SqlServerChangeCapturePreparer.Matching(_all, ["Nonexistent"]);
        _ignoringCasing = SqlServerChangeCapturePreparer.Matching(_all, ["dbo.authors"]);
    }

    [Fact] void should_take_every_table_when_no_allowlist_is_configured() => _withoutAllowlist.Count.ShouldEqual(3);
    [Fact] void should_match_on_the_bare_table_name() => _byBareName.ShouldContainOnly([("dbo", "Books")]);
    [Fact] void should_match_on_the_qualified_name() => _byQualifiedName.ShouldContainOnly([("sales", "Orders")]);
    [Fact] void should_take_nothing_for_a_table_that_does_not_exist() => _byUnknownName.ShouldBeEmpty();
    [Fact] void should_ignore_casing() => _ignoringCasing.ShouldContainOnly([("dbo", "Authors")]);
}
#endif

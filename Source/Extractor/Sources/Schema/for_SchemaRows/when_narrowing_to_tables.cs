// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Sources.Schema.for_SchemaRows;

public class when_narrowing_to_tables : Specification
{
    readonly SchemaRows _rows = new(
        [
            new("dbo", "Orders", "Id", "uniqueidentifier", 0, false, false, string.Empty),
            new("dbo", "Secrets", "Id", "uniqueidentifier", 0, false, false, string.Empty)
        ],
        [
            new("dbo", "Orders", "Id"),
            new("dbo", "Secrets", "Id")
        ],
        [
            new("dbo", "Secrets", "FK_Secrets_Orders", "OrderId", "dbo", "Orders", "Id")
        ],
        [
            new("dbo", "Secrets", "UQ_Secrets_Name", "Name")
        ]);

    SchemaRows _result;

    void Because() => _result = _rows.OnlyTables([("dbo", "Orders")]);

    [Fact] void should_keep_only_columns_of_allowed_tables() => _result.Columns.Single().Table.ShouldEqual("Orders");
    [Fact] void should_keep_only_primary_keys_of_allowed_tables() => _result.PrimaryKeys.Single().Table.ShouldEqual("Orders");
    [Fact] void should_drop_foreign_keys_of_excluded_tables() => _result.ForeignKeys.ShouldBeEmpty();
    [Fact] void should_drop_unique_constraints_of_excluded_tables() => _result.UniqueConstraints.ShouldBeEmpty();
    [Fact] void should_expose_only_the_allowed_tables() => _result.Tables.ShouldContainOnly([("dbo", "Orders")]);
}
#endif

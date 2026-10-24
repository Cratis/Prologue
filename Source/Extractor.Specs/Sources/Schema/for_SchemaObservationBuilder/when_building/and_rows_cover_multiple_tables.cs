// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Sources.Schema.for_SchemaObservationBuilder.when_building;

public class and_rows_cover_multiple_tables : Specification
{
    DatabaseSchemaObserved _result;

    void Because() => _result = (DatabaseSchemaObserved)SchemaObservationBuilder.Build(
        SourceKind.SqlServer,
        "shop-db",
        "Shop",
        DateTimeOffset.UtcNow,
        new SchemaRows(
            [
                new("dbo", "Orders", "Id", "uniqueidentifier", 0, false, false, string.Empty),
                new("sales", "Invoices", "Id", "uniqueidentifier", 0, false, false, string.Empty)
            ],
            [
                new("sales", "Invoices", "Id")
            ],
            [],
            [])).Payload;

    [Fact] void should_produce_one_table_per_source_table() => _result.Tables.Select(_ => _.Table).ShouldContainOnly(["Orders", "Invoices"]);
    [Fact] void should_attach_the_primary_key_to_the_owning_table() => _result.Tables.Single(_ => _.Table == "Invoices").PrimaryKey.ShouldContainOnly(["Id"]);
    [Fact] void should_leave_the_other_table_without_a_primary_key() => _result.Tables.Single(_ => _.Table == "Orders").PrimaryKey.ShouldBeEmpty();
}
#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Sources.Schema.for_SchemaObservationBuilder.when_building;

public class and_a_table_has_no_constraints : Specification
{
    SchemaTable _table;

    void Because() => _table = ((DatabaseSchemaObserved)SchemaObservationBuilder.Build(
        SourceKind.Postgres,
        "shop-db",
        "shop",
        DateTimeOffset.UtcNow,
        new SchemaRows(
            [new("public", "audit_entries", "message", "text", 0, true, false, string.Empty)],
            [],
            [],
            [])).Payload).Tables[0];

    [Fact] void should_have_the_column() => _table.Columns.Single().Name.ShouldEqual("message");
    [Fact] void should_have_no_primary_key() => _table.PrimaryKey.ShouldBeEmpty();
    [Fact] void should_have_no_foreign_keys() => _table.ForeignKeys.ShouldBeEmpty();
    [Fact] void should_have_no_unique_constraints() => _table.UniqueConstraints.ShouldBeEmpty();
}
#endif

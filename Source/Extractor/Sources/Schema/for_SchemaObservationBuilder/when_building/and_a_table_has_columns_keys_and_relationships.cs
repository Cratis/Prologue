// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Extractor.Capturing;

namespace Cratis.Prologue.Extractor.Sources.Schema.for_SchemaObservationBuilder.when_building;

public class and_a_table_has_columns_keys_and_relationships : Specification
{
    readonly DateTimeOffset _observed = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    Observation _result;
    SchemaTable _table;

    void Because()
    {
        _result = SchemaObservationBuilder.Build(
            SourceKind.SqlServer,
            "shop-db",
            "Shop",
            _observed,
            new SchemaRows(
                [
                    new("dbo", "OrderLines", "OrderId", "uniqueidentifier", 0, false, false, string.Empty),
                    new("dbo", "OrderLines", "LineNumber", "int", 0, false, false, string.Empty),
                    new("dbo", "OrderLines", "Sku", "nvarchar", 64, false, false, string.Empty),
                    new("dbo", "OrderLines", "Note", "nvarchar", 500, true, false, "('')")
                ],
                [
                    new("dbo", "OrderLines", "OrderId"),
                    new("dbo", "OrderLines", "LineNumber")
                ],
                [
                    new("dbo", "OrderLines", "FK_OrderLines_Orders", "OrderId", "dbo", "Orders", "Id")
                ],
                [
                    new("dbo", "OrderLines", "UQ_OrderLines_Sku", "OrderId"),
                    new("dbo", "OrderLines", "UQ_OrderLines_Sku", "Sku")
                ]));

        _table = ((DatabaseSchemaObserved)_result.Payload).Tables[0];
    }

    [Fact] void should_carry_the_engine() => ((DatabaseSchemaObserved)_result.Payload).Engine.ShouldEqual("sqlserver");
    [Fact] void should_carry_the_database() => ((DatabaseSchemaObserved)_result.Payload).Database.ShouldEqual("Shop");
    [Fact] void should_carry_the_configured_source_name() => ((DatabaseSchemaObserved)_result.Payload).Source.ShouldEqual("shop-db");
    [Fact] void should_anchor_to_the_observed_time() => _result.Occurred.ShouldEqual(_observed);
    [Fact] void should_use_the_source_kind() => _result.Source.ShouldEqual(SourceKind.SqlServer);
    [Fact] void should_keep_the_columns_in_order() => _table.Columns.Select(_ => _.Name).ShouldContainOnly(["OrderId", "LineNumber", "Sku", "Note"]);
    [Fact] void should_carry_the_column_size() => _table.Columns.Single(_ => _.Name == "Sku").MaxLength.ShouldEqual(64);
    [Fact] void should_mark_required_columns_as_not_nullable() => _table.Columns.Single(_ => _.Name == "Sku").IsNullable.ShouldBeFalse();
    [Fact] void should_mark_optional_columns_as_nullable() => _table.Columns.Single(_ => _.Name == "Note").IsNullable.ShouldBeTrue();
    [Fact] void should_carry_the_default_expression() => _table.Columns.Single(_ => _.Name == "Note").DefaultExpression.ShouldEqual("('')");
    [Fact] void should_keep_the_primary_key_in_key_order() => _table.PrimaryKey.ShouldContainOnly(["OrderId", "LineNumber"]);
    [Fact] void should_assemble_the_foreign_key() => _table.ForeignKeys.Single().ReferencedTable.ShouldEqual("Orders");
    [Fact] void should_carry_the_foreign_key_columns() => _table.ForeignKeys.Single().Columns.ShouldContainOnly(["OrderId"]);
    [Fact] void should_carry_the_referenced_columns() => _table.ForeignKeys.Single().ReferencedColumns.ShouldContainOnly(["Id"]);
    [Fact] void should_group_the_unique_constraint_columns() => _table.UniqueConstraints.Single().Columns.ShouldContainOnly(["OrderId", "Sku"]);
}
#endif

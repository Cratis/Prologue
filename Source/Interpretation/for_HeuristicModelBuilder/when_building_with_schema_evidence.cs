// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_HeuristicModelBuilder;

public class when_building_with_schema_evidence : Specification
{
    static readonly Guid _prologueId = Guid.NewGuid();
    static readonly DateTimeOffset _occurred = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    HeuristicModelBuilder _builder;
    ExtractionResult _result;
    ExtractedSlice _stateChange;
    ExtractedCommand _command;

    void Establish()
    {
        _builder = new HeuristicModelBuilder();

        // The schema settles into its own capture when capture starts; the command capture arrives later. The two
        // join on database name plus schema-qualified table name.
        var schemaCapture = new Capture(
            Guid.NewGuid(),
            _occurred,
            [
                new Observation(SourceKind.SqlServer, _occurred, new DatabaseSchemaObserved(
                    "sqlserver",
                    "LibraryDb",
                    "library-db",
                    [
                        new SchemaTable(
                            "dbo",
                            "Authors",
                            [
                                new SchemaColumn("AuthorId", "uniqueidentifier", 0, IsNullable: false, IsIdentity: false),
                                new SchemaColumn("Name", "nvarchar", 200, IsNullable: false, IsIdentity: false),
                                new SchemaColumn("Email", "nvarchar", 320, IsNullable: true, IsIdentity: false),
                                new SchemaColumn("Age", "int", 0, IsNullable: false, IsIdentity: false)
                            ],
                            ["AuthorId"],
                            [],
                            [new SchemaUniqueConstraint("UQ_Authors_Email", ["Email"])])
                    ]))
            ],
            _prologueId);

        var commandCapture = new Capture(
            Guid.NewGuid(),
            _occurred.AddSeconds(1),
            [
                new Observation(SourceKind.Http, _occurred.AddSeconds(1), new HttpCommandObserved("POST", "/api/authors/register", 201)),
                new Observation(SourceKind.SqlServer, _occurred.AddSeconds(1).AddMilliseconds(20), new DatabaseTransactionObserved(
                    "sqlserver", "LibraryDb", "tx1", [new TableChange("dbo", "Authors", ChangeOperation.Insert, ["AuthorId", "Name", "Email", "Age"])]))
            ],
            _prologueId);

        _result = _builder.Build(_prologueId, [schemaCapture, commandCapture]);
    }

    void Because()
    {
        _stateChange = _result.Modules.Single().Features.Single().Slices.Single(slice => slice.Type == ExtractedSliceType.StateChange);
        _command = _stateChange.Commands.Single();
    }

    [Fact] void should_type_properties_from_the_schema_over_name_conventions() => CommandProperty("Age").Type.ShouldEqual("int");
    [Fact] void should_type_the_identifier_from_the_schema() => CommandProperty("AuthorId").Type.ShouldEqual("Guid");
    [Fact] void should_mark_non_nullable_columns_required() => CommandProperty("Name").IsRequired.ShouldBeTrue();
    [Fact] void should_not_mark_nullable_columns_required() => CommandProperty("Email").IsRequired.ShouldBeFalse();
    [Fact] void should_carry_the_max_length_of_bounded_string_columns() => CommandProperty("Name").MaxLength.ShouldEqual(200);
    [Fact] void should_not_carry_a_max_length_for_non_string_columns() => CommandProperty("Age").MaxLength.ShouldEqual(0);

    [Fact]
    void should_add_a_required_rule_for_a_non_nullable_column() =>
        ValidationFor("Name", ExtractedValidationRuleKind.Required).Message.ShouldEqual("Name is required");

    [Fact]
    void should_add_a_required_rule_for_every_non_nullable_non_key_column() =>
        _command.Validations.Count(rule => rule.Kind == ExtractedValidationRuleKind.Required).ShouldEqual(2);

    [Fact]
    void should_not_add_a_required_rule_for_the_primary_key() =>
        _command.Validations.Any(rule => rule.Property == "AuthorId").ShouldBeFalse();

    [Fact]
    void should_not_add_a_required_rule_for_a_nullable_column() =>
        _command.Validations.Any(rule => rule.Property == "Email" && rule.Kind == ExtractedValidationRuleKind.Required).ShouldBeFalse();

    [Fact]
    void should_add_a_max_length_rule_for_a_bounded_string_column() =>
        ValidationFor("Name", ExtractedValidationRuleKind.MaxLength).Argument.ShouldEqual("200");

    [Fact]
    void should_add_a_max_length_rule_for_a_bounded_nullable_string_column() =>
        ValidationFor("Email", ExtractedValidationRuleKind.MaxLength).Argument.ShouldEqual("320");

    [Fact] void should_add_a_constraint_from_the_unique_constraint() => Constraint.Name.ShouldEqual("UniqueEmail");
    [Fact] void should_reference_the_property_in_camel_case_on_the_constraint() => Constraint.Property.ShouldEqual("email");
    [Fact] void should_point_the_constraint_at_the_produced_event() => Constraint.OnEvent.ShouldEqual("AuthorCreated");

    [Fact]
    void should_enrich_the_event_properties_from_the_schema() =>
        _stateChange.Events.Single().Properties.Single(property => property.Name == "Name").MaxLength.ShouldEqual(200);

    [Fact]
    void should_enrich_the_read_model_properties_from_the_schema() =>
        StateView.ReadModels.Single().Properties.Single(property => property.Name == "Age").Type.ShouldEqual("int");

    ExtractedSlice StateView => _result.Modules.Single().Features.Single().Slices.Single(slice => slice.Type == ExtractedSliceType.StateView);

    ExtractedConstraint Constraint => _stateChange.Constraints.Single();

    ExtractedProperty CommandProperty(string name) => _command.Properties.Single(property => property.Name == name);

    ExtractedValidationRule ValidationFor(string property, ExtractedValidationRuleKind kind) =>
        _command.Validations.Single(rule => rule.Property == property && rule.Kind == kind);
}
#endif

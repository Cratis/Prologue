// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Contracts;
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_HeuristicModelBuilder;

/// <summary>
/// Two services that both expose an authors endpoint backed by an Authors table must stay two modules — one per
/// service — and each must be analyzed against its own database schema, even though the databases share a name.
/// </summary>
public class when_building_from_captures_of_named_services : Specification
{
    static readonly DateTimeOffset _occurred = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    HeuristicModelBuilder _builder;
    IReadOnlyList<Capture> _captures;
    ExtractionResult _result;

    void Establish()
    {
        _builder = new HeuristicModelBuilder();
        _captures =
        [
            CaptureFor("catalog-service", ["AuthorId", "Name"]),
            CaptureFor("billing", ["AuthorId", "Iban"]),
            CaptureFor(ServiceName.Default, ["AuthorId", "Nickname"]),
        ];
    }

    void Because() => _result = _builder.Build(Guid.Empty, _captures);

    [Fact] void should_make_a_module_per_named_service() => _result.Modules.Select(module => module.Name).ShouldContain("CatalogService");
    [Fact] void should_name_the_other_services_module_after_it() => _result.Modules.Select(module => module.Name).ShouldContain("Billing");
    [Fact] void should_keep_the_path_derived_module_for_the_default_service() => _result.Modules.Select(module => module.Name).ShouldContain("Authors");
    [Fact] void should_not_merge_the_services() => _result.Modules.Count.ShouldEqual(3);
    [Fact] void should_infer_each_services_event_from_its_own_changes() => EventFor("Billing").Properties.Select(property => property.Name).ShouldContainOnly(["AuthorId", "Iban"]);

    static Capture CaptureFor(ServiceName service, IReadOnlyList<string> columns) =>
        new(
            Guid.NewGuid(),
            _occurred,
            [
                new Observation(SourceKind.Http, _occurred, new HttpCommandObserved("POST", "/api/authors/register", 201)) { Service = service },
                new Observation(SourceKind.SqlServer, _occurred.AddMilliseconds(20), new DatabaseTransactionObserved(
                    "sqlserver", "App", "tx1", [new TableChange("dbo", "Authors", ChangeOperation.Insert, columns)])) { Service = service }
            ])
        { Service = service };

    ExtractedEvent EventFor(string module) =>
        _result.Modules.Single(candidate => candidate.Name == module).Features
            .SelectMany(feature => feature.Slices)
            .Single(slice => slice.Type == ExtractedSliceType.StateChange)
            .Events.Single();
}
#endif

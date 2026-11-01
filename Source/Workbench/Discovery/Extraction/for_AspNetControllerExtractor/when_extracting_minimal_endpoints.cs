// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discovery.Extraction.for_AspNetControllerExtractor;

public class when_extracting_minimal_endpoints : Specification
{
    const string Source = """
        app.MapGet("/api/catalog", () => Results.Ok());
        app.MapPost("/api/reservations", Reserve);
        app.MapDelete("/api/reservations/{id}/cancel", Cancel);
        """;

    List<FoundCapability> _capabilities;

    void Because() => _capabilities = [.. AspNetControllerExtractor.Extract(new SourcePath("Program.cs"), Source)];

    [Fact] void should_find_the_endpoints_that_change_state() => _capabilities.Count.ShouldEqual(2);
    [Fact] void should_name_the_module_from_the_route() => _capabilities.TrueForAll(_ => _.Module.Value == "Reservations").ShouldBeTrue();
    [Fact] void should_name_a_collection_endpoint_from_its_verb() => _capabilities[0].Capability.Value.ShouldEqual("Create reservations");
    [Fact] void should_name_an_endpoint_from_its_last_segment() => _capabilities[1].Capability.Value.ShouldEqual("Cancel");
    [Fact] void should_normalize_the_route() => _capabilities[1].Shapes.Single().Value.ShouldEqual("DELETE /api/reservations/{id}/cancel");
}
#endif

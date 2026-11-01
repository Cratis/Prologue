// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discover.Extraction.for_AspNetControllerExtractor;

public class when_extracting_a_controller : Specification
{
    const string Source = """
        using Microsoft.AspNetCore.Mvc;

        namespace Library.Loans;

        [ApiController]
        [Route("api/loans")]
        public class LoansController : ControllerBase
        {
            [HttpGet("{id:int}")]
            public IActionResult Get(int id) => Ok();

            [HttpPost]
            public async Task<IActionResult> LendCopyAsync(LendRequest request) => Ok();

            [HttpPost("{id:int}/return")]
            [ProducesResponseType(typeof(Loan), 200)]
            public IActionResult ReturnLoan(int id) => Ok();

            [HttpPut, HttpPatch("{id}/extend")]
            public IActionResult Extend(int id) => Ok();
        }

        public class LoanPolicy
        {
            [HttpPost("never")]
            public void NotAnAction() { }
        }
        """;

    List<FoundCapability> _capabilities;

    void Because() => _capabilities = [.. AspNetControllerExtractor.Extract(new SourcePath("Loans/LoansController.cs"), Source)];

    [Fact] void should_find_the_actions_that_change_state() => _capabilities.Count.ShouldEqual(3);
    [Fact] void should_skip_queries() => _capabilities.Exists(_ => _.Capability.Value == "Get").ShouldBeFalse();
    [Fact] void should_name_the_module_after_the_controller() => _capabilities.TrueForAll(_ => _.Module.Value == "Loans").ShouldBeTrue();
    [Fact] void should_name_a_capability_from_its_action() => _capabilities[0].Capability.Value.ShouldEqual("Lend copy");
    [Fact] void should_use_the_controller_route_for_an_action_without_a_template() => _capabilities[0].Shapes.Single().Value.ShouldEqual("POST /api/loans");
    [Fact] void should_normalize_route_parameters() => _capabilities[1].Shapes.Single().Value.ShouldEqual("POST /api/loans/{id}/return");
    [Fact] void should_record_every_verb_of_an_action() => _capabilities[2].Shapes.Select(_ => _.Value).ShouldContainOnly("PUT /api/loans", "PATCH /api/loans/{id}/extend");
    [Fact] void should_record_the_file() => _capabilities[0].File.Value.ShouldEqual("Loans/LoansController.cs");
    [Fact] void should_ignore_classes_that_are_not_controllers() => _capabilities.Exists(_ => _.Capability.Value.Contains("action", StringComparison.OrdinalIgnoreCase)).ShouldBeFalse();
}
#endif

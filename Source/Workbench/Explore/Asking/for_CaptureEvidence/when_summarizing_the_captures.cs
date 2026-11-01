// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Explore.given;

namespace Cratis.Prologue.Workbench.Explore.Asking.for_CaptureEvidence;

public class when_summarizing_the_captures : a_lending_library
{
    string _summary;
    string _withoutScope;

    void Because()
    {
        _summary = CaptureEvidence.Summarize(_matrix, _uncorrelated, null, _scope);
        _withoutScope = CaptureEvidence.Summarize(_matrix, _uncorrelated, null, null);
    }

    [Fact] void should_name_every_shape_with_its_count() => _summary.ShouldContain("- POST /api/loans | 5 |");
    [Fact] void should_give_the_status_mix() => _summary.ShouldContain("201 80%, 409 20%");
    [Fact] void should_name_the_handler() => _summary.ShouldContain("LoansController.Lend");
    [Fact] void should_give_the_writes_with_their_share_of_runs_and_columns() => _summary.ShouldContain("Loans insert 80% (BookId, MemberId)");
    [Fact] void should_name_the_scenarios() => _summary.ShouldContain("Lend a copy 5");
    [Fact] void should_include_the_writes_with_no_command() => _summary.ShouldContain("- Inventory | 1 | update 1 | 02:00 x1");
    [Fact] void should_leave_out_the_shapes_excluded_from_interpretation() => _summary.ShouldNotContain("/api/simulation");
    [Fact] void should_include_them_when_nothing_is_excluded() => _withoutScope.ShouldContain("/api/simulation");
}
#endif

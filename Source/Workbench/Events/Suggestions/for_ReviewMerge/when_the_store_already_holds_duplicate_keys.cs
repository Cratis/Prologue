// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Events.given;

namespace Cratis.Prologue.Workbench.Events.Suggestions.for_ReviewMerge;

public class when_the_store_already_holds_duplicate_keys : an_uncaptured_library
{
    IReadOnlyList<ReviewedSuggestion> _merged;
    IReadOnlyList<ReviewedSuggestion> _foundTwice;

    void Because()
    {
        var plain = new ReviewedSuggestion(_suggestions[0], SuggestionStatus.Suggested, _suggestions[0].Refined, false, false);
        var accepted = plain with { Status = SuggestionStatus.Accepted };
        _merged = [.. ReviewMerge.Merge([plain, accepted, plain], [_suggestions[0]])];
        _foundTwice = [.. ReviewMerge.Merge([], [_suggestions[0], _suggestions[0] with { Mechanical = "Other" }])];
    }

    [Fact] void should_not_throw_and_keep_one() => _merged.Count.ShouldEqual(1);
    [Fact] void should_keep_the_one_that_was_decided() => _merged[0].Status.ShouldEqual(SuggestionStatus.Accepted);
    [Fact] void should_list_a_key_found_twice_once() => _foundTwice.Count.ShouldEqual(1);
}
#endif

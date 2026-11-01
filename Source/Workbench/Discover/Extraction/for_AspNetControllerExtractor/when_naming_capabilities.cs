// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discover.Extraction.for_AspNetControllerExtractor;

public class when_naming_capabilities : Specification
{
    string _async;
    string _acronym;
    string _single;

    void Because()
    {
        _async = Humanize.Name("ReturnLoanAsync");
        _acronym = Humanize.Name("ImportISBNCatalog");
        _single = Humanize.Name("lend");
    }

    [Fact] void should_split_words_and_drop_the_async_suffix() => _async.ShouldEqual("Return loan");
    [Fact] void should_keep_an_acronym_together() => _acronym.ShouldEqual("Import ISBN catalog");
    [Fact] void should_capitalize_a_single_word() => _single.ShouldEqual("Lend");
}
#endif

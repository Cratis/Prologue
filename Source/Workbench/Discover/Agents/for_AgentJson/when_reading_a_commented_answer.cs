// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json;

namespace Cratis.Prologue.Workbench.Discover.Agents.for_AgentJson;

public class when_reading_a_commented_answer : Specification
{
    IReadOnlyList<JsonElement> _objectEntries;
    IReadOnlyList<JsonElement> _arrayEntries;
    string? _objectProblem;
    string? _arrayProblem;

    void Because()
    {
        _objectEntries = AgentJson.Entries("{\"items\":[/* a finding */{\"name\":\"Loans\"},],}", "items", out _objectProblem);
        _arrayEntries = AgentJson.Entries("[/* a finding */{\"name\":\"Returns\"},]", "items", out _arrayProblem);
    }

    [Fact] void should_keep_the_object_entry_readable_after_parsing() => _objectEntries.Single().GetProperty("name").GetString().ShouldEqual("Loans");
    [Fact] void should_keep_the_array_entry_readable_after_parsing() => _arrayEntries.Single().GetProperty("name").GetString().ShouldEqual("Returns");
    [Fact] void should_accept_comments_and_trailing_commas_in_an_object() => _objectProblem.ShouldBeNull();
    [Fact] void should_accept_comments_and_trailing_commas_in_an_array() => _arrayProblem.ShouldBeNull();
}
#endif

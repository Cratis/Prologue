// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Decisions.for_DecisionRules;

public class when_classifying : Specification
{
    static DecisionOutcome Classify(int status, string path = "/api/loans") =>
        DecisionRules.Classify(new RecordedCommand("POST", path, status, TraceId.None, CommandShape.From("POST", path), 0));

    [Fact] void should_accept_a_success() => Classify(201).ShouldEqual(DecisionOutcome.Accepted);
    [Fact] void should_accept_a_redirect() => Classify(302).ShouldEqual(DecisionOutcome.Accepted);
    [Fact] void should_refuse_a_client_error() => Classify(409).ShouldEqual(DecisionOutcome.Refused);
    [Fact] void should_refuse_a_server_error() => Classify(500).ShouldEqual(DecisionOutcome.Refused);
    [Fact] void should_not_decide_on_an_informational_status() => Classify(101).ShouldEqual(DecisionOutcome.NotDecided);
    [Fact] void should_not_decide_without_a_status() => Classify(0).ShouldEqual(DecisionOutcome.NotDecided);
    [Fact] void should_not_decide_when_there_is_no_command() => DecisionRules.Classify(RecordedCommand.None).ShouldEqual(DecisionOutcome.NotDecided);
}
#endif

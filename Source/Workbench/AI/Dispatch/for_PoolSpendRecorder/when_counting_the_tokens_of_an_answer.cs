// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch.for_PoolSpendRecorder;

public class when_counting_the_tokens_of_an_answer : Specification
{
    [Fact] void should_use_the_total_when_reported() => PoolSpendRecorder.TokensOf(new UsageDetails { TotalTokenCount = 30, InputTokenCount = 1, OutputTokenCount = 1 }).ShouldEqual(30);
    [Fact] void should_add_input_and_output_when_there_is_no_total() => PoolSpendRecorder.TokensOf(new UsageDetails { InputTokenCount = 10, OutputTokenCount = 5 }).ShouldEqual(15);
    [Fact] void should_count_nothing_when_the_provider_reported_nothing() => PoolSpendRecorder.TokensOf(null).ShouldEqual(0);
}
#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Discover.Extraction.for_SourceFileScanner;

public class when_batching_files : Specification
{
    IReadOnlyList<IReadOnlyList<SourceFile>> _bySize;
    IReadOnlyList<IReadOnlyList<SourceFile>> _byCount;
    IReadOnlyList<IReadOnlyList<SourceFile>> _none;

    void Because()
    {
        _bySize = AnalysisBatches.Split(Enumerable.Range(0, 5).Select(index => new SourceFile(new SourcePath($"f{index}.cs"), 25_000)));
        _byCount = AnalysisBatches.Split(Enumerable.Range(0, 60).Select(index => new SourceFile(new SourcePath($"f{index:00}.cs"), 100)));
        _none = AnalysisBatches.Split([]);
    }

    [Fact] void should_keep_a_batch_under_the_size_limit() => _bySize.Select(_ => _.Count).ShouldEqual([2, 2, 1]);
    [Fact] void should_keep_a_batch_under_the_file_limit() => _byCount.Select(_ => _.Count).ShouldEqual([25, 25, 10]);
    [Fact] void should_keep_the_order() => _byCount.SelectMany(_ => _).First().Path.Value.ShouldEqual("f00.cs");
    [Fact] void should_make_no_batch_of_nothing() => _none.ShouldBeEmpty();
}
#endif

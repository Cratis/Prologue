// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Captures.given;
using Cratis.Prologue.Workbench.Explore.given;

namespace Cratis.Prologue.Workbench.Explore.Asking.for_CaptureEvidence;

public class when_there_is_more_than_fits : Specification
{
    string _summary;
    int _shapesListed;

    void Because()
    {
        var wide = Captured.Transaction([.. Enumerable.Range(0, 40).Select(table => (ChangeOperation.Update, $"Table{table}With{new string('X', 60)}", new[] { "Column" }))]);
        var matrix = Captured.Matrix([.. Enumerable.Range(0, CaptureEvidence.ShapesKept + 20).Select(index => Captured.Command("POST", $"/api/area{(char)('a' + (index / 26))}{(char)('a' + (index % 26))}/do", 200, "", "", wide))]);
        _summary = CaptureEvidence.Summarize(matrix, null, null, null);
        _shapesListed = CaptureEvidence.Shapes(matrix, null).Count();
    }

    [Fact] void should_keep_the_summary_within_the_limit() => (_summary.Length <= CaptureEvidence.MaximumCharacters).ShouldBeTrue();
    [Fact] void should_list_no_more_shapes_than_the_limit() => _shapesListed.ShouldEqual(CaptureEvidence.ShapesKept);
}
#endif

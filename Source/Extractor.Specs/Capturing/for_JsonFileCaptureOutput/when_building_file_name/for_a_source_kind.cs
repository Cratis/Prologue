// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Extractor.Capturing.for_JsonFileCaptureOutput.when_building_file_name;

public class for_a_source_kind : Specification
{
    string _first = string.Empty;
    string _rolled = string.Empty;

    void Because()
    {
        _first = JsonFileCaptureOutput.FileName("sqlserver", 1);
        _rolled = JsonFileCaptureOutput.FileName("http", 42);
    }

    [Fact] void should_include_the_source_kind_and_zero_padded_index() => _first.ShouldEqual("prologue-sqlserver-00001.jsonl");
    [Fact] void should_use_the_given_index() => _rolled.ShouldEqual("prologue-http-00042.jsonl");
}
#endif

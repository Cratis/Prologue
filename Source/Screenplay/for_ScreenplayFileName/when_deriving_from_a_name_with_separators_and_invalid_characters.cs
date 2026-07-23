// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Screenplay.for_ScreenplayFileName;

public class when_deriving_from_a_name_with_separators_and_invalid_characters : Specification
{
    string _fileName;

    void Because() => _fileName = ScreenplayFileName.For("library system-2!");

    [Fact] void should_sanitize_to_pascal_case() => _fileName.ShouldEqual("LibrarySystem2.play");
}
#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Screenplay.for_ScreenplayFileName;

public class when_deriving_from_a_pascal_case_name : Specification
{
    string _fileName;

    void Because() => _fileName = ScreenplayFileName.For("LibrarySystem");

    [Fact] void should_keep_the_name_and_append_the_extension() => _fileName.ShouldEqual("LibrarySystem.play");
}
#endif

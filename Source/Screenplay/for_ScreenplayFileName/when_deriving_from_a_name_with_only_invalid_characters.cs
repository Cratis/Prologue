// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Screenplay.for_ScreenplayFileName;

public class when_deriving_from_a_name_with_only_invalid_characters : Specification
{
    string _fileName;

    void Because() => _fileName = ScreenplayFileName.For("!?/");

    [Fact] void should_fall_back_to_the_captured_system_name() => _fileName.ShouldEqual("CapturedSystem.play");
}
#endif

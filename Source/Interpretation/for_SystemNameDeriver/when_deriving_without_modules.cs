// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;

namespace Cratis.Prologue.Interpretation.for_SystemNameDeriver;

public class when_deriving_without_modules : Specification
{
    string _name;

    void Because() => _name = SystemNameDeriver.Derive(ExtractionResult.Empty(Guid.NewGuid()));

    [Fact] void should_fall_back_to_the_captured_system_name() => _name.ShouldEqual("CapturedSystem");
}
#endif

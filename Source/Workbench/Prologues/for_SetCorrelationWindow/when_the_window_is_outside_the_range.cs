// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.Correlation;
using Cratis.Prologue.Workbench.Prologues.given;

namespace Cratis.Prologue.Workbench.Prologues.for_SetCorrelationWindow;

public class when_the_window_is_outside_the_range : a_prologue
{
    Result<CorrelationWindowSet, ValidationResult> _tooShort;
    Result<CorrelationWindowSet, ValidationResult> _tooLong;
    Result<CorrelationWindowSet, ValidationResult> _fine;

    void Because()
    {
        _tooShort = new SetCorrelationWindow(_id, new CorrelationWindow(10)).Handle(_prologue);
        _tooLong = new SetCorrelationWindow(_id, new CorrelationWindow(120_000)).Handle(_prologue);
        _fine = new SetCorrelationWindow(_id, new CorrelationWindow(1500)).Handle(_prologue);
    }

    [Fact] void should_refuse_a_window_that_is_too_short() => _tooShort.Appended().ShouldBeNull();
    [Fact] void should_refuse_a_window_that_is_too_long() => _tooLong.Appended().ShouldBeNull();
    [Fact] void should_accept_a_window_in_range() => _fine.Appended().Window.Value.ShouldEqual(1500);
}
#endif

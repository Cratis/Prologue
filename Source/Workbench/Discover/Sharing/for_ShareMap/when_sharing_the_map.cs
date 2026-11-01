// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.given;

namespace Cratis.Prologue.Workbench.Discover.Sharing.for_ShareMap;

public class when_sharing_the_map : a_prologue
{
    Result<MapShared, ValidationResult> _first;
    Result<MapShared, ValidationResult> _second;
    Result<MapShared, ValidationResult> _again;
    Result<MapShared, ValidationResult> _undefined;
    Result<MapSharingStopped, ValidationResult> _stopped;
    Result<MapSharingStopped, ValidationResult> _stopNotShared;
    Result<MapShared, ValidationResult> _afterStopping;

    void Because()
    {
        _first = new ShareMap(_id).Handle(_prologue, null);
        _second = new ShareMap(_id).Handle(_prologue, null);
        var shared = SharedMap.Reduce(_first.Appended()!, null, EventContext.EmptyWithEventSourceId(_id));
        _again = new ShareMap(_id).Handle(_prologue, shared);
        _undefined = new ShareMap(_id).Handle(null, null);
        _stopped = new StopSharingMap(_id).Handle(shared);
        _stopNotShared = new StopSharingMap(_id).Handle(null);
        var after = SharedMap.Reduce(_stopped.Appended()!, shared, EventContext.EmptyWithEventSourceId(_id));
        _afterStopping = new ShareMap(_id).Handle(_prologue, after);
    }

    [Fact] void should_share_with_a_token() => (_first.Appended()!.Token.Value.Length > 20).ShouldBeTrue();
    [Fact] void should_give_every_share_its_own_token() => _first.Appended()!.Token.ShouldNotEqual(_second.Appended()!.Token);
    [Fact] void should_refuse_sharing_a_map_that_is_shared() => _again.Refusal().Message.ShouldContain("shared already");
    [Fact] void should_refuse_an_undefined_prologue() => _undefined.Refusal().Message.ShouldContain("not defined");
    [Fact] void should_stop_with_the_token_it_had() => _stopped.Appended()!.Token.ShouldEqual(_first.Appended()!.Token);
    [Fact] void should_refuse_stopping_a_map_that_is_not_shared() => _stopNotShared.Refusal().Message.ShouldContain("not shared");
    [Fact] void should_allow_sharing_again_after_stopping() => _afterStopping.Appended().ShouldNotBeNull();
}
#endif

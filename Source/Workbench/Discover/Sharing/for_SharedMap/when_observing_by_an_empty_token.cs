// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle.ReadModels;

namespace Cratis.Prologue.Workbench.Discover.Sharing.for_SharedMap;

public class when_observing_by_an_empty_token : Specification
{
    IReadModels _readModels;
    IEnumerable<SharedMap> _none;
    IEnumerable<SharedMap> _blank;

    void Establish() => _readModels = Substitute.For<IReadModels>();

    void Because()
    {
        using var none = SharedMap.ObserveSharedMapByToken(ShareToken.None, _readModels).Subscribe(shares => _none = shares);
        using var blank = SharedMap.ObserveSharedMapByToken(new ShareToken("  "), _readModels).Subscribe(shares => _blank = shares);
    }

    [Fact] void should_find_no_share_for_the_none_token() => _none.ShouldBeEmpty();
    [Fact] void should_find_no_share_for_a_blank_token() => _blank.ShouldBeEmpty();
    [Fact] void should_not_ask_the_stored_shares() => _readModels.ReceivedCalls().ShouldBeEmpty();
}
#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_CloneDecision;

public class when_the_clone_ends_where_the_last_one_did : Specification
{
    static readonly SystemClone _cloned = new(new SystemCloneId(Guid.NewGuid()), CloneStatus.Cloned, new CommitId("abc"), new FileCount(12), null, DateTimeOffset.UtcNow);
    static readonly SystemClone _failed = _cloned with { Status = CloneStatus.Failed, Reason = new FailureReason("boom") };

    IEnumerable<object> _sameCommit;
    IEnumerable<object> _sameFailure;

    void Because()
    {
        _sameCommit = CloneDecision.Decide(CloneResult.Cloned(new CommitId("abc"), new FileCount(12), "/clones/a"), _cloned);
        _sameFailure = CloneDecision.Decide(CloneResult.Failed("boom"), _failed);
    }

    [Fact] void should_record_nothing_for_the_same_commit() => _sameCommit.ShouldBeEmpty();
    [Fact] void should_record_nothing_for_the_same_failure() => _sameFailure.ShouldBeEmpty();
}
#endif

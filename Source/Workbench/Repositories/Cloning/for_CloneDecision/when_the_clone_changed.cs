// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Prologue.Workbench.Repositories.Cloning.for_CloneDecision;

public class when_the_clone_changed : Specification
{
    static readonly SystemClone _cloned = new(new SystemCloneId(Guid.NewGuid()), CloneStatus.Cloned, new CommitId("abc"), new FileCount(12), null, DateTimeOffset.UtcNow);
    static readonly SystemClone _failed = _cloned with { Status = CloneStatus.Failed, Reason = new FailureReason("boom") };

    IEnumerable<object> _firstClone;
    IEnumerable<object> _newCommit;
    IEnumerable<object> _newFileCount;
    IEnumerable<object> _recovered;
    IEnumerable<object> _newFailure;

    void Because()
    {
        _firstClone = CloneDecision.Decide(CloneResult.Cloned(new CommitId("abc"), new FileCount(12), "/clones/a"), null);
        _newCommit = CloneDecision.Decide(CloneResult.Cloned(new CommitId("def"), new FileCount(12), "/clones/a"), _cloned);
        _newFileCount = CloneDecision.Decide(CloneResult.Cloned(new CommitId("abc"), new FileCount(13), "/clones/a"), _cloned);
        _recovered = CloneDecision.Decide(CloneResult.Cloned(new CommitId("abc"), new FileCount(12), "/clones/a"), _failed);
        _newFailure = CloneDecision.Decide(CloneResult.Failed("other"), _failed);
    }

    [Fact] void should_record_the_first_clone() => _firstClone.Single().ShouldBeOfExactType<SystemRepositoryCloned>();
    [Fact] void should_record_a_new_commit() => ((SystemRepositoryCloned)_newCommit.Single()).Commit.Value.ShouldEqual("def");
    [Fact] void should_record_a_changed_file_count() => ((SystemRepositoryCloned)_newFileCount.Single()).Files.Value.ShouldEqual(13);
    [Fact] void should_record_a_clone_after_a_failure() => _recovered.Single().ShouldBeOfExactType<SystemRepositoryCloned>();
    [Fact] void should_record_a_different_failure() => _newFailure.Single().ShouldBeOfExactType<SystemRepositoryCloneFailed>();
}
#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Discover.Analyzing.for_AnalysisTargets.given;
using Cratis.Prologue.Workbench.Repositories.Cloning;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discover.Analyzing.for_AnalysisTargets;

public class when_resolving_a_system : a_located_prologue
{
    static SystemClone Cloned(SystemId system, string commit) =>
        new(new(system.Value), CloneStatus.Cloned, new CommitId(commit), new FileCount(40), null, DateTimeOffset.UtcNow);

    Result<AnalysisTarget, ValidationResult> _cloned;
    Result<AnalysisTarget, ValidationResult> _notCloned;
    Result<AnalysisTarget, ValidationResult> _failedClone;
    Result<AnalysisTarget, ValidationResult> _local;
    Result<AnalysisTarget, ValidationResult> _unlocated;
    Result<AnalysisTarget, ValidationResult> _unknown;
    Result<AnalysisTarget, ValidationResult> _undefined;

    void Because()
    {
        _cloned = AnalysisTargets.ForSystem(_located, _lending, Cloned(_lending, "a41c9e2"), "/var/prologue/repos");
        _notCloned = AnalysisTargets.ForSystem(_located, _lending, null, "/var/prologue/repos");
        _failedClone = AnalysisTargets.ForSystem(_located, _lending, Cloned(_lending, "a41c9e2") with { Status = CloneStatus.Failed }, "/var/prologue/repos");
        _local = AnalysisTargets.ForSystem(_located, _search, null, "/var/prologue/repos");
        _unlocated = AnalysisTargets.ForSystem(_prologue, _lending, null, "/var/prologue/repos");
        _unknown = AnalysisTargets.ForSystem(_located, SystemId.New(), null, "/var/prologue/repos");
        _undefined = AnalysisTargets.ForSystem(null, _lending, null, "/var/prologue/repos");
    }

    [Fact] void should_read_the_clone_directory_plus_the_sub_path() => _cloned.Appended()!.Directory.Value.ShouldEqual("/var/prologue/repos/github/cratis/library-platform/main/services/core");
    [Fact] void should_record_the_commit_as_the_source() => _cloned.Appended()!.Source.Value.ShouldEqual("a41c9e2");
    [Fact] void should_use_the_system_as_the_subject() => _cloned.Appended()!.Subject.ShouldEqual(AnalysisSubjectId.For(_lending));
    [Fact] void should_refuse_a_system_that_is_not_cloned() => _notCloned.Refusal().Message.ShouldContain("Clone");
    [Fact] void should_refuse_a_system_whose_clone_failed() => _failedClone.Refusal().Message.ShouldContain("Clone");
    [Fact] void should_read_a_local_system_where_it_is() => _local.Appended()!.Directory.Value.ShouldEqual("/srv/library/search");
    [Fact] void should_read_a_local_system_without_a_clone() => _local.Appended()!.Source.Value.ShouldEqual("working tree");
    [Fact] void should_refuse_a_system_with_no_code_location() => _unlocated.Refusal().Message.ShouldContain("where the system's code lives");
    [Fact] void should_refuse_an_unknown_system() => _unknown.Refusal().Message.ShouldContain("no such system");
    [Fact] void should_refuse_an_undefined_prologue() => _undefined.Refusal().Message.ShouldContain("not defined");
}
#endif

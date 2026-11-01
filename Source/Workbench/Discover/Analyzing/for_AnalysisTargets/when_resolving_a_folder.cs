// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Workbench.Common.given;
using Cratis.Prologue.Workbench.Prologues.given;
using Cratis.Prologue.Workbench.Systems;

namespace Cratis.Prologue.Workbench.Discover.Analyzing.for_AnalysisTargets;

public class when_resolving_a_folder : a_prologue
{
    Result<AnalysisTarget, ValidationResult> _folder;
    Result<AnalysisTarget, ValidationResult> _sameFolder;
    Result<AnalysisTarget, ValidationResult> _missing;
    Result<AnalysisTarget, ValidationResult> _blank;
    Result<AnalysisTarget, ValidationResult> _undefined;

    void Because()
    {
        _folder = AnalysisTargets.ForFolder(_prologue, new FolderPath("/srv/legacy/library"), true);
        _sameFolder = AnalysisTargets.ForFolder(_prologue, new FolderPath(" /srv/Legacy/library/ "), true);
        _missing = AnalysisTargets.ForFolder(_prologue, new FolderPath("/srv/gone"), false);
        _blank = AnalysisTargets.ForFolder(_prologue, new FolderPath("  "), false);
        _undefined = AnalysisTargets.ForFolder(null, new FolderPath("/srv/legacy/library"), true);
    }

    [Fact] void should_name_the_folder_after_its_last_segment() => _folder.Appended()!.Name.Value.ShouldEqual("library");
    [Fact] void should_read_the_folder_itself() => _folder.Appended()!.Directory.Value.ShouldEqual("/srv/legacy/library");
    [Fact] void should_treat_it_as_a_local_folder() => _folder.Appended()!.Host.ShouldEqual(RepositoryHost.LocalFolder);
    [Fact] void should_make_the_same_folder_the_same_subject() => _sameFolder.Appended()!.Subject.ShouldEqual(_folder.Appended()!.Subject);
    [Fact] void should_refuse_a_folder_that_does_not_exist() => _missing.Refusal().Message.ShouldContain("does not exist");
    [Fact] void should_refuse_a_blank_path() => _blank.Refusal().Message.ShouldContain("folder path");
    [Fact] void should_refuse_an_undefined_prologue() => _undefined.Refusal().Message.ShouldContain("not defined");
}
#endif

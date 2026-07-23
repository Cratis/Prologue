// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Screenplay;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Printing;
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Screenplay.for_ScreenplayGenerator;

public class when_compiling_a_play_with_multi_line_descriptions : Specification
{
    const string ModuleDescription = "Everything about the library.\nCovers the authors and their catalog.";
    const string SliceDescription = "Registers an author in the catalog.\nEvery author must have a unique email address.";
    const string CommandDescription = "Registers an author with their name.\nValidates that the name is present.\nProduces the registration fact.";

    ScreenplayGenerator _generator;
    ExtractionResult _result;
    CompilationResult<ApplicationSyntax> _compilation;

    void Establish()
    {
        _generator = new ScreenplayGenerator(new ScreenplayPrinter());
        _result = new ExtractionResult(
            Guid.NewGuid(),
            [
                new ExtractedModule(
                    "Library",
                    [
                        new ExtractedFeature(
                            "Authors",
                            [],
                            [
                                new ExtractedSlice(
                                    "Register",
                                    ExtractedSliceType.StateChange,
                                    [new ExtractedCommand("RegisterAuthor", [new ExtractedProperty("Name", "string")], [], CommandDescription)],
                                    [new ExtractedEvent("AuthorRegistered", [new ExtractedProperty("Name", "string")])],
                                    [],
                                    [],
                                    [],
                                    SliceDescription)
                            ])
                    ],
                    ModuleDescription)
            ],
            "LibrarySystem");
    }

    void Because() => _compilation = new ScreenplayCompiler().Compile(_generator.Generate(_result));

    [Fact] void should_compile_without_errors() => _compilation.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    [Fact] void should_round_trip_the_module_description() => _compilation.Value!.Modules.Single().Description.ShouldEqual(ModuleDescription);
    [Fact] void should_round_trip_the_slice_description() => Slice.Description.ShouldEqual(SliceDescription);
    [Fact] void should_round_trip_the_command_description() => Slice.Commands.Single().Description.ShouldEqual(CommandDescription);

    SliceSyntax Slice => _compilation.Value!.Modules.Single().Features.Single().Slices.Single();
}
#endif

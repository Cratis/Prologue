// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Screenplay;
using Cratis.Screenplay.Diagnostics;
using Cratis.Screenplay.Syntax;

namespace Cratis.Prologue.Screenplay.for_ScreenplayGenerator;

public class when_compiling_the_generated_play : given.a_library_extraction_result
{
    CompilationResult<ApplicationSyntax> _compilation;

    void Because() => _compilation = new ScreenplayCompiler().Compile(_generator.Generate(_result));

    [Fact] void should_compile_without_errors() => _compilation.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
    [Fact] void should_compile_without_warnings() => _compilation.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
    [Fact] void should_produce_an_application() => _compilation.Value.ShouldNotBeNull();
    [Fact] void should_round_trip_the_module() => _compilation.Value!.Modules.Single().Name.ShouldEqual("Library");
}
#endif

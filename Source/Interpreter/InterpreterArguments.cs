// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Interpreter.Contracts;
using Cratis.Prologue.Screenplay;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents the command-line arguments for the interpreter job — where the capture files are mounted, where the
/// extraction result goes, where the generated Screenplay goes and which Prologue the captures belong to. Every
/// value can also be supplied through an environment variable, which is how the containerized job is typically
/// configured.
/// </summary>
/// <param name="CapturesFolder">The folder holding the capture files from the Extractor.</param>
/// <param name="OutputFile">The file the extraction result is written to.</param>
/// <param name="PrologueId">The Prologue the captures belong to.</param>
/// <param name="PlayOutput">The file the generated Screenplay is written to; empty to derive it from the output folder and the system name.</param>
public record InterpreterArguments(string CapturesFolder, string OutputFile, Guid PrologueId, string PlayOutput = "")
{
    /// <summary>
    /// The default folder capture files are read from when nothing is specified.
    /// </summary>
    public const string DefaultCapturesFolder = "/captures";

    /// <summary>
    /// The default file the extraction result is written to when nothing is specified.
    /// </summary>
    public const string DefaultOutputFile = $"/output/{ExtractionResultFile.FileName}";

    /// <summary>
    /// Resolves the file the generated Screenplay is written to — the explicit <see cref="PlayOutput"/> when one
    /// was supplied, otherwise the extraction result's folder combined with the file name derived from the
    /// system name.
    /// </summary>
    /// <param name="systemName">The system name derived for the captured system.</param>
    /// <returns>The path of the Screenplay file to write.</returns>
    public string PlayOutputFor(string systemName) =>
        PlayOutput.Length > 0
            ? PlayOutput
            : Path.Combine(Path.GetDirectoryName(OutputFile) ?? string.Empty, ScreenplayFileName.For(systemName));

    /// <summary>
    /// Parses the command-line arguments, falling back to the <c>PROLOGUE_CAPTURES</c>, <c>PROLOGUE_OUTPUT</c>,
    /// <c>PROLOGUE_PLAY_OUTPUT</c> and <c>PROLOGUE_ID</c> environment variables and then the defaults.
    /// </summary>
    /// <param name="args">The raw command-line arguments.</param>
    /// <returns>The parsed arguments, or <see langword="null"/> when the arguments are malformed.</returns>
    public static InterpreterArguments? Parse(string[] args)
    {
        var captures = Environment.GetEnvironmentVariable("PROLOGUE_CAPTURES") is { Length: > 0 } capturesFromEnvironment ? capturesFromEnvironment : DefaultCapturesFolder;
        var output = Environment.GetEnvironmentVariable("PROLOGUE_OUTPUT") is { Length: > 0 } outputFromEnvironment ? outputFromEnvironment : DefaultOutputFile;
        var playOutput = Environment.GetEnvironmentVariable("PROLOGUE_PLAY_OUTPUT") is { Length: > 0 } playOutputFromEnvironment ? playOutputFromEnvironment : string.Empty;
        var prologueId = Guid.TryParse(Environment.GetEnvironmentVariable("PROLOGUE_ID"), out var idFromEnvironment) ? idFromEnvironment : Guid.Empty;

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--captures" when index + 1 < args.Length:
                    captures = args[++index];
                    break;
                case "--output" when index + 1 < args.Length:
                    output = args[++index];
                    break;
                case "--play-output" when index + 1 < args.Length:
                    playOutput = args[++index];
                    break;
                case "--prologue-id" when index + 1 < args.Length && Guid.TryParse(args[index + 1], out var parsed):
                    prologueId = parsed;
                    index++;
                    break;
                default:
                    return null;
            }
        }

        return new InterpreterArguments(captures, output, prologueId, playOutput);
    }
}

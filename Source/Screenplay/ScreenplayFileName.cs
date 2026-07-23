// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Screenplay;

/// <summary>
/// Derives the file name a generated Screenplay document is written to — the system name sanitized to a PascalCase
/// identifier with the <c>.play</c> extension, falling back to <see cref="Fallback"/> when the name yields nothing.
/// </summary>
public static class ScreenplayFileName
{
    /// <summary>
    /// The file extension of a Screenplay document.
    /// </summary>
    public const string Extension = ".play";

    /// <summary>
    /// The base name used when the system name holds no letters or digits to derive one from.
    /// </summary>
    public const string Fallback = "CapturedSystem";

    /// <summary>
    /// Derives the file name for a system name.
    /// </summary>
    /// <param name="systemName">The system name derived for the captured system; may be empty.</param>
    /// <returns>The derived file name (for example <c>LibrarySystem.play</c>).</returns>
    public static string For(string systemName)
    {
        var words = systemName.Split(
            [.. systemName.Where(character => !char.IsLetterOrDigit(character)).Distinct()],
            StringSplitOptions.RemoveEmptyEntries);
        var name = string.Concat(words.Select(Capitalize));
        return $"{(name.Length == 0 ? Fallback : name)}{Extension}";
    }

    // Uppercase the first character and preserve the rest, so an already-PascalCase word survives intact.
    static string Capitalize(string word) =>
        word.Length == 1 ? word.ToUpperInvariant() : char.ToUpperInvariant(word[0]) + word[1..];
}

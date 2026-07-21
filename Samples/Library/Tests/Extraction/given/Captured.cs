// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Storage;

namespace Library.Tests.Extraction.given;

/// <summary>
/// Reads what the Prologue Extractor ended up storing. Nothing here asserts immediately — the extractor holds
/// observations open for its correlation window before deciding what belongs together, and the Receiver writes them
/// after that, so the only honest way to look is to keep looking until they turn up or time runs out.
/// </summary>
public static class Captured
{
    /// <summary>
    /// Reads the identifiers of the captures already stored, so a spec can tell what it caused from what was
    /// already there — the MongoDB container outlives a single run.
    /// </summary>
    /// <param name="store">The <see cref="ICaptureStore"/> to read from.</param>
    /// <param name="prologueId">The Prologue the captures belong to.</param>
    /// <returns>The identifiers of the captures stored so far.</returns>
    public static async Task<IReadOnlySet<Guid>> Identifiers(this ICaptureStore store, Guid prologueId)
    {
        var captures = await store.GetForPrologue(prologueId);
        return captures.Select(capture => capture.Id).ToHashSet();
    }

    /// <summary>
    /// Waits for captures that were not there before to satisfy what the spec is looking for.
    /// </summary>
    /// <param name="store">The <see cref="ICaptureStore"/> to read from.</param>
    /// <param name="prologueId">The Prologue the captures belong to.</param>
    /// <param name="alreadyStored">The identifiers that were stored before the spec acted.</param>
    /// <param name="settled">What the new captures have to satisfy.</param>
    /// <param name="patience">How long to keep looking.</param>
    /// <returns>The new captures, as they last looked.</returns>
    public static async Task<IReadOnlyList<Capture>> WaitForNew(
        this ICaptureStore store,
        Guid prologueId,
        IReadOnlySet<Guid> alreadyStored,
        Func<IReadOnlyList<Capture>, bool> settled,
        TimeSpan patience)
    {
        var deadline = DateTimeOffset.UtcNow + patience;
        IReadOnlyList<Capture> fresh;

        do
        {
            var captures = await store.GetForPrologue(prologueId);
            fresh = [.. captures.Where(capture => !alreadyStored.Contains(capture.Id))];

            if (settled(fresh))
            {
                return fresh;
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
        while (DateTimeOffset.UtcNow < deadline);

        return fresh;
    }

    /// <summary>
    /// Determines whether any of the captures carries a payload of the given kind.
    /// </summary>
    /// <typeparam name="TPayload">The <see cref="ObservationPayload"/> being looked for.</typeparam>
    /// <param name="captures">The captures to look through.</param>
    /// <returns>True when at least one capture carries such a payload.</returns>
    public static bool Carry<TPayload>(this IReadOnlyList<Capture> captures)
        where TPayload : ObservationPayload =>
        captures.Any(capture => capture.Entries.Any(entry => entry.Payload is TPayload));
}

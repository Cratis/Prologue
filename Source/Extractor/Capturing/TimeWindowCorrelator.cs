// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents an <see cref="ICorrelator"/> that correlates an HTTP command with the evidence it produced — the
/// database transactions and OpenTelemetry signals within a configurable time window after it, plus any telemetry
/// sharing the command's trace id. A command and its evidence become one capture. Trace-carrying telemetry with no
/// preceding command is grouped into one capture per trace; standalone database transactions and metrics each
/// become their own capture.
/// </summary>
/// <param name="options">The Prologue options carrying the correlation window and the Prologue captures belong to.</param>
public class TimeWindowCorrelator(IOptions<PrologueOptions> options) : ICorrelator
{
    readonly List<Observation> _pending = [];
    readonly Lock _sync = new();

    /// <inheritdoc/>
    public void Add(Observation observation)
    {
        lock (_sync)
        {
            _pending.Add(observation);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<Capture> Drain(DateTimeOffset upTo)
    {
        var window = options.Value.Correlation.Window;
        var settledCutoff = upTo - window;
        var captures = new List<Capture>();
        var claimed = new HashSet<Observation>(ReferenceEqualityComparer.Instance);

        lock (_sync)
        {
            foreach (var command in SettledCommands(settledCutoff))
            {
                var evidence = RelatedEvidence(command, window, claimed);
                claimed.Add(command);

                foreach (var observation in evidence)
                {
                    claimed.Add(observation);
                }

                captures.Add(NewCapture(command.Occurred, [command, .. evidence]));
            }

            // Trace-carrying telemetry with no correlated command, grouped into one capture per trace.
            foreach (var trace in SettledStandalone(settledCutoff, claimed, CarriesTrace).GroupBy(TraceIdOf, StringComparer.Ordinal))
            {
                var observations = trace.OrderBy(observation => observation.Occurred).ToList();

                foreach (var observation in observations)
                {
                    claimed.Add(observation);
                }

                // Without a trace id there is nothing tying the signals together — each stands alone.
                if (trace.Key.Length == 0)
                {
                    captures.AddRange(observations.Select(observation => NewCapture(observation.Occurred, [observation])));
                    continue;
                }

                captures.Add(NewCapture(observations[0].Occurred, observations));
            }

            // Database transactions and metrics carry no trace, so each settles into its own capture.
            foreach (var observation in SettledStandalone(settledCutoff, claimed, payload => !CarriesTrace(payload)))
            {
                claimed.Add(observation);
                captures.Add(NewCapture(observation.Occurred, [observation]));
            }

            _pending.RemoveAll(claimed.Contains);
        }

        return captures;
    }

    static string TraceIdOf(Observation observation) => observation.Payload switch
    {
        HttpCommandObserved command => command.TraceId,
        TelemetryObserved telemetry => telemetry.TraceId,
        LogObserved log => log.TraceId,
        _ => string.Empty,
    };

    // Evidence is anything a command may have produced, as opposed to the command itself.
    static bool IsEvidence(ObservationPayload payload) =>
        payload is DatabaseTransactionObserved or TelemetryObserved or MetricObserved or LogObserved;

    // Payloads carrying a trace id can be correlated by trace rather than by time alone.
    static bool CarriesTrace(ObservationPayload payload) => payload is TelemetryObserved or LogObserved;

    Capture NewCapture(DateTimeOffset occurred, IReadOnlyList<Observation> entries) =>
        new(Guid.NewGuid(), occurred, entries, options.Value.PrologueId);

    List<Observation> SettledCommands(DateTimeOffset settledCutoff) =>
        [.. _pending
            .Where(observation => observation.Payload is HttpCommandObserved && observation.Occurred <= settledCutoff)
            .OrderBy(observation => observation.Occurred)];

    List<Observation> RelatedEvidence(Observation command, TimeSpan window, HashSet<Observation> claimed)
    {
        var commandTraceId = TraceIdOf(command);

        return [.. _pending
            .Where(observation =>
                IsEvidence(observation.Payload) &&
                !claimed.Contains(observation) &&
                ((observation.Occurred >= command.Occurred && observation.Occurred <= command.Occurred + window) ||
                 (commandTraceId.Length > 0 && CarriesTrace(observation.Payload) && TraceIdOf(observation) == commandTraceId)))
            .OrderBy(observation => observation.Occurred)];
    }

    List<Observation> SettledStandalone(DateTimeOffset settledCutoff, HashSet<Observation> claimed, Func<ObservationPayload, bool> matches) =>
        [.. _pending
            .Where(observation =>
                IsEvidence(observation.Payload) &&
                matches(observation.Payload) &&
                !claimed.Contains(observation) &&
                observation.Occurred <= settledCutoff)
            .OrderBy(observation => observation.Occurred)];
}

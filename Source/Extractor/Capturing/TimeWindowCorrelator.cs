// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Options;

namespace Cratis.Prologue.Extractor.Capturing;

/// <summary>
/// Represents an <see cref="ICorrelator"/> that correlates an HTTP command with the evidence it produced — the
/// database transactions and OpenTelemetry spans within a configurable time window after it, plus any spans sharing
/// the command's trace id. A command and its evidence become one capture. Spans with no preceding command are
/// grouped into one capture per trace (the intent and events of a trace); standalone database transactions each
/// become their own capture.
/// </summary>
/// <param name="options">The Prologue options carrying the correlation window.</param>
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

                captures.Add(new Capture(Guid.NewGuid(), command.Occurred, [command, .. evidence]));
            }

            // Spans with no correlated command, grouped into one capture per trace.
            foreach (var trace in SettledStandaloneTelemetry(settledCutoff, claimed).GroupBy(observation => ((TelemetryObserved)observation.Payload).TraceId))
            {
                if (string.IsNullOrEmpty(trace.Key))
                {
                    foreach (var span in trace)
                    {
                        claimed.Add(span);
                        captures.Add(new Capture(Guid.NewGuid(), span.Occurred, [span]));
                    }

                    continue;
                }

                var spans = trace.OrderBy(observation => observation.Occurred).ToList();
                foreach (var span in spans)
                {
                    claimed.Add(span);
                }

                captures.Add(new Capture(Guid.NewGuid(), spans[0].Occurred, spans));
            }

            foreach (var transaction in SettledStandaloneTransactions(settledCutoff, claimed))
            {
                claimed.Add(transaction);
                captures.Add(new Capture(Guid.NewGuid(), transaction.Occurred, [transaction]));
            }

            _pending.RemoveAll(claimed.Contains);
        }

        return captures;
    }

    static string TraceIdOf(Observation observation) => observation.Payload switch
    {
        HttpCommandObserved command => command.TraceId,
        TelemetryObserved telemetry => telemetry.TraceId,
        _ => string.Empty,
    };

    List<Observation> SettledCommands(DateTimeOffset settledCutoff) =>
        [.. _pending
            .Where(observation => observation.Payload is HttpCommandObserved && observation.Occurred <= settledCutoff)
            .OrderBy(observation => observation.Occurred)];

    List<Observation> RelatedEvidence(Observation command, TimeSpan window, HashSet<Observation> claimed)
    {
        var commandTraceId = TraceIdOf(command);
        return [.. _pending
            .Where(observation =>
                observation.Payload is DatabaseTransactionObserved or TelemetryObserved &&
                !claimed.Contains(observation) &&
                ((observation.Occurred >= command.Occurred && observation.Occurred <= command.Occurred + window) ||
                 (commandTraceId.Length > 0 && observation.Payload is TelemetryObserved telemetry && telemetry.TraceId == commandTraceId)))
            .OrderBy(observation => observation.Occurred)];
    }

    List<Observation> SettledStandaloneTelemetry(DateTimeOffset settledCutoff, HashSet<Observation> claimed) =>
        [.. _pending
            .Where(observation =>
                observation.Payload is TelemetryObserved &&
                !claimed.Contains(observation) &&
                observation.Occurred <= settledCutoff)
            .OrderBy(observation => observation.Occurred)];

    List<Observation> SettledStandaloneTransactions(DateTimeOffset settledCutoff, HashSet<Observation> claimed) =>
        [.. _pending
            .Where(observation =>
                observation.Payload is DatabaseTransactionObserved &&
                !claimed.Contains(observation) &&
                observation.Occurred <= settledCutoff)
            .OrderBy(observation => observation.Occurred)];
}

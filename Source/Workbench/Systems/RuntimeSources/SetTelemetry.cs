// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Systems.RuntimeSources;

/// <summary>
/// Sets the OpenTelemetry services whose spans are captured for a system, and the span attributes whose values may be kept.
/// Every other attribute value is never captured.
/// </summary>
/// <param name="Prologue">The prologue.</param>
/// <param name="Id">The system.</param>
/// <param name="Services">The service names; empty stops telemetry capture for the system.</param>
/// <param name="AttributeKeys">The attribute keys whose values may be captured.</param>
[Command]
[ReadProtection(ReadProtectionMode.On)]
public record SetTelemetry(PrologueId Prologue, SystemId Id, IEnumerable<TelemetryServiceName> Services, IEnumerable<AttributeKey> AttributeKeys)
{
    /// <summary>
    /// Handles the command, refusing an unknown system and telemetry the system already has.
    /// </summary>
    /// <param name="prologue">The prologue.</param>
    /// <returns>The telemetry set, or a refusal.</returns>
    public Result<SystemTelemetrySet, ValidationResult> Handle(PrologueDefinition? prologue)
    {
        if (SystemRules.RefuseUnknown(prologue, Id) is { } unknown)
        {
            return unknown;
        }

        var services = SystemRules.Clean(Services.Select(service => service.Value)).Select(service => new TelemetryServiceName(service)).ToArray();
        var keys = SystemRules.Clean(AttributeKeys.Select(key => key.Value)).Select(key => new AttributeKey(key)).ToArray();
        var current = SystemRules.Find(prologue, Id)!;

        return current.TelemetryServices.SequenceEqual(services) && current.AttributeKeys.SequenceEqual(keys)
            ? ValidationResult.Error("The system already has this telemetry")
            : new SystemTelemetrySet(Id, services, keys);
    }
}

/// <summary>
/// The telemetry captured for a system was set.
/// </summary>
/// <param name="Id">The system.</param>
/// <param name="Services">The service names.</param>
/// <param name="AttributeKeys">The attribute keys whose values may be captured.</param>
[EventType]
public record SystemTelemetrySet(SystemId Id, IEnumerable<TelemetryServiceName> Services, IEnumerable<AttributeKey> AttributeKeys);

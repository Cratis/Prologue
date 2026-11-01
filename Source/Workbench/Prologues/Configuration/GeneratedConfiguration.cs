// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Workbench.Common;

namespace Cratis.Prologue.Workbench.Prologues.Configuration;

/// <summary>
/// The <c language="json">cratis-prologue.json</c> text that matches a prologue's definition, kept current as the definition changes.
/// </summary>
/// <param name="Id">The prologue.</param>
/// <param name="Json">The configuration file's text. Secrets appear by name only.</param>
[ReadModel]
public record GeneratedConfiguration(PrologueId Id, string Json)
{
    /// <summary>
    /// Observes the configuration for a prologue.
    /// </summary>
    /// <param name="id">The prologue.</param>
    /// <param name="readModels">Observes the stored prologues.</param>
    /// <returns>The configuration.</returns>
    public static ISubject<GeneratedConfiguration> ObserveGeneratedConfiguration(PrologueId id, IReadModels readModels) =>
        readModels.Observe<PrologueDefinition>(prologue => prologue.Id == id).FirstItem()
            .Mapped(prologue => new GeneratedConfiguration(prologue.Id, PrologueConfigurationGenerator.ToJson(prologue)));
}

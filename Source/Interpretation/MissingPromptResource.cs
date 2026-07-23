// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// The exception that is thrown when an embedded prompt resource is missing from the assembly — a packaging
/// defect, never a runtime condition.
/// </summary>
/// <param name="resourceName">The name of the missing embedded resource.</param>
public class MissingPromptResource(string resourceName)
    : Exception($"Embedded prompt resource '{resourceName}' was not found in the interpretation assembly.");

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Suggestions;

/// <summary>
/// A property a suggested event carries.
/// </summary>
/// <param name="Name">The property name.</param>
/// <param name="Type">The type the interpreter inferred, for example String or Guid.</param>
/// <param name="IsRequired">Whether the database requires a value.</param>
/// <param name="MaxLength">The longest value the database allows; zero when unbounded or unknown.</param>
public record SuggestedProperty(string Name, string Type, bool IsRequired, int MaxLength);

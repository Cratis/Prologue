// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Discovery.Runtime;

/// <summary>
/// How many captured commands ended in an HTTP status.
/// </summary>
/// <param name="Status">The HTTP status code.</param>
/// <param name="Count">The number of captures.</param>
public record StatusCount(int Status, int Count);

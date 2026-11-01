// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures;

namespace Cratis.Prologue.Workbench.Explore.Asking;

/// <summary>
/// An agent's answer, with the command shapes it cited that really exist.
/// </summary>
/// <param name="Text">The answer.</param>
/// <param name="Citations">The cited command shapes, each as the captures know it.</param>
public record ParsedAnswer(string Text, IEnumerable<CommandShape> Citations);

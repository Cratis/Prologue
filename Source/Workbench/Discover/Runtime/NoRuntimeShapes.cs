// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using Cratis.Prologue.Workbench.Prologues;

namespace Cratis.Prologue.Workbench.Discover.Runtime;

/// <summary>
/// The runtime evidence when nothing supplies any: no command shape was seen, so every capability is found only in code.
/// Replace it by registering another <see cref="IRuntimeShapes"/> that is fed from the captures.
/// </summary>
public class NoRuntimeShapes : IRuntimeShapes
{
    /// <inheritdoc/>
    public IObservable<RuntimeEvidence> Observe(PrologueId prologue) => Observable.Return(RuntimeEvidence.None);
}

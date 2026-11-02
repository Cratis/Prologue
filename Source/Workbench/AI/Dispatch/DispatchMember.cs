// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Providers;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// One provider of a pool as the dispatcher sees it when work arrives.
/// </summary>
/// <param name="Provider">The provider.</param>
/// <param name="Name">The provider's name.</param>
/// <param name="IsInside">Whether the provider runs inside the customer's network.</param>
/// <param name="Headroom">How much of the provider's allowance is left, from 0 to 1; 1 when Cratis.AI does not know.</param>
/// <param name="CanStart">Whether the provider can take work now.</param>
/// <param name="Problem">Why it cannot; empty when it can.</param>
public record DispatchMember(AIProviderId Provider, string Name, bool IsInside, double Headroom, bool CanStart, string Problem);

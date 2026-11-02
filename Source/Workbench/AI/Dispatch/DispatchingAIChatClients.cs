// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.AI.Conversations;
using Cratis.AI.LanguageModels;
using Microsoft.Extensions.AI;

namespace Cratis.Prologue.Workbench.AI.Dispatch;

/// <summary>
/// Cratis.AI's <see cref="IAIChatClients"/> with the Workbench's dispatch in front of it: every agent that draws from a pool is dispatched by the pool's
/// recorded order, fallback and ceiling. A client for an agent the caller defines itself is not a Workbench agent and goes straight to Cratis.AI.
/// </summary>
/// <param name="inner">Cratis.AI's own chat clients.</param>
/// <param name="plans">Plans each call.</param>
/// <param name="spend">Records what each answer cost.</param>
public class DispatchingAIChatClients(IAIChatClients inner, IPoolPlans plans, IPoolSpendLedger spend) : IAIChatClients
{
    /// <inheritdoc/>
    public IChatClient For(LanguageModelPurpose purpose) => new DispatchingChatClient(purpose, inner, plans, spend);

    /// <inheritdoc/>
    public IChatClient For(LanguageModelPurpose purpose, ChatAgentTarget target) => inner.For(purpose, target);
}

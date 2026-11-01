// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Captures.Recording;
using Cratis.Prologue.Workbench.Decisions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// Registers what records captures for the Workbench and the seam decision reasons are read through.
/// </summary>
public static class CaptureServiceCollectionExtensions
{
    /// <summary>
    /// Adds the capture recorder and, unless Discover has supplied its own, a source of decision reasons with no decision points.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddCaptures(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICaptureRecorder, CaptureRecorder>();
        builder.Services.TryAddSingleton<IDecisionReasons, NoDecisionReasons>();
        return builder;
    }
}

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Rebuild.Parity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench.Rebuild;

/// <summary>
/// Registers what records the parity observations the Extractor reports.
/// </summary>
public static class RebuildServiceCollectionExtensions
{
    /// <summary>
    /// Adds the parity recorder.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add to.</param>
    /// <returns>The builder for continuation.</returns>
    public static WebApplicationBuilder AddRebuild(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IParityRecorder, ParityRecorder>();
        return builder;
    }
}

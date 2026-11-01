// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Screenplay;
using Cratis.Screenplay.Printing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Prologue.Workbench.Model;

/// <summary>
/// Registers what the Model page needs.
/// </summary>
public static class ModelServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Screenplay generator the model's <c language="csharp">.play</c> is written with.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/>.</param>
    /// <returns>The same builder.</returns>
    public static WebApplicationBuilder AddModel(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IScreenplayPrinter, ScreenplayPrinter>();
        builder.Services.AddSingleton<IScreenplayGenerator, ScreenplayGenerator>();
        return builder;
    }
}

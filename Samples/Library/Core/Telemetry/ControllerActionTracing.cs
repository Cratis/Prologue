// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Library.Core.Telemetry;

/// <summary>
/// Wraps every controller action — the <c>GET</c>s that read and the <c>POST</c>s and <c>DELETE</c>s that change
/// state — in a span named after the action itself. The Prologue Extractor sees these alongside the HTTP command
/// it proxied and the database transaction it produced, correlated by trace id.
/// </summary>
public class ControllerActionTracing : IAsyncActionFilter
{
    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var descriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        var name = descriptor is null
            ? context.ActionDescriptor.DisplayName ?? "action"
            : $"{descriptor.ControllerName}.{descriptor.ActionName}";

        using var activity = LibraryTelemetry.ActivitySource.StartActivity(name, ActivityKind.Internal);
        activity?.SetTag("library.controller", descriptor?.ControllerName);
        activity?.SetTag("library.action", descriptor?.ActionName);
        activity?.SetTag("http.request.method", context.HttpContext.Request.Method);

        var executed = await next();

        if (executed.Exception is not null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, executed.Exception.Message);
            return;
        }

        activity?.SetTag("http.response.status_code", context.HttpContext.Response.StatusCode);
    }
}

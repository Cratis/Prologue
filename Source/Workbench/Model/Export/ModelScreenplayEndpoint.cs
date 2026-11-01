// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Mime;
using Cratis.Chronicle.ReadModels;
using Cratis.Prologue.Screenplay;
using Cratis.Prologue.Workbench.Events.Accepted;
using Cratis.Prologue.Workbench.Events.Suggestions;
using Cratis.Prologue.Workbench.Prologues;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace Cratis.Prologue.Workbench.Model.Export;

/// <summary>
/// Serves the Screenplay of a prologue's model as a file download.
/// </summary>
public static class ModelScreenplayEndpoint
{
    /// <summary>The path of the download, with the prologue in it.</summary>
    public const string Route = "/prologues/{prologueId:guid}/model.play";

    /// <summary>
    /// Maps <c language="csharp">GET /prologues/{id}/model.play</c>: the <c language="csharp">.play</c> text as <c language="csharp">text/plain</c>, an attachment that is never cached.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    /// <returns>The same builder.</returns>
    public static IEndpointRouteBuilder MapModelScreenplay(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Route, async (Guid prologueId, HttpResponse response, IReadModels readModels, IScreenplayGenerator generator) =>
        {
            response.Headers[HeaderNames.CacheControl] = "no-store";
            response.Headers[HeaderNames.ContentDisposition] = "attachment; filename=\"model.play\"";
            var id = new PrologueId(prologueId);
            var review = await readModels.GetInstanceById<EventReview>((EventSourceId)id);
            var screenplay = ModelScreenplays.From(AcceptedModels.From(id, review), generator);
            return Results.Text(screenplay.Play, MediaTypeNames.Text.Plain, System.Text.Encoding.UTF8);
        });
        return endpoints;
    }
}

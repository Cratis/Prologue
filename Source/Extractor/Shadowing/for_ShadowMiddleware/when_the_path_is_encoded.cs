// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Cratis.Prologue.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Prologue.Extractor.Shadowing.for_ShadowMiddleware;

public class when_the_path_is_encoded : Specification
{
    readonly List<ShadowJob> _queued = [];

    async Task Because()
    {
        var middleware = new ShadowMiddleware(
            http =>
            {
                http.Response.StatusCode = 201;
                return Task.CompletedTask;
            },
            new ShadowRouting([new ShadowRuleOptions { Shape = "POST /api/books/{id}/tags", Destination = "http://new-system:5000/" }]),
            new Queue(_queued),
            NullLogger<ShadowMiddleware>.Instance);

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.PathBase = new PathString("/legacy app");
        context.Request.Path = new PathString("/api/books/978-0 123/tags");
        context.Request.QueryString = new QueryString("?title=a%26b");
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
        context.Request.ContentLength = 2;
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);
    }

    [Fact] void should_keep_the_path_percent_encoded_as_it_was_sent() => _queued.Single().Path.ShouldEqual("/legacy%20app/api/books/978-0%20123/tags");
    [Fact] void should_keep_the_query_as_it_was_sent() => _queued.Single().Query.ShouldEqual("?title=a%26b");

    sealed class Queue(List<ShadowJob> jobs) : IShadowQueue
    {
        public bool TryEnqueue(ShadowJob job)
        {
            jobs.Add(job);
            return true;
        }
    }
}
#endif

// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Library.Core.Pages;

/// <summary>
/// Carries the Razor frontend's requests to the library API and turns every response — the ones that worked and
/// the ones the domain rejected — into an <see cref="ApiResult"/>. A rejection is an outcome the page renders,
/// never an exception it throws.
/// </summary>
/// <param name="httpClientFactory">The factory the named library API client comes from.</param>
public class ApiTransport(IHttpClientFactory httpClientFactory)
{
    static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Sends a request and reads the value the API returned.
    /// </summary>
    /// <typeparam name="TValue">The type of value the API returns.</typeparam>
    /// <param name="method">The <see cref="HttpMethod"/> to send with.</param>
    /// <param name="path">The path, relative to the API's base address.</param>
    /// <param name="body">The body to send, or <see langword="null"/> when there is none.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The value, or the problem the API reported.</returns>
    public async Task<ApiResult<TValue>> Read<TValue>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await SendRequest(method, path, body, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<TValue>(default, await ProblemFrom(response, cancellationToken));
            }

            return new ApiResult<TValue>(await response.Content.ReadFromJsonAsync<TValue>(_serializerOptions, cancellationToken), null);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            return new ApiResult<TValue>(default, Unreachable(exception));
        }
    }

    /// <summary>
    /// Sends a request whose response body is not needed.
    /// </summary>
    /// <param name="method">The <see cref="HttpMethod"/> to send with.</param>
    /// <param name="path">The path, relative to the API's base address.</param>
    /// <param name="body">The body to send, or <see langword="null"/> when there is none.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The outcome of the call.</returns>
    public async Task<ApiResult> Send(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await SendRequest(method, path, body, cancellationToken);

            return new ApiResult(response.IsSuccessStatusCode ? null : await ProblemFrom(response, cancellationToken));
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            return new ApiResult(Unreachable(exception));
        }
    }

    static bool IsTransportFailure(Exception exception) =>
        exception is HttpRequestException or OperationCanceledException or JsonException or NotSupportedException or InvalidOperationException;

    static ApiProblem Unreachable(Exception exception) =>
        new("The library API could not be reached", exception.Message);

    static async Task<ApiProblem> ProblemFrom(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var details = await ReadProblemDetails(response, cancellationToken);
        var title = details?.Title;

        return new ApiProblem(
            string.IsNullOrWhiteSpace(title) ? DefaultTitle(response.StatusCode) : title,
            details?.Detail ?? string.Empty);
    }

    static async Task<ProblemDetails?> ReadProblemDetails(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ProblemDetails>(_serializerOptions, cancellationToken);
        }
        catch (Exception exception) when (IsTransportFailure(exception))
        {
            // The response carried no problem document, so the status code is all there is left to report on.
            return null;
        }
    }

    static string DefaultTitle(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.NotFound => "Not found",
        HttpStatusCode.Conflict => "Conflict",
        HttpStatusCode.UnprocessableEntity => "Rejected",
        HttpStatusCode.BadRequest => "Bad request",
        _ => $"The library API returned {(int)statusCode}"
    };

    async Task<HttpResponseMessage> SendRequest(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: _serializerOptions);
        }

        return await httpClientFactory.CreateClient(LibraryApi.HttpClientName).SendAsync(request, cancellationToken);
    }
}

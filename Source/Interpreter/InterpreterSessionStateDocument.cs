// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Prologue.Interpretation;
using MongoDB.Bson.Serialization.Attributes;

namespace Cratis.Prologue.Interpreter;

/// <summary>
/// Represents the MongoDB document an interpreter session's state is persisted as — the
/// <see cref="InterpreterSessionState"/> serialized with System.Text.Json, keyed by the Prologue it belongs to.
/// Wrapping the JSON keeps the stored representation identical to how every other host serializes the state,
/// so a session persisted by the service resumes anywhere the state round-trips.
/// </summary>
public class InterpreterSessionStateDocument
{
    /// <summary>
    /// The name of the collection session state documents are stored in.
    /// </summary>
    public const string CollectionName = "interpreter-sessions";

    static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Gets or sets the identifier of the Prologue the session interprets captures for.
    /// </summary>
    [BsonId]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the JSON representation of the session's <see cref="InterpreterSessionState"/>; empty when the
    /// session has never checkpointed.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Creates the document for a session state.
    /// </summary>
    /// <param name="prologueId">The Prologue the session belongs to.</param>
    /// <param name="state">The <see cref="InterpreterSessionState"/> to persist.</param>
    /// <returns>The document to store.</returns>
    public static InterpreterSessionStateDocument For(Guid prologueId, InterpreterSessionState state) =>
        new() { Id = prologueId, State = JsonSerializer.Serialize(state, _serializerOptions) };

    /// <summary>
    /// Deserializes the session state held by the document.
    /// </summary>
    /// <returns>The <see cref="InterpreterSessionState"/>, or <see langword="null"/> when the document holds none.</returns>
    public InterpreterSessionState? ToState() =>
        State.Length == 0 ? null : JsonSerializer.Deserialize<InterpreterSessionState>(State, _serializerOptions);
}

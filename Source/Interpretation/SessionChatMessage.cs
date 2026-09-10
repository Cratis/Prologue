// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Interpretation;

/// <summary>
/// Represents one message in an interpreter session's language-model conversation. The transcript is data in the
/// session state — the language model is stateless, so any host resumes a session by replaying these messages.
/// </summary>
/// <param name="Role">The role of the message author — <c language="csharp">system</c>, <c language="csharp">user</c>, or <c language="csharp">assistant</c>.</param>
/// <param name="Text">The text of the message.</param>
public record SessionChatMessage(string Role, string Text);

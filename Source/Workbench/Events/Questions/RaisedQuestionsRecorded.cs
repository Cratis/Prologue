// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Events.Questions;

/// <summary>
/// An interpretation raised different questions of its own than the ones before. The questions replace the ones before; an answered question is
/// not raised again.
/// </summary>
/// <param name="Questions">Every question the interpretation raises.</param>
[EventType]
public record RaisedQuestionsRecorded(IEnumerable<RaisedQuestion> Questions);

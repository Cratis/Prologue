// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Workbench.Captures;

/// <summary>
/// A database transaction of a capture.
/// </summary>
/// <param name="Tables">The tables the transaction changed.</param>
/// <param name="OffsetMilliseconds">When the transaction was observed, relative to the first observation of the capture.</param>
public record RecordedTransaction(IEnumerable<RecordedTableChange> Tables, int OffsetMilliseconds);

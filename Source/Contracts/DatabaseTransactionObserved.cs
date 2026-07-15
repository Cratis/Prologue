// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Prologue.Contracts;

/// <summary>
/// Represents the metadata of a single database transaction that changed one or more tables.
/// </summary>
/// <param name="Engine">The database engine the transaction occurred in (for example <c>sqlserver</c> or <c>postgres</c>).</param>
/// <param name="Database">The name of the database the transaction occurred in.</param>
/// <param name="TransactionId">The engine-specific identifier of the transaction (commit LSN for SQL Server, transaction id for PostgreSQL).</param>
/// <param name="Tables">The per-table change metadata for the transaction.</param>
public record DatabaseTransactionObserved(
    string Engine,
    string Database,
    string TransactionId,
    IReadOnlyList<TableChange> Tables) : ObservationPayload;

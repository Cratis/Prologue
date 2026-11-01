// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Prologue.Workbench.Common;

namespace Cratis.Prologue.Workbench.Systems;

/// <summary>
/// A database whose transaction log is read for one system. Only the name of the secret holding the connection string is kept.
/// </summary>
/// <param name="Engine">The database engine.</param>
/// <param name="ConnectionString">The secret holding the connection string.</param>
/// <param name="Tables">The tables allowed for capture; empty allows them all.</param>
public record DatabaseSource(DatabaseEngine Engine, SecretName ConnectionString, IEnumerable<TableName> Tables);

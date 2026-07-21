// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Library.Core.Database;

/// <summary>
/// Brings the database up before the system starts serving: creates the schema and writes the seed data. The
/// sample owns a throwaway database per run, so creating the schema outright is honest here — a long-lived system
/// would use migrations instead.
/// </summary>
/// <remarks>
/// There is deliberately nothing about Prologue here. Enabling Change Data Capture and checking logical
/// replication are the Extractor's job — a system being captured should not have to carry setup code for the tool
/// capturing it, which is the whole point of being able to point Prologue at software that already exists.
/// </remarks>
/// <param name="serviceProvider">The service provider scopes are created from.</param>
/// <param name="options">The database options.</param>
/// <param name="logger">The logger.</param>
public class DatabaseInitializer(
    IServiceProvider serviceProvider,
    IOptions<DatabaseOptions> options,
    ILogger<DatabaseInitializer> logger) : IHostedService
{
    /// <inheritdoc/>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.InitializeOnStartup)
        {
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        var created = await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        DatabaseInitializerLog.SchemaReady(logger, options.Value.Provider.ToString(), created);

        if (options.Value.SeedOnStartup)
        {
            var seeder = scope.ServiceProvider.GetRequiredService<SeedData>();
            await seeder.Apply(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

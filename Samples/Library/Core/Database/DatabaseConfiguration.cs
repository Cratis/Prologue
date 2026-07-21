// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Library.Core.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Library.Core.Database;

/// <summary>
/// Wires up the library's database — the provider it runs on, its schema, and its seed data. Nothing here knows
/// anything about Prologue; making the database observable is the Extractor's job.
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Registers the <see cref="LibraryDbContext"/> against the configured provider, along with the startup schema
    /// initializer and seed data.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to add registrations to.</param>
    /// <returns>The <paramref name="builder"/> for chaining.</returns>
    public static IHostApplicationBuilder AddLibraryDatabase(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));

        var options = builder.Configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>() ?? new DatabaseOptions();
        var connectionString = builder.Configuration.GetConnectionString(DatabaseOptions.ConnectionStringName)
            ?? throw new LibraryDatabaseNotConfigured(DatabaseOptions.ConnectionStringName);

        builder.Services.AddDbContext<LibraryDbContext>(context => Use(context, options.Provider, connectionString));

        builder.Services.AddScoped<SeedData>();
        builder.Services.AddHostedService<DatabaseInitializer>();

        return builder;
    }

    static void Use(DbContextOptionsBuilder context, DatabaseProvider provider, string connectionString)
    {
        switch (provider)
        {
            case DatabaseProvider.SqlServer:
                context.UseSqlServer(connectionString);
                break;

            default:
                // Long-lived PostgreSQL systems overwhelmingly use snake_case; matching that makes the captured
                // schema look like the real thing rather than a .NET model dumped into Postgres.
                context.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
                break;
        }
    }
}

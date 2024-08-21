// -----------------------------------------------------------------------
// <copyright file="PostgresBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// <see cref="Postgres"/> extensions.
/// </summary>
public static class PostgresBuilderExtensions
{
    /// <summary>
    /// Adds a health check to the PostgreSQL server resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <returns>A resource builder with the health check annotation added.</returns>
    public static IResourceBuilder<PostgresServerResource> WithHealthCheck(this IResourceBuilder<PostgresServerResource> builder) => WithHealthCheckCore(builder);

    /// <summary>
    /// Adds a health check to the PostgreSQL database resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <returns>A resource builder with the health check annotation added.</returns>
    public static IResourceBuilder<PostgresDatabaseResource> WithHealthCheck(this IResourceBuilder<PostgresDatabaseResource> builder) => WithHealthCheckCore(builder);

    /// <summary>
    /// Adds a database resource with a sanitised name, from the database name.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="databaseName">The database name.</param>
    /// <returns>The resource builder for the database resource.</returns>
    public static IResourceBuilder<PostgresDatabaseResource> AddDatabaseWithSanitisedName(this IResourceBuilder<PostgresServerResource> builder, string databaseName)
    {
        return builder.AddDatabase(Sanitize(databaseName), databaseName);

        static string Sanitize(string input)
        {
            var values = input.ToCharArray();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (!char.IsAsciiLetterOrDigit(value) && value is not '-')
                {
                    values[i] = '-';
                }
            }

            return new string(values);
        }
    }

    private static IResourceBuilder<T> WithHealthCheckCore<T>(IResourceBuilder<T> builder)
        where T : IResourceWithConnectionString => builder.WithAnnotation(HealthCheckAnnotation.Create(builder.Resource, connectionString => new HealthChecks.NpgSql.NpgSqlHealthCheck(new HealthChecks.NpgSql.NpgSqlHealthCheckOptions(connectionString))));
}
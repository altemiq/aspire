// -----------------------------------------------------------------------
// <copyright file="PostgresBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using System.Runtime.CompilerServices;

public static class PostgresBuilderExtensions
{
    public static IResourceBuilder<T> WithTle<T>(this IResourceBuilder<T> builder, string branch = "main")
        where T : PostgresServerResource
    {
        // get the versions
        var image = "library/postgres";
        var tag = "17.2";
        var postgresVersion = "-17";
        if (builder.Resource.TryGetLastAnnotation<ContainerImageAnnotation>(out var imageAnnotation))
        {
            image = imageAnnotation.Image;
            tag = imageAnnotation.Tag ?? "latest";
            postgresVersion = double.TryParse(GetPrefix(tag), System.Globalization.CultureInfo.InvariantCulture, out var version)
                ? $"-{Math.Truncate(version).ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                : string.Empty;

            static string GetPrefix(string tag)
            {
                var index = tag.IndexOf('-', StringComparison.Ordinal);
                return index > 0 ? tag[..index] : tag;
            }
        }

        var temp = Path.Combine(Path.GetTempPath(), "Tle.Dockerfile");

        var stream = typeof(PgAdminTheme).Assembly.GetManifestResourceStream(typeof(PgAdminTheme), "Tle.Dockerfile")
            ?? throw new InvalidOperationException();
        using (stream)
        {
            using (var fileStream = new FileStream(temp, FileMode.Create))
            {
                stream.CopyTo(fileStream);
            }
        }

        builder.WithDockerfile(".", temp)
            .WithImage($"{image}/tle")
            .WithImageTag(tag)
            .WithBuildArg("IMAGE", image)
            .WithBuildArg("TAG", tag)
            .WithBuildArg("POSTGRES_VERSION", postgresVersion)
            .WithBuildArg("TLE_BRANCH", branch)
            .WithArgs("postgres", "-c", "shared_preload_libraries=pg_tle");
            //.WithContainerRuntimeArgs("postgres", "-c", "shared_preload_libraries=pg_tle");
            // .WithContainerFiles(
            //     "/docker-entrypoint-initdb.d",
            //     [
            //         new ContainerFile { Name = "01-install-pg_tle.sh", Contents = "[ -d /extension ] && psql -f /extension/*.sql" }, //, Mode = UnixFileMode.OtherExecute | UnixFileMode.GroupExecute | UnixFileMode.UserExecute },
            //         new ContainerFile
            //         {
            //             Name = "02-activate-pg_tle.sql",
            //             Contents = """
            //                        ALTER SYSTEM SET shared_preload_libraries TO 'pg_tle';
            //                        create extension pg_tle;
            //                        grant pgtle_admin to postgres;
            //                        """,
            //         },
            //     ]);

        return builder;
    }
}
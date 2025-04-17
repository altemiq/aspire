// -----------------------------------------------------------------------
// <copyright file="PgAdminBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

/// <summary>
/// The <see cref="Postgres.PgAdminContainerResource"/> extensions.
/// </summary>
public static class PgAdminBuilderExtensions
{
    /// <summary>
    /// Adds the theme preference to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="theme">The theme.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<Postgres.PgAdminContainerResource> WithTheme(this IResourceBuilder<Postgres.PgAdminContainerResource> builder, PgAdminTheme theme) => builder.WithPreference("misc:user_interface:theme", theme.ToString().ToLowerInvariant());

    /// <summary>
    /// Adds the preference to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="key">The preference key.</param>
    /// <param name="value">The preference value.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<Postgres.PgAdminContainerResource> WithPreference(this IResourceBuilder<Postgres.PgAdminContainerResource> builder, string key, object? value) => builder.WithPreferences(new Dictionary<string, object?>(StringComparer.Ordinal) { { key, value } });

    /// <summary>
    /// Adds the preferences to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="preferences">The preferences.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<Postgres.PgAdminContainerResource> WithPreferences(this IResourceBuilder<Postgres.PgAdminContainerResource> builder, IDictionary<string, object?> preferences)
    {
        _ = builder.EnsureContainerFiles();
        foreach (var kvp in preferences)
        {
            builder.WithAnnotation(new PgAdminPreferenceAnnotation(kvp.Key, kvp.Value));
        }

        return builder;
    }

    private static IResourceBuilder<T> EnsureContainerFiles<T>(this IResourceBuilder<T> builder)
        where T : ContainerResource
    {
        if (!builder.Resource.HasAnnotationOfType<PgAdminAnnotation>())
        {
            builder.WithContainerFiles(
                "/pgadmin4",
                (_, _) =>
                {
                    IEnumerable<ContainerFileSystemItem> items = [];
                    if (builder.Resource.TryGetAnnotationsOfType<PgAdminPreferenceAnnotation>(out var annotations))
                    {
                        var preferences = annotations.Select(a => $"\"{a.Key}\":\"{GetValue(a.Value)}\"");
                        items =
                        [
                            new ContainerFile { Name = "preferences.json", Contents = $$"""{"preferences":{{{string.Join(',', preferences)}}}""" },
                        ];

                        static string GetValue(object? value)
                        {
                            return GetValueCore(value) ?? "null";

                            static string? GetValueCore(object? value)
                            {
                                return value switch
                                {
                                    string s => $"\"{s}\"",
                                    not null => value.ToString(),
                                    _ => null,
                                };
                            }
                        }
                    }

                    return Task.FromResult(items);
                });

            builder.WithAnnotation(new PgAdminAnnotation());
        }

        return builder;
    }

    private sealed record PgAdminPreferenceAnnotation(string Key, object? Value) : IResourceAnnotation;

    private sealed class PgAdminAnnotation : IResourceAnnotation;
}
// -----------------------------------------------------------------------
// <copyright file="PgAdminBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

extern alias core;

/// <summary>
/// The <see cref="core::Aspire.Hosting.Postgres.PgAdminContainerResource"/> extensions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
public static class PgAdminBuilderExtensions
{
    /// <summary>
    /// Adds the theme preference to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="theme">The theme.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> WithTheme(this IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> builder, PgAdminTheme theme) => builder
        .WithPreference("misc:themes:theme", theme.ToString().ToLowerInvariant())
        .WithPreference("misc:user_interface:theme", theme.ToString().ToLowerInvariant());

    /// <summary>
    /// Adds the preference to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="key">The preference key.</param>
    /// <param name="value">The preference value.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> WithPreference(this IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> builder, string key, object? value) => builder.WithPreferences(new Dictionary<string, object?>(StringComparer.Ordinal) { { key, value } });

    /// <summary>
    /// Adds the preferences to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="preferences">The preferences.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> WithPreferences(this IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> builder, IDictionary<string, object?> preferences)
    {
        _ = builder.EnsureContainerFiles();
        foreach (var kvp in preferences)
        {
            builder.WithAnnotation(new PgAdminPreferenceAnnotation(kvp.Key, kvp.Value));
        }

        return builder;
    }

    /// <summary>
    /// Adds the configuration to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> WithConfiguration(this IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> builder, string key, object? value) => builder.WithConfiguration(new Dictionary<string, object?>(StringComparer.Ordinal) { { key, value } });

    /// <summary>
    /// Adds the configuration to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="key">The configuration key.</param>
    /// <param name="values">The configuration values.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> WithConfiguration(this IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> builder, string key, IEnumerable<object?> values) => builder.WithConfiguration(new Dictionary<string, object?>(StringComparer.Ordinal) { { key, values } });

    /// <summary>
    /// Adds the configuration to the container.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> WithConfiguration(this IResourceBuilder<core::Aspire.Hosting.Postgres.PgAdminContainerResource> builder, IDictionary<string, object?> configuration)
    {
        _ = builder.EnsureContainerFiles();
        foreach (var kvp in configuration)
        {
            builder.WithAnnotation(new PgAdminConfigAnnotation(kvp.Key, kvp.Value));
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
                    ICollection<ContainerFileSystemItem> items = [];
                    if (builder.Resource.TryGetAnnotationsOfType<PgAdminPreferenceAnnotation>(out var preferences))
                    {
                        items.Add(
                            new ContainerFile
                            {
                                Name = "preferences.json",
                                Contents = $$"""
                                             {
                                               "preferences":
                                               {
                                                 {{string.Join("    ," + Environment.NewLine, preferences.Select(a => $"\"{a.Key}\":{GetValue(a.Value)}"))}}
                                               }
                                             }
                                             """,
                            });

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

                    if (builder.Resource.TryGetAnnotationsOfType<PgAdminConfigAnnotation>(out var configuration))
                    {
                        items.Add(
                            new ContainerFile
                            {
                                Name = "config_local.py",
                                Contents = string.Join(Environment.NewLine, configuration.Select(a => $"{a.Key} = {GetValue(a.Value)}")),
                            });

                        static string GetValue(object? value)
                        {
                            return GetValueCore(value) ?? "null";

                            static string? GetValueCore(object? value)
                            {
                                return value switch
                                {
                                    string s => $"'{s}'",
                                    true => bool.TrueString,
                                    false => bool.FalseString,
                                    System.Collections.IEnumerable c => $"[{string.Join(", ", c.Cast<object?>().Select(GetValueCore))}]",
                                    not null => value.ToString(),
                                    _ => null,
                                };
                            }
                        }
                    }

                    return Task.FromResult<IEnumerable<ContainerFileSystemItem>>(items);
                });

            builder.WithAnnotation(new PgAdminAnnotation());
        }

        return builder;
    }

    private sealed record PgAdminPreferenceAnnotation(string Key, object? Value) : IResourceAnnotation;

    private sealed record PgAdminConfigAnnotation(string Key, object? Value) : IResourceAnnotation;

    private sealed class PgAdminAnnotation : IResourceAnnotation;
}
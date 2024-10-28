// -----------------------------------------------------------------------
// <copyright file="VolumeNameGenerator.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.Utils;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// The volume name generator.
/// </summary>
internal static class VolumeNameGenerator
{
    /// <summary>
    /// Creates a volume name with the form <c>$"{applicationName}-{resourceName}-{suffix}</c>, e.g. <c>"myapplication-postgres-data"</c>.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="suffix">The suffix.</param>
    /// <returns>The volume name.</returns>
    /// <remarks>If the application name contains chars that are invalid for a volume name, the prefix <c>"volume-"</c> will be used instead.</remarks>
    public static string CreateVolumeName<T>(IResourceBuilder<T> builder, string suffix)
        where T : IResource
    {
        if (!HasOnlyValidChars(suffix))
        {
            throw new ArgumentException($"The suffix '{suffix}' contains invalid characters. Only [a-zA-Z0-9_.-] are allowed.", nameof(suffix));
        }

        // Creates a volume name with the form < c > $"{applicationName}-{sha256 of apphost path}-{resourceName}-{suffix}</c>, e.g. <c>"myapplication-a345f2451-postgres-data"</c>.
        // Create volume name like "{Sanitize(appname).Lower()}-{sha256.Lower()}-postgres-data"

        // Compute a short hash of the content root path to differentiate between multiple AppHost projects with similar volume names
        var safeApplicationName = Sanitize(builder.ApplicationBuilder.Environment.ApplicationName).ToLowerInvariant();
        var applicationHash = builder.ApplicationBuilder.Configuration["AppHost:Sha256"]![..10].ToLowerInvariant();
        var resourceName = builder.Resource.Name;
        return $"{safeApplicationName}-{applicationHash}-{resourceName}-{suffix}";
    }

    /// <summary>
    /// Sanitizes the name.
    /// </summary>
    /// <param name="name">The name to sanitize.</param>
    /// <returns>The sanitized name.</returns>
    public static string Sanitize(string name) =>
        string.Create(name.Length, name, static (s, name) =>
        {
            // According to the error message from docker CLI, volume names must be of form "[a-zA-Z0-9][a-zA-Z0-9_.-]"
            var nameSpan = name.AsSpan();
            for (var i = 0; i < nameSpan.Length; i++)
            {
                var c = nameSpan[i];

                s[i] = IsValidChar(i, c) ? c : '_';
            }
        });

    private static bool HasOnlyValidChars(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (!IsValidChar(i, value[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidChar(int i, char c)
    {
        if (i == 0 && !(char.IsAsciiLetter(c) || char.IsNumber(c)))
        {
            // First char must be a letter or number
            return false;
        }

        if (!(char.IsAsciiLetter(c) || char.IsNumber(c) || c == '_' || c == '.' || c == '-'))
        {
            // Subsequent chars must be a letter, number, underscore, period, or hyphen
            return false;
        }

        return true;
    }
}
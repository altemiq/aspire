// -----------------------------------------------------------------------
// <copyright file="MiniStackService.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

/// <summary>
/// The <see cref="Aspire.Hosting.MiniStack"/> service.
/// </summary>
/// <param name="name">The name.</param>
/// <param name="value">The value.</param>
public sealed class MiniStackService(string name, string value)
{
    /// <summary>
    /// Gets the value.
    /// </summary>
    internal string Value => value;

    /// <summary>
    /// The or operator.
    /// </summary>
    /// <param name="first">The first value.</param>
    /// <param name="second">The second value.</param>
    /// <returns>The combined value.</returns>
    public static MiniStackServices operator |(MiniStackService first, MiniStackService second) => new(first, second);

    /// <inheritdoc />
    public override string ToString() => name;
}
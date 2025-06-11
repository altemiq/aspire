// -----------------------------------------------------------------------
// <copyright file="PostGisVersion.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

/// <summary>
/// The PostGIS version.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "We are not allowed dots in enum names")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InconsistentNaming", Justification = "We are not allowed dots in enum names")]
public enum PostGisVersion
{
    /// <summary>
    /// Version 2.5.
    /// </summary>
    V2_5,

    /// <summary>
    /// Version 3.0.
    /// </summary>
    V3_0,

    /// <summary>
    /// Version 3.1.
    /// </summary>
    V3_1,

    /// <summary>
    /// Version 3.2.
    /// </summary>
    V3_2,

    /// <summary>
    /// Version 3.3.
    /// </summary>
    V3_3,

    /// <summary>
    /// Version 3.4.
    /// </summary>
    V3_4,

    /// <summary>
    /// Version 3.5.
    /// </summary>
    V3_5,
}
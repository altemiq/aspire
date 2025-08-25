// -----------------------------------------------------------------------
// <copyright file="ContainerLinesCallbackAnnotation.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// The <c>Containerfile</c> contents callback.
/// </summary>
/// <param name="Callback">The callback.</param>
public sealed record ContainerLinesCallbackAnnotation(Func<IEnumerable<string>, IEnumerable<string>> Callback) : IResourceAnnotation;
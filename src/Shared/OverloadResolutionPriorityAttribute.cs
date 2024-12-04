// -----------------------------------------------------------------------
// <copyright file="OverloadResolutionPriorityAttribute.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace System.Runtime.CompilerServices;

/// <summary>
/// Specifies the priority of a member in overload resolution. When unspecified, the default priority is 0.
/// </summary>
/// <param name="priority">The priority of the attributed member. Higher numbers are prioritized, lower numbers are deprioritized. 0 is the default if no attribute is present.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class OverloadResolutionPriorityAttribute(int priority) : Attribute
{
    /// <summary>
    /// Gets the priority of the member. Higher numbers are prioritized, lower numbers are deprioritized.
    /// </summary>
    public int Priority => priority;
}
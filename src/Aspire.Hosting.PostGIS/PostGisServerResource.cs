// -----------------------------------------------------------------------
// <copyright file="PostGisServerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a PostGIS container.
/// </summary>
public class PostGisServerResource(string name, ParameterResource? userName, ParameterResource password) : ApplicationModel.PostgresServerResource(name, userName, password)
{
    /// <summary>
    /// The primary end point name.
    /// </summary>
    internal const string PrimaryEndpointName = "tcp";

    private static readonly System.Reflection.PropertyInfo UserNameReferenceProperty = Utilties.GetUserNameReferenceProperty();

    /// <summary>
    /// Gets the user name reference.
    /// </summary>
    internal ReferenceExpression UserNameReference => UserNameReferenceProperty.GetValue(this) as ReferenceExpression ?? throw new InvalidOperationException();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "This is required.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Required")]
    private static class Utilties
    {
        public static System.Reflection.PropertyInfo GetUserNameReferenceProperty() => typeof(ApplicationModel.PostgresServerResource).GetProperty(nameof(UserNameReference), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ?? throw new InvalidOperationException();
    }
}
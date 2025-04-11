// -----------------------------------------------------------------------
// <copyright file="MinIOServerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a MinIO container.
/// </summary>
public class MinIOServerResource : ContainerResource, IResourceWithServiceDiscovery
{
    private const string DefaultRootUser = "minio";

    /// <summary>
    /// Initialises a new instance of the <see cref="MinIOServerResource"/> class.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <param name="userName">A parameter that contains the MinIO server username, or <see langword="null"/> to use a default value.</param>
    /// <param name="password">A parameter that contains the MinIO server password.</param>
    /// <param name="region">The region.</param>
    public MinIOServerResource(string name, ParameterResource? userName, ParameterResource password, string? region)
        : base(name)
    {
        ArgumentNullException.ThrowIfNull(password);

        this.UserNameParameter = userName;
        this.PasswordParameter = password;
        this.Region = region;
    }

    /// <summary>
    /// Gets the region.
    /// </summary>
    public string? Region { get; }

    /// <summary>
    /// Gets the parameter that contains the MinIO server username.
    /// </summary>
    public ParameterResource? UserNameParameter { get; }

    /// <summary>
    /// Gets the parameter that contains the MinIO password.
    /// </summary>
    public ParameterResource PasswordParameter { get; }

    /// <summary>
    /// Gets the username reference.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1214:Unnecessary interpolated string", Justification = "This is required to turn it into an interpolated string handler")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This suppression is required.")]
    internal ReferenceExpression UserNameReference =>
        this.UserNameParameter is { } userNameParameter ?
            ReferenceExpression.Create($"{userNameParameter}") :
            ReferenceExpression.Create($"{DefaultRootUser}");
}
// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

_ = builder.AddContainer("mycontainer", "myimage")
    .WithContainerfile("Context");

_ = builder
    .AddPostgres16("database")
    .WithDotnet()
    .WithImageBuildPolicy(ImageBuildPolicy.Default);

_ = builder.AddContainerBuildEnvironment("container-build");

await builder.Build().RunAsync(CancellationToken.None).ConfigureAwait(false);

/// <summary>
/// The console logger.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0047:Declare types in namespaces", Justification = "This is only referenced above.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Checked")]
internal sealed class ConsoleLogger : Microsoft.Extensions.Logging.ILogger
{
    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => default;

    /// <inheritdoc/>
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

    /// <inheritdoc/>
    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel is Microsoft.Extensions.Logging.LogLevel.Error)
        {
            Console.Error.WriteLine(formatter(state, exception));
        }
        else
        {
            Console.Out.WriteLine(formatter(state, exception));
        }
    }
}
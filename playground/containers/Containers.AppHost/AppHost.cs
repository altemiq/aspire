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

await builder.Build().RunBuildAsync(new ConsoleLogger(), CancellationToken.None).ConfigureAwait(false);

/// <content>
/// Program class.
/// </content>
[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1205:Partial elements should declare access", Justification = "Valid")]
static partial class Program
{
    private sealed class ConsoleLogger : Microsoft.Extensions.Logging.ILogger
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
}
// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var db1 = builder
    .AddPostGis("db1")
    .WithDataVolume();

db1.WithPgAdmin(container =>
    {
        container
            .WaitFor(db1)
            .WithTheme(PgAdminTheme.System)
            .WithImageTag("9")
            .WithConfiguration("UPGRADE_CHECK_ENABLED", value: false)
            .WithImagePullPolicy(ImagePullPolicy.Always);

        if (Environment.GetEnvironmentVariable("OLLAMA_HOST") is { } ollamaHost)
        {
            container
                .WithConfiguration("LLM_ENABLED", value: true)
                .WithConfiguration("DEFAULT_LLM_PROVIDER", "ollama")
                .WithConfiguration("OLLAMA_API_URL", $"{Uri.UriSchemeHttp}://{ollamaHost}:11434");
        }
        else
        {
            container.WithConfiguration("LLM_ENABLED", value: false);
        }
    });

var database = db1.AddDatabase("db1-database");

_ = builder.AddProject<Projects.PostGis_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database);

await builder.Build().RunAsync().ConfigureAwait(false);
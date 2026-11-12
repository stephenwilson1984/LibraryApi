using Serilog;

// Set up serilog in a two-step process. First, we configure basic logging to be able to log errors during ASP.NET Core startup.
// Later we read log settings from appsettings.json. Read more at https://github.com/serilog/serilog-aspnetcore#two-stage-initialization.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Library Api web host...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, config) =>
    {
        config.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
    });

    RegisterServices(builder);

    var app = builder.Build();

    ConfigureApplication(app);

    await app.RunAsync();

    static void RegisterServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
    }

    static void ConfigureApplication(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
    }
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly.");
    return 1;
}
finally
{
    Log.Information("Shut down complete.");
    await Log.CloseAndFlushAsync();
}

return 0;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using MilkingSystem.Core.Configuration;
using MilkingSystem.Core.Notifications;
using MilkingSystem.Core.Repositories;
using MilkingSystem.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.Configure<MilkingSettings>(
    builder.Configuration.GetSection(nameof(MilkingSettings)));

// Configure Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

    // Repositories - SingleInstance since each is stateless and opens a fresh connection per call
    containerBuilder.Register(_ => new AnimalRepository(connectionString))
        .As<IAnimalRepository>()
        .SingleInstance();

    containerBuilder.Register(_ => new RobotRepository(connectionString))
        .As<IRobotRepository>()
        .SingleInstance();

    containerBuilder.Register(_ => new MilkingEventRepository(connectionString))
        .As<IMilkingEventRepository>()
        .SingleInstance();

    containerBuilder.Register(_ => new WeightMeasurementRepository(connectionString))
        .As<IWeightMeasurementRepository>()
        .SingleInstance();

    containerBuilder.RegisterType<InMemoryRobotNotifier>()
        .As<IRobotNotifier>()
        .SingleInstance();

    // MilkingService is SingleInstance because it owns the per-animal SemaphoreSlim dictionary
    // that must be shared across all concurrent requests.
    containerBuilder.RegisterType<MilkingService>()
        .As<IMilkingService>()
        .SingleInstance();

    containerBuilder.RegisterType<WeightService>()
        .As<IWeightService>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<AnimalService>()
        .As<IAnimalService>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<RobotService>()
        .As<IRobotService>()
        .InstancePerLifetimeScope();
});

var app = builder.Build();

// Register an application-lifetime audit subscriber.
// Any component that needs to react to milking events can subscribe here.
// The subscription lives for the lifetime of the app — no need to dispose it.
var notifier = app.Services.GetRequiredService<IRobotNotifier>();
notifier.Subscribe(notification =>
    app.Logger.LogInformation(
        "Milking completed — animal {AnimalId} ({IdentificationNumber}) by robot {RobotId} at {Timestamp:u}",
        notification.AnimalId,
        notification.AnimalIdentificationNumber,
        notification.RobotId,
        notification.Timestamp));

// Configure the HTTP request pipeline.
// Must be first so it wraps all downstream middleware.
app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

    context.RequestServices
        .GetRequiredService<ILogger<Program>>()
        .LogError(exceptionFeature?.Error, "Unhandled exception");

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/problem+json";

    await context.Response.WriteAsJsonAsync(new
    {
        title = "An unexpected error occurred.",
        status = StatusCodes.Status500InternalServerError
    });
}));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }

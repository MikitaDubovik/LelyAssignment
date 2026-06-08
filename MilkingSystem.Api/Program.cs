using Autofac;
using Autofac.Extensions.DependencyInjection;
using MilkingSystem.Core.Configuration;
using MilkingSystem.Core.Notifications;
using MilkingSystem.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<MilkingSettings>(
    builder.Configuration.GetSection(nameof(MilkingSettings)));

// Configure Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Register DataService
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    containerBuilder.Register(c => new DataService(connectionString!))
        .AsSelf()
        .SingleInstance();

    containerBuilder.RegisterType<InMemoryRobotNotifier>()
        .As<IRobotNotifier>()
        .SingleInstance();

    containerBuilder.RegisterType<MilkingService>()
        .As<IMilkingService>()
        .InstancePerLifetimeScope();

    containerBuilder.RegisterType<WeightService>()
        .As<IWeightService>()
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
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }

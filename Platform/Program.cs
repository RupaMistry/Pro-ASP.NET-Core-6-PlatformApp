using Platform;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var myNewLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RupaPipeLine");

myNewLogger.LogDebug($"Pipleine configuration started");

app.MapGet("population/{city?}", Population.Endpoint);


myNewLogger.LogDebug($"Pipleine configuration ended");

app.Run();


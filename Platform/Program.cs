using Platform;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var myNewLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RupaPipeLine");

myNewLogger.LogDebug($"Pipleine configuration started");

app.MapGet("population/{city?}", Population.Endpoint);

app.MapGet("/", async (HttpContext context, IConfiguration configuration, IWebHostEnvironment env) =>
{
    var defaultSetting = configuration["Logging:LogLevel:Default"];
    await context.Response.WriteAsync($"The default settings is: {defaultSetting}");

    // You can access the Environment value either by using IConfiguration or by directly through IWebHostEnvironment.
    //var envConfig = configuration["ASPNETCORE_ENVIRONMENT"];
    //await context.Response.WriteAsync($"\nEnvironment settings is: {envConfig}");
    await context.Response.WriteAsync($"\nEnvironment settings is: {env.EnvironmentName}");

    var userSecretKey = configuration["WebService:Id"];
    var userSecretValue = configuration["WebService:Key"];

    await context.Response.WriteAsync($"\nnThe secret ID is: {userSecretKey}\nThe secret key is: {userSecretValue}");
});


myNewLogger.LogDebug($"Pipleine configuration ended");

app.Run();


using Platform;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Setting the status code used in the redirection response and the port to which the client is redirected.
builder.Services.AddHttpsRedirection(opts =>
{
    opts.HttpsPort = 5500;
    opts.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
});

// Enabling HSTS(HTTP Strict Transport Security)
builder.Services.AddHsts(opts =>
{
    opts.MaxAge = TimeSpan.FromDays(1);
    opts.IncludeSubDomains = true;
});

// The options pattern is used to configure a CookiePolicyOptions object, which sets the overall policy for cookies in the application
builder.Services.Configure<CookiePolicyOptions>(opts =>
{
    opts.CheckConsentNeeded = context => true;
});

// AddHttpLogging() selects the fields and headers that are included in the logging message
builder.Services.AddHttpLogging(opts =>
{
    opts.LoggingFields =
      HttpLoggingFields.RequestMethod |
      HttpLoggingFields.RequestPath |
      HttpLoggingFields.ResponseStatusCode;
});

// Configuring the Session Service and Middleware

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromMinutes(30);
    opts.Cookie.IsEssential = true;
});


var app = builder.Build();

// HSTS is disabled during development and enabled only in production
if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// This middleware will enforce the cookie policy and is added to the request pipeline.
app.UseCookiePolicy();

app.UseMiddleware<Platform.ConsentMiddleware>();

// This middleware is used to generate log messages that describe the HTTP requests received by an application and the responses it produces.
app.UseHttpLogging();

// This middleware component is used to handle requests for static content 
var env = app.Environment;
app.UseStaticFiles(new StaticFileOptions()
{
    //FileProvider property is used to select a different location for static content
    FileProvider = new PhysicalFileProvider($"{env.ContentRootPath}/staticfiles"),
    //RequestPath property is used to specify a URL prefix that denotes requests for static context.
    RequestPath = "/files"
});

//var myNewLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("RupaPipeLine");

//myNewLogger.LogDebug($"Pipleine configuration started");

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

//myNewLogger.LogDebug($"Pipleine configuration ended");


app.MapGet("/cookie", async context =>
{
    //Cookies are accessed through the HttpRequest.Cookies property, where the name of the cookie is used as the key
    int counter1 = int.Parse(context.Request.Cookies["counter1"] ?? "0") + 1;

    //Cookies are set through the HttpResponse.Cookies property, and the Append method creates or replaces a cookie in the response
    context.Response.Cookies.Append("counter1", counter1.ToString(), new CookieOptions
    {
        MaxAge = TimeSpan.FromMinutes(30),
        IsEssential = true
    });

    int counter2 = int.Parse(context.Request.Cookies["counter2"] ?? "0") + 1;
    context.Response.Cookies.Append("counter2", counter2.ToString(), new CookieOptions { MaxAge = TimeSpan.FromMinutes(30) });

    await context.Response.WriteAsync($"Counter1: {counter1}, Counter2: {counter2}");
});

//This middleware deletes the cookies when the /clear URL is requested
app.MapGet("clear", context =>
{
    context.Response.Cookies.Delete("counter1");
    context.Response.Cookies.Delete("counter2");
    context.Response.Redirect("/");
    return Task.CompletedTask;
});

// Enforce all the client requests to use https by redirection, if accessed over http.
app.UseHttpsRedirection();

app.UseSession();

app.Run();


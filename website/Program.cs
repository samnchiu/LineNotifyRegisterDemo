using website.DBContext;
using website.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using website.Middleware;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);



Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting web host");

    

// builder.WebHost.UseKestrel(options =>
// {
//     options.ConfigureHttpsDefaults(httpsOptions =>
//     {
//         httpsOptions.ServerCertificate = new X509Certificate2("certificate.pfx", "password");
//     });
// });



// Add services to the container.
builder.Services.AddControllersWithViews();
//註冊EFCoreContext
builder.Services.AddDbContext<EFCoreContext>();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options => {
    options.ClientId = builder.Configuration["Criipto:ClientId"];
    options.ClientSecret = builder.Configuration["Criipto:ClientSecret"];
    options.Authority = builder.Configuration["Criipto:Domain"];
    options.ResponseType = "code";
    options.Scope.Add("openid");
    options.Scope.Add("profile");

    options.Events = new OpenIdConnectEvents()
        {
            OnAuthorizationCodeReceived = context => {
                context.TokenEndpointRequest?.SetParameter("id_token_key_type", "JWK");
                return Task.CompletedTask;
            }
        };

        options.SaveTokens = true;

    // The next to settings must match the Callback URLs in Criipto Verify
    options.CallbackPath = new PathString("/Home/Callback"); 
    options.SignedOutCallbackPath = new PathString("/Home/signout");
});
builder.Services.AddScoped<LineUser>();
builder.Host.UseSerilog();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
    app.UseSerilogRequestLogging(options =>
    {
        // 如果要自訂訊息的範本格式，可以修改這裡，但修改後並不會影響結構化記錄的屬性
        options.MessageTemplate = "Handled {RequestPath}";

        // 預設輸出的紀錄等級為 Information，你可以在此修改記錄等級
        // options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

        // 你可以從 httpContext 取得 HttpContext 下所有可以取得的資訊！
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserID", httpContext.User.Identity?.Name);
        };
    });
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LineUserMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}").RequireAuthorization();

app.Run();


    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}



using AspNetCore.Proxy;
using ProxyKit;
using Constants = proxy_http_post_multipard_bad_encoding_test.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();




// ProxyKit configuration
// https://github.com/ProxyKit/ProxyKit
builder.Services.AddProxy();



// AspNetCore.Proxy configuration
// https://github.com/twitchax/AspNetCore.Proxy
builder.Services.AddProxies();
builder.Services
    .AddHttpClient("HostProxy")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            AllowAutoRedirect = false
        });



var app = builder.Build();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



// ProxyKit
app.UseWhen(context => context.Request.Path.StartsWithSegments("/ProxyKit"), app2 =>
{
    app2.RunProxy(async context =>
    {
        var forwardContext = context
            .ForwardTo(Constants.PostmanEchoPost)
            .AddXForwardedHeaders();

        forwardContext.UpstreamRequest.RequestUri = new Uri(Constants.PostmanEchoPost);

        return await forwardContext.Send();
    });
});


// AspNetCore.Proxy
app.UseWhen(context => context.Request.Path.StartsWithSegments("/AspNetCoreProxy"), app2 =>
{
    app2.RunHttpProxy(proxyBuilder =>
    {
        proxyBuilder.WithEndpoint("https://postman-echo.com/");
        proxyBuilder.WithOptions(optionsBuilder =>
        {
            optionsBuilder.WithBeforeSend((_, requestMessage) =>
            {
                requestMessage.RequestUri = new Uri(Constants.PostmanEchoPost);
                return Task.CompletedTask;
            });
            optionsBuilder.WithHttpClientName("HostProxy");
        });
    });
});


app.Run();
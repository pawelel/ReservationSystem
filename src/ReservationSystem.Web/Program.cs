using System.Globalization;
using Microsoft.AspNetCore.Localization;
using ReservationSystem.Application;
using ReservationSystem.Application.Abstractions;
using ReservationSystem.Infrastructure;
using ReservationSystem.Web.Api;
using ReservationSystem.Web.Auth;
using ReservationSystem.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.AddScoped<IUserContext, CookieUserContext>();
builder.Services.AddSingleton<ReservationSystem.Web.Demo.RaceRunner>();

builder.Services.AddLocalization();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("pl") };
builder.Services.Configure<RequestLocalizationOptions>(opts =>
{
    opts.DefaultRequestCulture = new RequestCulture("en");
    opts.SupportedCultures = supportedCultures;
    opts.SupportedUICultures = supportedCultures;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new() { Title = "Reservation System API", Version = "v1" }));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseRequestLocalization();
app.UseAntiforgery();

app.UseSwagger();
app.UseSwaggerUI();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapReservationApi();
app.MapSessionApi();
app.MapFormEndpoints();
app.MapDemoApi();

app.Run();

public partial class Program;

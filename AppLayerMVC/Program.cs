using Microsoft.EntityFrameworkCore;
using BLL.Models;
using DAL.EF;
using BLL;
using DAL.Repo;
using BLL.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.AccessDeniedPath = "/Login/Index";
    });
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// WHY: Warmup/Status uses the existing scoped context to run a lightweight check against the configured Neon database.
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConn")));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<LayoutRepo>();
builder.Services.AddScoped<BookingRepo>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LayoutService>();
builder.Services.AddScoped<BookingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Book/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();

// WHY: Prevent browser history from restoring authenticated pages after logout; static files have already been handled above.
app.Use(async (context, next) =>
{
    // WHY: Leave static asset responses cacheable even when an authenticated browser requests them.
    var isStaticAsset = Path.HasExtension(context.Request.Path.Value ?? string.Empty);
    if (context.User.Identity?.IsAuthenticated == true && !isStaticAsset)
    {
        context.Response.Headers.CacheControl = "no-store, no-cache";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
    }

    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    // WHY: Open with the database-independent warm-up page; the actual landing page URL remains /Select/Index.
    pattern: "{controller=Warmup}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();


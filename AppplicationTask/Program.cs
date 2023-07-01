using AppplicationTask.Data.Contexts;
using AppplicationTask.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<FileSystemContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("FileSystemDB");
    options.UseSqlServer(connStr);
});

builder.Services                   // аутентифікація за допомогою cookie
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/Account/Login");
        options.LogoutPath = new PathString("/Account/Logout");
    });

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IDbSerializer, DbSerializer>();

builder.Services.AddTransient<IHashService, HashService>();

var app = builder.Build();


using(var scope = app.Services.CreateScope())
{
    await DbInitializer.Init(scope.ServiceProvider.GetRequiredService<FileSystemContext>());
        // ініціалізація бд так як вказано на схемі
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Root}/{**path}");

app.Run();

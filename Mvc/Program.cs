using Repositories.Implementations;
using Repositories.Interfaces;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ItaskInterface, TaskRepository>();
builder.Services.AddScoped<IProjectInterface, ProjectRepository>();
builder.Services.AddScoped<IEmployeeInterface,EmployeeRepository>();
builder.Services.AddScoped<NpgsqlConnection>((conn) =>
{
    var connectionstring = conn.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionstring);
});

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";

    await next();
});

// ✅ Step 2: Block navigation to authenticated pages after logout
app.Use(async (context, next) =>
{
    // var path = context.Request.Path.Value?.ToLower();

    // If user is NOT logged in
    if (context.Session.GetString("EmployeeId") == null)
    {
        
        // And they try to access any page other than Login or Register
        if ("EmployeeId" == null)
        {
            // Allow Login, Register, and static files (CSS, JS, etc.)
            // if (!path.Contains("/login") &&
            //     !path.Contains("/register") &&
            //     !path.Contains("/css") &&
            //     !path.Contains("/js") &&
            //     !path.Contains("/images") &&
            //     !path.Contains("/lib"))
            // {
            //     context.Response.Redirect("/Home/Login");
            //     return;
            // }
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

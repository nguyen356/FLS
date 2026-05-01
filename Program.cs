using FlowerShop.Services;


var builder = WebApplication.CreateBuilder(args);

// ✅ FIXED: Scoped instead of Singleton for MongoDBService
builder.Services.AddScoped<MongoDBService>();  // ← Changed from Singleton

// ✅ Razor compilation
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// ✅ HttpContextAccessor & Session
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
// ✅ Session BEFORE authorization/routing
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
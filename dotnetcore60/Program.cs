using dotnetcore60.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ISimulator, SimulationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

string? shouldCrash = Environment.GetEnvironmentVariable("ShouldCrash");
if (!string.IsNullOrWhiteSpace(shouldCrash))
{
    throw new Exception("I am going to crash now at startup");
}

string? shouldHang = Environment.GetEnvironmentVariable("ShouldHang");
if (!string.IsNullOrWhiteSpace(shouldHang))
{
    Console.WriteLine("I am going to hang the app now");
}

app.Run();

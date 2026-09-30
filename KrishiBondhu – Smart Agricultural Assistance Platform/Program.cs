
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using KrishiBondhu___Smart_Agricultural_Assistance_Platform.Middleware;

QuestPDF.Settings.License = LicenseType.Evaluation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IFarmerService, FarmerService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<ICultivationService, CultivationService>();
builder.Services.AddScoped<ISoilTestService, SoilTestService>();
builder.Services.AddScoped<IDiseaseService, DiseaseService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// Custom Authentication Middleware
app.UseMiddleware<CustomAuthenticationMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();


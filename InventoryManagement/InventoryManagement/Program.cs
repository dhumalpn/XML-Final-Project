using InventoryManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InventoryManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<InventoryManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryManagementContext") ?? throw new InvalidOperationException("Connection string 'InventoryManagementContext' not found.")));

// Add memory cache (used by OpenFoodFactsService)
builder.Services.AddMemoryCache();

// Register OpenFoodFactsService with a typed HttpClient
builder.Services.AddHttpClient<OpenFoodFactsService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Register UpcLookupService
builder.Services.AddHttpClient<IUpcLookupService, UpcLookupService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

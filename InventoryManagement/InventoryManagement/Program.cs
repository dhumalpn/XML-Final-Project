using InventoryManagement.Data;
using InventoryManagement.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<InventoryManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryManagementContext") ?? throw new InvalidOperationException("Connection string 'InventoryManagementContext' not found.")));

// Add memory cache (used by OpenFoodFactsService)
builder.Services.AddMemoryCache();

// Add Razor Pages services
builder.Services.AddRazorPages();

// Register OpenFoodFactsService with a typed HttpClient
builder.Services.AddHttpClient<OpenFoodFactsService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Also add timeout for UPCItemDB client
builder.Services.AddHttpClient("UPCItemDB", client =>
{
	client.Timeout = TimeSpan.FromSeconds(30);
	client.BaseAddress = new Uri("https://api.upcitemdb.com/prod/trial/");
	client.DefaultRequestHeaders.Add("User-Agent", "InventoryManagement/1.0");
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

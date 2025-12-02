using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<InventoryManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventoryManagementContext") ?? throw new InvalidOperationException("Connection string 'InventoryManagementContext' not found.")));


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<InventoryManagementContext>();
        context.Database.Migrate();
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}
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

app.MapControllers();

app.Run();

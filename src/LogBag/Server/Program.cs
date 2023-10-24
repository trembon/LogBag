using LogBag.Services;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddRazorPages();

builder.Services.AddTransient<IAddLogsService, AddLogsService>();
builder.Services.AddSingleton<IMongoService, MongoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

//app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

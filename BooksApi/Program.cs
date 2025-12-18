using BooksApi.Data;
using BooksApi.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/booksapi-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Books API with AI Chat", 
        Version = "v1",
        Description = "A modern books management API with AI-powered chatbot"
    });
});

// Add memory cache
builder.Services.AddMemoryCache();

// CORS for React dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// EF Core with SQLite
var connectionString = builder.Configuration.GetConnectionString("Default")
                      ?? "Data Source=books.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// AI search service
builder.Services.AddScoped<IBookSearchService, SimpleBookSearchService>();

// Chat service - try OpenAI first, fall back to simple
var openAiKey = builder.Configuration["OpenAI:ApiKey"] 
    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (!string.IsNullOrEmpty(openAiKey))
{
    builder.Services.AddScoped<IChatService, OpenAIChatService>();
    Log.Information("Using OpenAI chat service");
}
else
{
    builder.Services.AddScoped<IChatService, SimpleChatService>();
    Log.Warning("OpenAI API key not found. Using simple chat service. Set OpenAI:ApiKey in appsettings.json or OPENAI_API_KEY environment variable for full AI features.");
}

var app = builder.Build();

// Ensure database exists and apply migrations (dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.MapControllers();

try
{
    Log.Information("Starting Books API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

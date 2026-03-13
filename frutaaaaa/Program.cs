using frutaaaaa.Data;
using frutaaaaa.Audit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "https://fruta-six.vercel.app",
                "https://fruta-api.ddnsfree.com", // Or your latest DDNS
                " https://scandic-hermine-snuffly.ngrok-free.dev", // Or your latest DDNS
                "https://fruta.accesscam.org"     // And this one too
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// Services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Audit Logging System ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditContext>();
builder.Services.AddScoped<AuditInterceptor>();

var journalConnectionString = builder.Configuration.GetConnectionString("JournalConnection");
builder.Services.AddDbContext<AuditDbContext>(options =>
    options.UseMySql(journalConnectionString, ServerVersion.AutoDetect(journalConnectionString)));

// HttpClient for ip-api.com geolocation (used by SessionController)
builder.Services.AddHttpClient();

var app = builder.Build();

// Set the global service provider so ApplicationDbContext can resolve the audit interceptor
ApplicationDbContext.ServiceProvider = app.Services;

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
  
// IIS already handles HTTPS, no need to force port binding
// Remove: app.Urls.Add("http://0.0.0.0:80");

//app.UseHttpsRedirection(); // keep this: redirects http:// to https://

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

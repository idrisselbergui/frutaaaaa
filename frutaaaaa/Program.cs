using frutaaaaa.Data;
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
                "http://localhost:5173",       // React Dev
                "https://fruta-six.vercel.app", // React on Vercel
                "https://fruta-api.ddnsfree.com" // ngrok address
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.UseHttpsRedirection(); // keep this: redirects http:// to https://

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

app.Run();

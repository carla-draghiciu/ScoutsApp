using Microsoft.EntityFrameworkCore;
using scout_api;
using scout_api.Controllers;
using scout_api.Hubs;
using scout_api.Middleware;
using scout_api.Repositories;
using scout_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
}); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SessionRepository>();
builder.Services.AddScoped<EventRepository>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LoggingService>();
builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    ));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            "http://172.20.10.3:4200",
            "https://172.20.10.3:4200",
            "http://192.168.100.30:4200",
            "https://192.168.100.30:4200",
            "https://cheerful-hope-production-d5cf.up.railway.app"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(a => a.Run(async context =>
{
    var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    if (ex != null)
        await context.Response.WriteAsJsonAsync(new { error = ex.Error.Message, detail = ex.Error.ToString() });
}));

//app.UseHttpsRedirection();

app.UseMiddleware<LoggingMiddleware>();

app.UseCors("AllowAngular");

app.MapHub<ChatHub>("/hubs/chat");

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

using Microsoft.EntityFrameworkCore;
using scout_api;
using scout_api.Controllers;
using scout_api.Hubs;
using scout_api.Middleware;
using scout_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SessionService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LoggingService>();
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngular", policy =>
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod());
//              //.AllowCredentials()); // for SignalR
//});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // for SignalR
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

//app.UseCors("AllowAngular");

app.UseHttpsRedirection();

app.UseMiddleware<LoggingMiddleware>();

app.UseCors("AllowAngular");

app.MapHub<ChatHub>("/hubs/chat");

app.UseAuthorization();

app.MapControllers();

app.Run();

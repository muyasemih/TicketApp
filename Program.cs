using Microsoft.EntityFrameworkCore;
using TicketApp.Models;
using TicketApp.Repositories;
using TicketApp.Services;
using TicketApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseExceptionHandler("/error");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();




app.Run();
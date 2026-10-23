using System.Text.Json.Serialization;
using InternalRequests.Application.Interfaces;
using InternalRequests.Application.Services;
using InternalRequests.Infrastructure.Data;
using InternalRequests.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

builder.Services.AddScoped<IDemandRepository, DemandRepository>();
builder.Services.AddScoped<DemandService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return false;

                var host = uri.Host;

                return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
                    || host.Equals("internal-requests-gamma.vercel.app", StringComparison.OrdinalIgnoreCase)
                    || host.Equals("internal-requests-git-second-aonosorahs-projects.vercel.app", StringComparison.OrdinalIgnoreCase)
                    || host.EndsWith(
                        "-aonosorahs-projects.vercel.app",
                        StringComparison.OrdinalIgnoreCase
                    )
                    || host.EndsWith(
                        ".app.github.dev",
                        StringComparison.OrdinalIgnoreCase
                    );
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.MapControllers();

app.Run();
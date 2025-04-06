using Microsoft.EntityFrameworkCore;
using XRPAtom.Infrastructure.Data;
using XRPAtom.Infrastructure;
using XRPAtom.Blockchain;
using XRPAtom.API.Configuration;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using XRPAtom.Infrastructure.BackgroundServices;

namespace XRPAtom.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            // Configure Swagger with JWT Authentication
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "XRP Atom API", 
                    Version = "v1",
                    Description = "API for XRP Atom Energy Curtailment Platform"
                });

                // Define the security scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Make sure swagger UI requires a Bearer token specified
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            // Get connection string
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Please add it to your appsettings.json file.");
            }

            // Add DbContext with PostgreSQL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString,
                    b => b.MigrationsAssembly("XRPAtom.Infrastructure")));

            // Add JWT Authentication
            builder.Services.AddJwtAuthentication(builder.Configuration);

            // Register Infrastructure services
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Register Blockchain services
            builder.Services.AddBlockchainServices(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "XRP Atom API v1");
                });
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            // Add Authentication and Authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Run migrations automatically in development (with better error handling)
            if (app.Environment.IsDevelopment())
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        
                    logger.LogInformation("Ensuring database is created with all tables...");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Database creation completed successfully");
                }
                catch (Exception ex)
                {
                    using var scope = app.Services.CreateScope();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while creating the database");
                }
            }

            app.Run();
        }
    }
}
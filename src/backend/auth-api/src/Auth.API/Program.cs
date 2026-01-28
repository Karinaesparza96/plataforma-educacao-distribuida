using Auth.API.Extensions;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.Application.Settings;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Core.Utils;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

        TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);

        builder.Services.AddDatabaseConfiguration(builder.Configuration, builder.Environment);
        builder.Services.AddIdentityConfiguration();
        builder.Services.AddJwtConfiguration(builder.Configuration);
        builder.Services.AddJwksConfiguration();

        builder.Services.AddAuthorization();
        builder.Services.AddMemoryCache();
        builder.Services.AddResilienceConfiguration(builder.Configuration);

        builder.Services.AddScoped<AuthService, AuthService>();
        builder.Services.AddScoped<IAuthDbContext>(provider => provider.GetRequiredService<AuthDbContext>());

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        builder.Services.AddScoped<Core.Mediator.IMediatorHandler, Core.Mediator.MediatorHandler>();
        builder.Services.AddScoped<MediatR.INotificationHandler<Core.Messages.DomainNotificacaoRaiz>, Core.Messages.DomainNotificacaoHandler>();

        builder.Services.RegisterNotification();

        builder.Services.AddControllers();

        builder.Services.AddSwaggerConfiguration();

        builder.Services.AddMessageBusConfiguration(builder.Configuration);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });


        var urls = builder.Configuration["Urls"];
        if (!string.IsNullOrEmpty(urls))
        {
            builder.WebHost.UseUrls(urls);
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseCors("AllowAll");
        app.UseJwksDiscovery();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", DateTime = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithOpenApi();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await InitializeDatabaseAsync(context, userManager, roleManager);
        }

        app.Run();

        static async Task InitializeDatabaseAsync(AuthDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await context.Database.EnsureCreatedAsync();

            string[] roles = { "Administrador", "Usuario" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            const string adminEmail = "admin@auth.api";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nome = "Administrador do Sistema",
                    DataNascimento = new DateTime(1990, 1, 1),
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Teste@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrador");
                }

                var aluno1 = new ApplicationUser
                {
                    Id = "06b1b8f1-f079-4048-9c8d-190c8056ea60",
                    UserName = "aluno1@auth.api",
                    Email = "aluno1@auth.api",
                    Nome = "Aluno UM do Sistema",
                    DataNascimento = new DateTime(1973, 1, 1),
                    EmailConfirmed = true
                };

                var resultAlunoUm = await userManager.CreateAsync(aluno1, "Teste@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(aluno1, "Usuario");
                }

                var aluno2 = new ApplicationUser
                {
                    Id = "ca39e314-c960-42bc-9c9d-3cad9b589a8d",
                    UserName = "aluno2@auth.api",
                    Email = "aluno2@auth.api",
                    Nome = "Aluno DOIS do Sistema",
                    DataNascimento = new DateTime(2016, 1, 1),
                    EmailConfirmed = true
                };

                var resultAlunoDois = await userManager.CreateAsync(aluno2, "Teste@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(aluno2, "Usuario");
                }
            }
        }
    }
}

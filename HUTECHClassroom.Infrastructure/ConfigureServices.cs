﻿using HUTECHClassroom.Domain.Constants.Services;
using HUTECHClassroom.Domain.Interfaces;
using HUTECHClassroom.Infrastructure.Services.Authentication;
using HUTECHClassroom.Infrastructure.Services.ComputerVision;
using HUTECHClassroom.Infrastructure.Services.Email;
using HUTECHClassroom.Infrastructure.Services.Excel;
using HUTECHClassroom.Infrastructure.Services.Photos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HUTECHClassroom.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        #region Authentication
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[ServiceConstants.TOKEN_KEY]));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var hasCookieToken = context.Request.Cookies.TryGetValue(AuthenticationConstants.CookieAccessToken, out var cookieToken);
                        if (hasCookieToken && cookieToken is { })
                        {
                            context.Token = cookieToken;
                            return Task.CompletedTask;
                        }
                        var hasAccessToken = context.Request.Query.TryGetValue(AuthenticationConstants.WebSocketAccessToken, out var accessToken);
                        if (hasAccessToken && accessToken is { })
                        {
                            context.Token = accessToken;
                            return Task.CompletedTask;
                        }
                        return Task.CompletedTask;
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(5)
                };
            });

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserAccessor, UserAccessor>();
        #endregion

        #region Clients
        services.AddSingleton<IComputerVisionClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AzureComputerVisionSettings>>().Value;

            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(settings.Key))
            {
                Endpoint = settings.Endpoint
            };
        });
        #endregion

        #region Settings
        services.Configure<GmailSMTPSettings>(configuration.GetSection("EmailService:Gmail"));
        services.Configure<CloudinarySettings>(configuration.GetSection(ServiceConstants.CLOUDINARY_SETTINGS_KEY));
        services.Configure<AzureComputerVisionSettings>(configuration.GetSection("Azure:ComputerVision"));
        #endregion

        #region Services
        services.AddHttpContextAccessor();
        services.AddScoped<IEmailService, GmailSMTPService>();
        services.AddScoped<IExcelServie, ExcelSerive>();
        services.AddScoped<IPhotoAccessor, CloudinaryPhotoAccessor>();
        services.AddScoped<IAzureComputerVisionService, AzureComputerVisionService>();
        #endregion

        return services;
    }
    public static Task<WebApplication> UseInfrastructureAsync(this WebApplication app)
    {
        return Task.FromResult(app);
    }
}

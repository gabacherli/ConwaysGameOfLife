﻿using GameOfLife.API.Middleware.Providers;
using GameOfLife.API.Repositories.Read;
using GameOfLife.API.Repositories.Write;
using GameOfLife.API.Services;
using GameOfLife.API.Settings;

namespace GameOfLife.API.Extensions
{
    public static class ServicesExtensions
    {
        public static void AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(configuration);
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IBoardService, BoardService>();
            services.AddTransient<IBoardReadRepository, BoardReadRepository>();
            services.AddTransient<IBoardWriteRepository, BoardWriteRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<TraceIdProvider>();
        }
    }
}

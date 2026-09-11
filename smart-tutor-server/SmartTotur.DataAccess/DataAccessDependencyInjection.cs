using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTutor.DataAccess.Claims;
using SmartTutor.DataAccess.Persistence;
using SmartTutor.DataAccess.Repositories.Impl;
using SmartTutor.DataAccess.Repositories.Repo;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.DataAccess
{
    public static class DataAccessDependencyInjection
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration); // 1. Cấu hình DB

            services.AddRepositories();          // 2. Đăng ký Repositories & Services

            return services;
        }

        private static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IResetPasswordTokenRepository, ResetPasswordTokenRepository>();
            services.AddScoped<IClaimService, ClaimService>();
        }

        private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseConfig = configuration.GetSection("Database").Get<DatabaseConfiguration>();
            if (databaseConfig != null && databaseConfig.UseInMemoryDatabase)
            {
                services.AddDbContext<SmartTutorContext>(options =>
                    options.UseInMemoryDatabase("smart_tutor"));
            }
            else
            {
                var connectionString = databaseConfig?.ConnectionString ?? string.Empty;
                services.AddDbContext<SmartTutorContext>(options =>
                    options.UseSqlServer(connectionString,
                        opt => opt.MigrationsAssembly(typeof(SmartTutorContext).Assembly.FullName)));
            }
        }

        private class DatabaseConfiguration
        {
            public bool UseInMemoryDatabase { get; set; }
            public string? ConnectionString { get; set; }
        }
    }
}

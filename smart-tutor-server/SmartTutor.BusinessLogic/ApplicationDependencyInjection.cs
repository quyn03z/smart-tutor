using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartTutor.BusinessLogic
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IHostEnvironment env)
        {
            services.AddServices(env);

            return services;
        }

        private static void AddServices(this IServiceCollection services, IHostEnvironment env)
        { 
        }
    }
}

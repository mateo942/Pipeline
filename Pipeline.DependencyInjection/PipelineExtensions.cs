using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Pipeline.DependencyInjection
{
    public static class PipelineExtensions
    {
        public static IServiceCollection AddPipeline(this IServiceCollection services)
        {
            services.AddScoped<IPipelineManager, PipelineManagerDI>();

            return services;
        }
    }
}

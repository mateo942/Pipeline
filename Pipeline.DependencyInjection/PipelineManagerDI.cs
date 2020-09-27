using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline.DependencyInjection
{
    public class PipelineManagerDI : PipelineManager
    {
        private readonly IServiceProvider _serviceProvider;

        public PipelineManagerDI(IServiceProvider  serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override PipelineRunner Configure(PipelineConfigurationBase pipelineConfiguration)
        {
            var context = new PipelineContext(pipelineConfiguration.GetSteps().Select(x => x.Id).ToArray());

            return new PipelineRunnerDI(pipelineConfiguration, context, _serviceProvider);
        }
    }
}

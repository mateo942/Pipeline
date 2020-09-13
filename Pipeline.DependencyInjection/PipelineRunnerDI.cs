using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Pipeline.DependencyInjection
{
    public class PipelineRunnerDI : PipelineRunner
    {
        private readonly IServiceProvider _serviceProvider;

        public PipelineRunnerDI(PipelineConfigurationBase pipelineConfiguration, PipelineContext pipelineContext, IServiceProvider serviceProvider) 
            : base(pipelineConfiguration, pipelineContext)
        {
            _serviceProvider = serviceProvider;
        }

        protected override object GetPipelineObject(IStepConfiguration @object)
        {
            if (@object.Instance is Type type)
                return _serviceProvider.GetRequiredService(type);

            return base.GetPipelineObject(@object);
        }
    }
}

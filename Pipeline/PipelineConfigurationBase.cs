using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public interface IStepConfiguration
    {
        object Instance { get; }
        Variables LocalVariable { get; }
    }

    public interface IStepConfiguration<T> : IStepConfiguration where T : IPipelineCommand
    {
        T Command { get; }
    }

    public abstract class PipelineConfigurationBase
    {
        public abstract IStepConfiguration GetNext();
    }
}

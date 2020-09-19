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
        public abstract Variables GlobalVariable { get; }

        public abstract IStepConfiguration GetNext();

        public abstract Action<PipelineContext> BeforeStart { get; protected set; }
        public abstract Action<PipelineContext> BeforeStep { get; protected set; }
        public abstract Action<PipelineContext> AfterStep { get; protected set; }
        public abstract Action<PipelineContext> AfterEnd { get; protected set; }
        public abstract Action<PipelineContext> AlwaysEnd { get; protected set; }
    }
}

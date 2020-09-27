using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public interface IStepConfiguration
    {
        string Id { get; }

        string Scope { get; }

        object Instance { get; }
        Variables LocalVariable { get; }

        bool AlwaysRun { get; set; }
    }

    public interface IStepConfiguration<T> : IStepConfiguration where T : IPipelineCommand
    {
        T Command { get; }
    }

    public abstract class PipelineConfigurationBase
    {
        public abstract Variables GlobalVariable { get; }

        public abstract IEnumerable<IStepConfiguration> GetSteps();

        public abstract Action<PipelineContext> BeforeStart { get; protected set; }
        public abstract Action<PipelineContext> BeforeStep { get; protected set; }
        public abstract Action<PipelineContext> AfterStep { get; protected set; }
        public abstract Action<PipelineContext> AfterEnd { get; protected set; }
        public abstract Action<PipelineContext> AlwaysEnd { get; protected set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public class StepConfiguration : IStepConfiguration
    {
        public StepConfiguration(object instance, Variables localVariable)
        {
            Instance = instance;
            LocalVariable = localVariable;
        }

        public object Instance { get; }
        public Variables LocalVariable { get; }
    }

    public class StepConfigurationWithCommand<T> : StepConfiguration, IStepConfiguration<T> where T : IPipelineCommand
    {
        public StepConfigurationWithCommand(object instance, Variables localVariable, T command) 
            : base(instance, localVariable)
        {
            Command = command;
        }

        public T Command { get; }
    }

    public class PipelineConfiguration : PipelineConfigurationBase
    {
        //Instance\Type, Local variable, command
        private readonly Queue<IStepConfiguration> _queue;

        #region Instance
        public PipelineConfiguration()
        {
            _queue = new Queue<IStepConfiguration>();
        }

        public PipelineConfiguration NextStep(IPipeline pipeline)
        {
            return NextStep(pipeline, new Variables());
        }

        public PipelineConfiguration NextStep(IPipeline pipeline, Variables variables)
        {
            var p = new StepConfiguration(pipeline, variables);
            _queue.Enqueue(p);

            return this;
        }

        public PipelineConfiguration NextStep<T>(IPipeline<T> pipeline, T command) where T : IPipelineCommand
        {
            return NextStep<T>(pipeline, new Variables(), command);
        }

        public PipelineConfiguration NextStep<T>(IPipeline<T> pipeline, Variables variables, T command) where T : IPipelineCommand
        {
            var p = new StepConfigurationWithCommand<T>(pipeline, variables, command);
            _queue.Enqueue(p);

            return this;
        } 
        #endregion

        #region Type
        public PipelineConfiguration NextStep<TPipeline>()
        {
            return NextStep<TPipeline>(new Variables());
        }

        public PipelineConfiguration NextStep<TPipeline>(Variables variables)
        {
            var type = typeof(TPipeline);

            return NextStep(type, variables);
        }

        public PipelineConfiguration NextStep(Type pipeline)
        {
            return NextStep(pipeline, new Variables());
        }

        public PipelineConfiguration NextStep(Type pipeline, Variables variables)
        {
            var p = new StepConfiguration(pipeline, variables);
            _queue.Enqueue(p);

            return this;
        }

        public PipelineConfiguration NextStep<TPipeline, T>(T command) where T : IPipelineCommand
        {
            return NextStep<T>(typeof(TPipeline), command);
        }

        public PipelineConfiguration NextStep<TPipeline, T>(T command, Variables variables) where T : IPipelineCommand
        {
            return NextStep<T>(typeof(TPipeline), variables, command);
        }

        public PipelineConfiguration NextStep<T>(Type pipeline, T command) where T : IPipelineCommand
        {
            return NextStep<T>(pipeline, new Variables(), command);
        }

        public PipelineConfiguration NextStep<T>(Type pipeline, Variables variables, T command) where T : IPipelineCommand
        {
            var p = new StepConfigurationWithCommand<T>(pipeline, variables, command);
            _queue.Enqueue(p);

            return this;
        }
        #endregion

        public override IStepConfiguration GetNext()
        {
            if (_queue.Count == 0)
                return null;

            return _queue.Dequeue();
        }
    }
}

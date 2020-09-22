using System;
using System.Collections.Generic;
using System.Text;

namespace Pipeline
{
    public class StepConfiguration : IStepConfiguration
    {
        public StepConfiguration(object instance, Variables localVariable, string scope)
        {
            Instance = instance;
            LocalVariable = localVariable;
            Scope = scope;
        }

        public object Instance { get; }
        public Variables LocalVariable { get; }
        public string Scope { get; }
    }

    public class StepConfigurationWithCommand<T> : StepConfiguration, IStepConfiguration<T> where T : IPipelineCommand
    {
        public StepConfigurationWithCommand(object instance, Variables localVariable, string scope, T command) 
            : base(instance, localVariable, scope)
        {
            Command = command;
        }

        public T Command { get; }
    }

    public class PipelineConfiguration : PipelineConfigurationBase
    {
        //Instance\Type, Local variable, command
        private readonly Queue<IStepConfiguration> _queue;

        private readonly Variables _variables = new Variables();
        public override Variables GlobalVariable => _variables;


        public override Action<PipelineContext> BeforeStart { get; protected set; }
        public override Action<PipelineContext> BeforeStep { get; protected set; }
        public override Action<PipelineContext> AfterStep { get; protected set; }
        public override Action<PipelineContext> AfterEnd { get; protected set; }
        public override Action<PipelineContext> AlwaysEnd { get; protected set; }

        #region Instance
        public PipelineConfiguration()
        {
            _queue = new Queue<IStepConfiguration>();
        }

        public PipelineConfiguration NextStep(IPipeline pipeline)
        {
            return NextStep(pipeline, Variables.Empty);
        }

        public PipelineConfiguration NextStep(IPipeline pipeline, Variables variables)
        {
            return NextStep(pipeline, variables, string.Empty);
        }

        public PipelineConfiguration NextStep(IPipeline pipeline, Variables variables, string scope)
        {
            var p = new StepConfiguration(pipeline, variables, scope);
            _queue.Enqueue(p);

            return this;
        }

        public PipelineConfiguration NextStep<T>(IPipeline<T> pipeline, T command) where T : IPipelineCommand
        {
            return NextStep<T>(pipeline, string.Empty, command);
        }

        public PipelineConfiguration NextStep<T>(IPipeline<T> pipeline, string scope, T command) where T : IPipelineCommand
        {
            return NextStep<T>(pipeline, Variables.Empty, scope, command);
        }

        public PipelineConfiguration NextStep<T>(IPipeline<T> pipeline, Variables variables, string scope, T command) where T : IPipelineCommand
        {
            var p = new StepConfigurationWithCommand<T>(pipeline, variables, scope, command);
            _queue.Enqueue(p);

            return this;
        } 
        #endregion

        #region Type
        public PipelineConfiguration NextStep<TPipeline>()
        {
            return NextStep<TPipeline>(Variables.Empty);
        }

        public PipelineConfiguration NextStep<TPipeline>(Variables variables)
        {
            var type = typeof(TPipeline);

            return NextStep(type, variables);
        }

        public PipelineConfiguration NextStep<TPipeline>(Variables variables, string scope)
        {
            var type = typeof(TPipeline);

            return NextStep(type, variables, scope);
        }

        public PipelineConfiguration NextStep(Type pipeline)
        {
            return NextStep(pipeline, Variables.Empty);
        }

        public PipelineConfiguration NextStep(Type pipeline, Variables variables)
        {
            return NextStep(pipeline, variables, string.Empty);

        }

        public PipelineConfiguration NextStep(Type pipeline, Variables variables, string scope)
        {
            var p = new StepConfiguration(pipeline, variables, scope);
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

        public PipelineConfiguration NextStep<TPipeline, T>(T command, Variables variables, string scope) where T : IPipelineCommand
        {
            return NextStep<T>(typeof(TPipeline), variables, scope, command);
        }

        public PipelineConfiguration NextStep<T>(Type pipeline, T command) where T : IPipelineCommand
        {
            return NextStep<T>(pipeline, Variables.Empty, command);
        }

        public PipelineConfiguration NextStep<T>(Type pipeline, Variables variables, T command) where T : IPipelineCommand
        {
            return NextStep<T>(pipeline, variables, string.Empty, command);
        }

        public PipelineConfiguration NextStep<T>(Type pipeline, Variables variables, string scope, T command) where T : IPipelineCommand
        {
            var p = new StepConfigurationWithCommand<T>(pipeline, variables, scope, command);
            _queue.Enqueue(p);

            return this;
        }
        #endregion

        public PipelineConfiguration AddGlobalVariable<T>(string key, T value)
        {
            _variables.Set(key, value);
            return this;
        }
        public PipelineConfiguration AddGlobalVariables(IDictionary<string, object> v)
        {
            _variables.AddRange(v);

            return this;
        }

        public override IStepConfiguration GetNext()
        {
            if (_queue.Count == 0)
                return null;

            return _queue.Dequeue();
        }

        public PipelineConfiguration AddBeforeStart(Action<PipelineContext> action)
        {
            BeforeStart = action;
            return this;
        }

        public PipelineConfiguration AddBeforeStep(Action<PipelineContext> action)
        {
            BeforeStep = action;
            return this;
        }

        public PipelineConfiguration AddAfterStep(Action<PipelineContext> action)
        {
            AfterStep = action;
            return this;
        }

        public PipelineConfiguration AddAfterEnd(Action<PipelineContext> action)
        {
            AfterEnd = action;
            return this;
        }

        public PipelineConfiguration AddAlwaysEnd(Action<PipelineContext> action)
        {
            AlwaysEnd = action;
            return this;
        }
    }
}

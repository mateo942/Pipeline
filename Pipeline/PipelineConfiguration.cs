using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pipeline
{
    public class StepConfiguration : IStepConfiguration
    {
        public string Id { get; }

        public StepConfiguration(object instance, Variables localVariable, string scope)
        {
            Instance = instance;
            LocalVariable = localVariable;
            Scope = scope;
            Id = Guid.NewGuid().ToString();
        }

        public StepConfiguration(string id, object instance, Variables localVariable, string scope)
        {
            Instance = instance;
            LocalVariable = localVariable;
            Scope = scope;
            Id = id;
        }

        public object Instance { get; }
        public Variables LocalVariable { get; }
        public string Scope { get; }

        public string RunWith { get; set; }
        public bool AlwaysRun { get; set; }

        public void Configure(Action<IStepConfiguration> cfg)
        {
            cfg?.Invoke(this);
        }
    }

    public class StepConfigurationWithCommand<T> : StepConfiguration, IStepConfiguration<T> where T : IPipelineCommand
    {
        public StepConfigurationWithCommand(object instance, Variables localVariable, string scope, T command) 
            : base(instance, localVariable, scope)
        {
            Command = command;
        }

        public StepConfigurationWithCommand(string id, object instance, Variables localVariable, string scope, T command)
           : base(id, instance, localVariable, scope)
        {
            Command = command;
        }

        public T Command { get; }
    }

    public class PipelineConfiguration : PipelineConfigurationBase
    {
        //Instance\Type, Local variable, command
        private readonly List<IStepConfiguration> _queue;

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
            _queue = new List<IStepConfiguration>();
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
            _queue.Add(p);

            return this;
        }

        public PipelineConfiguration NextStepWithCommand<T>(IPipeline<T> pipeline, T command) where T : IPipelineCommand
        {
            return NextStepWithCommand<T>(pipeline, string.Empty, command);
        }

        public PipelineConfiguration NextStepWithCommand<T>(IPipeline<T> pipeline, string scope, T command) where T : IPipelineCommand
        {
            return NextStepWithCommand<T>(pipeline, Variables.Empty, scope, command, null);
        }

        public PipelineConfiguration NextStepWithCommand<T>(IPipeline<T> pipeline, Variables variables, string scope, T command, Action<IStepConfiguration> cfg) where T : IPipelineCommand
        {
            var p = new StepConfigurationWithCommand<T>(pipeline, variables, scope, command);
            p.Configure(cfg);
            _queue.Add(p);

            return this;
        }

        public PipelineConfiguration NextStepWithCommand<T>(string id, IPipeline<T> pipeline, Variables variables, string scope, T command, Action<IStepConfiguration> cfg) where T : IPipelineCommand
        {
            var p = new StepConfigurationWithCommand<T>(id, pipeline, variables, scope, command);
            p.Configure(cfg);
            _queue.Add(p);

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

            return NextStep(type, variables, scope, null);
        }

        public PipelineConfiguration NextStep<TPipeline>(Variables variables, string scope, Action<IStepConfiguration> cfg)
        {
            var type = typeof(TPipeline);

            return NextStep(type, variables, scope, cfg);
        }

        public PipelineConfiguration NextStep(Type pipeline)
        {
            return NextStep(pipeline, Variables.Empty);
        }

        public PipelineConfiguration NextStep(Type pipeline, Variables variables)
        {
            return NextStep(pipeline, variables, string.Empty, null);

        }

        public PipelineConfiguration NextStep(Type pipeline, Variables variables, string scope, Action<IStepConfiguration> cfg)
        {
            var p = new StepConfiguration(pipeline, variables, scope);
            p.Configure(cfg);
            _queue.Add(p);

            return this;
        }

        public PipelineConfiguration NextStep(string id, Type pipeline, Variables variables, string scope, Action<IStepConfiguration> cfg)
        {
            var p = new StepConfiguration(id, pipeline, variables, scope);
            p.Configure(cfg);
            _queue.Add(p);

            return this;
        }

        public PipelineConfiguration NextStepWithCommand<TPipeline, T>(T command) where T : IPipelineCommand
        {
            return NextStepWithCommand<T>(typeof(TPipeline), command);
        }

        public PipelineConfiguration NextStepWithCommand<TPipeline, T>(T command, Variables variables) where T : IPipelineCommand
        {
            return NextStepWithCommand<T>(typeof(TPipeline), variables, command);
        }

        public PipelineConfiguration NextStepWithCommand<TPipeline, T>(T command, Variables variables, string scope) where T : IPipelineCommand
        {
            return NextStepWithCommand<T>(typeof(TPipeline), variables, scope, command);
        }

        public PipelineConfiguration NextStepWithCommand<T>(Type pipeline, T command) where T : IPipelineCommand
        {
            return NextStepWithCommand<T>(pipeline, Variables.Empty, command);
        }

        public PipelineConfiguration NextStepWithCommand<T>(Type pipeline, Variables variables, T command) where T : IPipelineCommand
        {
            return NextStepWithCommand<T>(pipeline, variables, string.Empty, command);
        }

        public PipelineConfiguration NextStepWithCommand<T>(Type pipeline, Variables variables, string scope, T command) where T : IPipelineCommand
        {
            var p = new StepConfigurationWithCommand<T>(pipeline, variables, scope, command);
            _queue.Add(p);

            return this;
        }

        public PipelineConfiguration NextStepWithCommand<T>(string id, Type pipeline, Variables variables, string scope,
            T command, Action<IStepConfiguration> cfg) where T : IPipelineCommand
        {
            var p = new StepConfigurationWithCommand<T>(id, pipeline, variables, scope, command);
            p.Configure(cfg);
            _queue.Add(p);

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

        public override IEnumerable<IStepConfiguration> GetSteps()
        {
            return _queue.ToList();
        }
    }
}

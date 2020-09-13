using System;
using System.Threading;

namespace Pipeline
{
    public class PipelineManager
    {
        public PipelineRunner Configure(PipelineConfigurationBase pipelineConfiguration)
        {
            var context = new PipelineContext();

            return new PipelineRunner(pipelineConfiguration, context);
        }
    }

    public class PipelineRunner
    {
        private readonly PipelineConfigurationBase _pipelineConfiguration;
        private readonly PipelineContext _pipelineContext;

        public PipelineRunner(PipelineConfigurationBase pipelineConfiguration, PipelineContext pipelineContext)
        {
            _pipelineConfiguration = pipelineConfiguration;
            _pipelineContext = pipelineContext;
        }

        public bool Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            InitGlobalVariable();

            IStepConfiguration stepConfiguration;
            while ((stepConfiguration = _pipelineConfiguration.GetNext()) != null && cancellationToken.IsCancellationRequested == false)
            {
                _pipelineContext.ClearLocalVariable();
                _pipelineContext.SetLocalVariable(stepConfiguration.LocalVariable);

                var pip = GetPipelineObject(stepConfiguration);

                if (pip is IPipeline pipeline)
                {
                    pipeline.Execute(_pipelineContext, cancellationToken);
                }
                else
                {
                    var type = pip.GetType();
                    var genericInterface = type.GetInterface(typeof(IPipeline<>).Name);

                    if (genericInterface != null)
                    {
                        var command = stepConfiguration.GetType().GetProperty("Command").GetValue(stepConfiguration);

                        var method = type.GetMethod("Execute");
                        method.Invoke(pip, new object[] { command, _pipelineContext, cancellationToken });
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            return true;
        }

        protected virtual object GetPipelineObject(IStepConfiguration @object)
        {
            return @object.Instance;
        }

        protected virtual void InitGlobalVariable()
        {
            var v = _pipelineContext.GlobalVariables;

            v.Add("DATE_START", DateTime.UtcNow);
        }
    }
}

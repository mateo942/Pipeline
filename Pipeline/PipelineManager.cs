using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline
{
    public interface IPipelineManager
    {
        PipelineRunner Configure(PipelineConfigurationBase pipelineConfiguration);
    }

    public class PipelineManager : IPipelineManager
    {
        public virtual PipelineRunner Configure(PipelineConfigurationBase pipelineConfiguration)
        {
            var context = new PipelineContext();

            return new PipelineRunner(pipelineConfiguration, context);
        }
    }

    public interface IPipelineRunner
    {
        Task<bool> Run(CancellationToken cancellationToken = default(CancellationToken));
    }

    public class PipelineRunner : IPipelineRunner
    {
        private readonly PipelineConfigurationBase _pipelineConfiguration;
        private readonly PipelineContext _pipelineContext;

        public PipelineRunner(PipelineConfigurationBase pipelineConfiguration, PipelineContext pipelineContext)
        {
            _pipelineConfiguration = pipelineConfiguration;
            _pipelineContext = pipelineContext;
        }

        public async Task<bool> Run(CancellationToken cancellationToken = default(CancellationToken))
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
                    await pipeline.Execute(_pipelineContext, cancellationToken);
                }
                else
                {
                    var type = pip.GetType();
                    var genericInterface = type.GetInterface(typeof(IPipeline<>).Name);

                    if (genericInterface != null)
                    {
                        var command = stepConfiguration.GetType().GetProperty("Command").GetValue(stepConfiguration);

                        var method = type.GetMethod("Execute");
                        Task task = (Task) method.Invoke(pip, new object[] { command, _pipelineContext, cancellationToken });

                        await task.ConfigureAwait(false);
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

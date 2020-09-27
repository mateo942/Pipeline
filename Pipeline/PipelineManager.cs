using System;
using System.Collections.Generic;
using System.Linq;
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
            var context = new PipelineContext(pipelineConfiguration.GetSteps().Select(x => x.Id).ToArray());

            return new PipelineRunner(pipelineConfiguration, context);
        }
    }

    public interface IPipelineRunner
    {
        Task<bool> Run(CancellationToken cancellationToken = default(CancellationToken));
    }

    public class PipelineRunner : IPipelineRunner
    {
        internal const string DATE_START = "DATE_START";

        private readonly PipelineConfigurationBase _pipelineConfiguration;
        private readonly PipelineContext _pipelineContext;

        public PipelineRunner(PipelineConfigurationBase pipelineConfiguration, PipelineContext pipelineContext)
        {
            _pipelineConfiguration = pipelineConfiguration;
            _pipelineContext = pipelineContext;
        }

        public async Task<bool> Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                InitGlobalVariable();

                _pipelineConfiguration.BeforeStart?.Invoke(_pipelineContext);

                var steps = _pipelineConfiguration.GetSteps().ToArray();
                string[] executedStep = new string[steps.Count()];

                bool error = false;

                List<Exception> exceptions = new List<Exception>();

                for (int i = 0; i < steps.Length; i++)
                {
                    IStepConfiguration stepConfiguration = steps[i];

                    try
                    {
                        _pipelineContext.CurrentStepId = stepConfiguration.Id;

                        if (cancellationToken.IsCancellationRequested && (error == false || stepConfiguration.AlwaysRun))
                            continue;

                        _pipelineContext.ClearLocalVariable();
                        _pipelineContext.SetLocalVariable(stepConfiguration.LocalVariable);
                        _pipelineContext.SetCurrentScope(stepConfiguration.Scope);

                        _pipelineConfiguration.BeforeStep?.Invoke(_pipelineContext);

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
                                Task task = (Task)method.Invoke(pip, new object[] { command, _pipelineContext, cancellationToken });

                                await task.ConfigureAwait(false);
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }

                        executedStep[i] = stepConfiguration.Id;

                        _pipelineConfiguration.AfterStep?.Invoke(_pipelineContext);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(new PipelineException(ex.Message, stepConfiguration.Id, stepConfiguration.GetType().FullName, ex));
                        error = true;
                    }
                }

                if (error)
                {
                    throw new AggregatePipelineException(exceptions);
                }

                _pipelineConfiguration.AfterEnd?.Invoke(_pipelineContext);
            }
            finally
            {
                _pipelineConfiguration.AlwaysEnd?.Invoke(_pipelineContext);
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

            v.Add(DATE_START, DateTime.UtcNow);

            v.AddRange(_pipelineConfiguration.GlobalVariable);
        }
    }
}

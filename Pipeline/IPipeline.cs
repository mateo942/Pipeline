using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline
{
    public interface IPipeline
    {
        Task Execute(PipelineContext pipelineContext, CancellationToken cancellationToken);
    }

    public interface IPipelineCommand
    {

    }

    public interface IPipeline<in T> where T : IPipelineCommand
    {
        Task Execute(T command, PipelineContext pipelineContext, CancellationToken cancellationToken);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new PipelineConfiguration()
                .NextStep(new StepOne())
                .NextStep(new StepGlobalVariable())
                .NextStep(new StepShowMessageFromCommand(), new StepCommand() { Message = "Hello world" })
                .NextStep(new StepLocalVariable(), new Variables { { "MESSAGE", "Hello from variables"} });


            var manager = new PipelineManager();

            manager.Configure(configuration)
                .Run();
        }
    }

    public class StepOne : IPipeline
    {
        public Task Execute(PipelineContext pipelineContext, CancellationToken cancellationToken)
        {
            Console.WriteLine("Step One");

            return Task.CompletedTask;
        }
    }

    public class StepGlobalVariable : IPipeline
    {
        public Task Execute(PipelineContext pipelineContext, CancellationToken cancellationToken)
        {
            var dateStart = pipelineContext.GlobalVariables.Get<DateTime>("DATE_START");

            Console.WriteLine("Date start: {0}", dateStart);

            return Task.CompletedTask;
        }
    }

    public class StepLocalVariable : IPipeline
    {
        public Task Execute(PipelineContext pipelineContext, CancellationToken cancellationToken)
        {
            var message = pipelineContext.LocalVariables.Get<string>("MESSAGE");

            Console.WriteLine("Message form local variables: {0}", message);

            return Task.CompletedTask;
        }
    }

    public class StepCommand : IPipelineCommand
    {
        public string Message { get; set; }
    }

    public class StepShowMessageFromCommand : IPipeline<StepCommand>
    {
        public Task Execute(StepCommand command, PipelineContext pipelineContext, CancellationToken cancellationToken)
        {
            Console.WriteLine("Message: {0}", command.Message);

            return Task.CompletedTask;
        }
    }

}

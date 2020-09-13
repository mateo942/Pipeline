using Microsoft.Extensions.DependencyInjection;
using Pipeline.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipeline.Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Run().ConfigureAwait(false);
            await RunDI().ConfigureAwait(false);

        }

        static async Task Run()
        {
            var configuration = new PipelineConfiguration()
                   .NextStep(new StepOne())
                   .NextStep(new StepGlobalVariable())
                   .NextStep(new StepShowMessageFromCommand(), new StepCommand() { Message = "Hello world" })
                   .NextStep(new StepLocalVariable(), new Variables { { "MESSAGE", "Hello from variables" } });

            var manager = new PipelineManager();

            await manager.Configure(configuration)
                .Run();
        }

        static async Task RunDI()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddPipeline();

            serviceCollection.AddTransient<StepOne>();
            serviceCollection.AddTransient<StepGlobalVariable>();
            serviceCollection.AddTransient<StepLocalVariable>();
            serviceCollection.AddTransient<StepShowMessageFromCommand>();
            serviceCollection.AddScoped<StepDiHelp>();
            serviceCollection.AddScoped<StepDI>();

            var provider = serviceCollection.BuildServiceProvider();

            var configuration = new PipelineConfiguration()
                   .NextStep<StepOne>()
                   .NextStep<StepGlobalVariable>()
                   .NextStep<StepShowMessageFromCommand, StepCommand>(new StepCommand() { Message = "Hello world" })
                   .NextStep<StepLocalVariable>(new Variables { { "MESSAGE", "Hello from variables" } })
                   .NextStep<StepDI>();


            using (var scope = provider.CreateScope())
            {
                var help = scope.ServiceProvider.GetRequiredService<StepDiHelp>();
                help.SetMessage("Hello DI");

                var pipelineManager = scope.ServiceProvider.GetRequiredService<IPipelineManager>();

                await pipelineManager.Configure(configuration).Run();
            }

            await provider.DisposeAsync().ConfigureAwait(false);
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

    public class StepDiHelp 
    {
        private string message;

        public string GetMessage()
        {
            return message;
        }

        public void SetMessage(string value)
        {
            message = value;
        }
    }


    public class StepDI : IPipeline
    {
        private readonly StepDiHelp _stepDiHelp;

        public StepDI(StepDiHelp stepDiHelp)
        {
            _stepDiHelp = stepDiHelp;
        }

        public Task Execute(PipelineContext pipelineContext, CancellationToken cancellationToken)
        {
            Console.WriteLine("Message form DI: {0}", _stepDiHelp.GetMessage());

            return Task.CompletedTask;
        }
    }

}

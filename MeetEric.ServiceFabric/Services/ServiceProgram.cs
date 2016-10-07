namespace MeetEric.Services
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading.Tasks;
    using Actors;
    using Common;
    using Diagnostics;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Threading.Tasks;

    public class ServiceProgram
    {
        static ServiceProgram()
        {
            FactoryInitializer.Initialize();
        }

        protected static void RunStateless<T>(Func<StatelessServiceContext, T> factory)
            where T : StatelessService
        {
            Run<T>(new StatelessServiceRunner<T>(factory));
        }

        protected static void RunStateful<T>(Func<StatefulServiceContext, T> factory)
            where T : StatefulService
        {
            Run<T>(new StatefulServiceRunner<T>(factory));
        }

        protected static async Task RegisterActor<TFactory, T>()
            where TFactory : ActorFactory<T>, new()
            where T : ActorBase
        {
            await ActorRuntime.RegisterActorAsync<T>((context, actorType) => new MeetEricActorService(context, actorType, (service, id) => new TFactory().CreateActor(service, id)));
        }

        protected static void RegisterActors(params Func<Task>[] actors)
        {
            Run<ActorRunner>(new ActorRunner(actors));
        }

        private static void Run<T>(IServiceRunner serviceRunner)
        {
            try
            {
                MeetEricTask.Run(async () => await RunAsync(serviceRunner));
                serviceRunner.BlockProcessFromExit();
            }
            catch (Exception ex)
            {
                new ServiceFabricLog<T>().LogException(ex);

                // cause the program to crash to so events are visible in the event log
                throw;
            }
        }

        private static async Task RunAsync(IServiceRunner runner)
        {
            var serviceName = runner.ServiceName;
            var log = MeetEricFactory.GetService<ILoggingService>().CreateLoggingContext();
            var context = new Dictionary<string, string>()
            {
                { "Name", serviceName }
            };

            try
            {
                log.LogEvent("RegisteringService", context);
                await runner.RegisterService();
                log.LogEvent("RegisteredService", context);
            }
            catch (Exception e)
            {
                log.LogException(e, context);
                throw;
            }
        }
    }
}

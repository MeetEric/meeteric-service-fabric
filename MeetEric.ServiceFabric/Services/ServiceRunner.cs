namespace MeetEric.Services
{
    using System;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Diagnostics;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal abstract class ServiceRunner<TService, TContext> : IServiceRunner
        where TContext : ServiceContext
    {
        protected ServiceRunner(Func<TContext, TService> factory)
        {
            Factory = factory;
            ServiceName = typeof(TService).Name;
        }

        public string ServiceName { get; }

        private Func<TContext, TService> Factory { get; }

        public async Task RegisterService()
        {
            await RegisterService($"{ServiceName}Type", ConfigureService);
        }

        public virtual void BlockProcessFromExit()
        {
            Thread.Sleep(Timeout.Infinite);
        }

        protected abstract Task RegisterService(string name, Func<TContext, TService> factory);

        private TService ConfigureService(TContext serviceContext)
        {
            MeetEricFactory.RegisterService<IVersionService>(() => new StaticVersionService(serviceContext.CodePackageActivationContext.CodePackageVersion));
            MeetEricFactory.RegisterService<IWatchdogLoggingFactory>(() => new ServiceFabricWatchdogLogFactory(new FabricClient(), serviceContext));
            return Factory(serviceContext);
        }
    }
}

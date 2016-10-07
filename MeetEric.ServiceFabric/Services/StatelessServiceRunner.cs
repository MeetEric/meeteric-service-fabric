namespace MeetEric.Services
{
    using System;
    using System.Fabric;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class StatelessServiceRunner<T> : ServiceRunner<T, StatelessServiceContext>
        where T : StatelessService
    {
        public StatelessServiceRunner(Func<StatelessServiceContext, T> factory)
            : base(factory)
        {
        }

        protected override async Task RegisterService(string name, Func<StatelessServiceContext, T> factory)
        {
            await ServiceRuntime.RegisterServiceAsync($"{ServiceName}Type", factory);
        }
    }
}

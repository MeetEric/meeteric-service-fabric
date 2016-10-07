namespace MeetEric.Services
{
    using System;
    using System.Fabric;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Runtime;

    internal class StatefulServiceRunner<T> : ServiceRunner<T, StatefulServiceContext>
        where T : StatefulService
    {
        public StatefulServiceRunner(Func<StatefulServiceContext, T> factory)
            : base(factory)
        {
        }

        protected override async Task RegisterService(string name, Func<StatefulServiceContext, T> factory)
        {
            await ServiceRuntime.RegisterServiceAsync($"{ServiceName}Type", factory);
        }
    }
}

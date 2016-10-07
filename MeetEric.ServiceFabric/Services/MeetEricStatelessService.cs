namespace MeetEric.Services
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Health;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Diagnostics;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public abstract class MeetEricStatelessService : StatelessService
    {
        protected MeetEricStatelessService(StatelessServiceContext serviceContext)
            : base(serviceContext)
        {
            Log = MeetEricFactory.GetService<ILoggingService>().CreateLoggingContext();
            Cluster = new FabricClient();
        }

        protected ILoggingContext Log { get; private set; }

        protected FabricClient Cluster { get; }

        protected sealed override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ReportHealth(typeof(MeetEricStatelessService).Name, HealthState.Ok);
                await RunServiceAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
                await ReportHealth(typeof(MeetEricStatelessService).Name, HealthState.Error);
            }
        }

        protected async Task ReportHealth(string context, HealthState state)
        {
            var details = new HealthInformation("MeetEric", context, state);
            var report = new StatelessServiceInstanceHealthReport(Context.PartitionId, Context.ReplicaOrInstanceId, details);
            Cluster.HealthManager.ReportHealth(report);
            await Task.Yield();
        }

        protected abstract Task RunServiceAsync(CancellationToken cancellationToken);
    }
}

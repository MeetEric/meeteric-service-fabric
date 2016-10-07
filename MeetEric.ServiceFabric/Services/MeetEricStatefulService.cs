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

    public abstract class MeetEricStatefulService : StatefulService
    {
        protected MeetEricStatefulService(StatefulServiceContext serviceContext)
            : base(serviceContext)
        {
            MeetEricFactory.RegisterService<IVersionService>(() => new StaticVersionService(serviceContext.CodePackageActivationContext.CodePackageVersion));
            Log = MeetEricFactory.GetService<ILoggingService>().CreateLoggingContext();
            Cluster = new FabricClient();
        }

        protected MeetEricStatefulService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica)
            : base(serviceContext, reliableStateManagerReplica)
        {
            Log = MeetEricFactory.GetService<ILoggingService>().CreateLoggingContext();
            Cluster = new FabricClient();
        }

        protected bool IsPrimary { get; private set; }

        protected ILoggingContext Log { get; private set; }

        protected FabricClient Cluster { get; }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        protected sealed override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ReportHealth(typeof(MeetEricStatefulService).Name, HealthState.Ok);
                await RunServiceAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
                await ReportHealth(typeof(MeetEricStatefulService).Name, HealthState.Error);
            }
        }

        protected async Task ReportHealth(string context, HealthState state)
        {
            var details = new HealthInformation("MeetEric", context, state);
            var report = new StatefulServiceReplicaHealthReport(Context.PartitionId, Context.ReplicaId, details);
            Cluster.HealthManager.ReportHealth(report);
            await Task.Yield();
        }

        protected override Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            IsPrimary = newRole == ReplicaRole.Primary;
            return base.OnChangeRoleAsync(newRole, cancellationToken);
        }

        protected int GetEndpointPort(string endpointName)
        {
            var serviceEndpoint = this.Context.CodePackageActivationContext.GetEndpoint(endpointName);
            return serviceEndpoint.Port;
        }

        protected abstract Task RunServiceAsync(CancellationToken cancellationToken);
    }
}

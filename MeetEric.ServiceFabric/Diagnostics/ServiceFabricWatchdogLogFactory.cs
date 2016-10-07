namespace MeetEric.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Health;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Security;
    using static System.Fabric.FabricClient;

    public class ServiceFabricWatchdogLogFactory : IWatchdogLoggingFactory
    {
        public ServiceFabricWatchdogLogFactory(FabricClient fabric, ServiceContext context)
        {
            Fabric = fabric;
            Node = context.NodeContext;
        }

        private FabricClient Fabric { get; }

        private NodeContext Node { get; }

        public IWatchdogContext CreateWatchdogContext()
        {
            return new ServiceFabricContext(Fabric, Node);
        }

        private class ServiceFabricContext : IWatchdogContext
        {
            public ServiceFabricContext(FabricClient fabric, NodeContext node)
            {
                Node = new ServiceFabricNodeContext(fabric.HealthManager, node);
            }

            public IWatchdogNodeContext Node { get; }
        }

        private class ServiceFabricNodeContext : IWatchdogNodeContext
        {
            public ServiceFabricNodeContext(HealthClient health, NodeContext node)
            {
                Health = health;
                NodeName = node.NodeName;
            }

            private HealthClient Health { get; }

            private string NodeName { get; }

            public async Task ReportError(IIdentifier id, string message)
            {
                await ReportHealth(id, message, HealthState.Error);
            }

            public async Task ReportInformation(IIdentifier id, string message)
            {
                await ReportHealth(id, message, HealthState.Ok);
            }

            public async Task ReportWarning(IIdentifier id, string message)
            {
                await ReportHealth(id, message, HealthState.Warning);
            }

            private Task ReportHealth(IIdentifier id, string message, HealthState state)
            {
                var details = new HealthInformation("MeetEric", id.Moniker, state);
                details.Description = message;
                var report = new NodeHealthReport(NodeName, details);
                Health.ReportHealth(report);
                return Task.FromResult(0);
            }
        }
    }
}

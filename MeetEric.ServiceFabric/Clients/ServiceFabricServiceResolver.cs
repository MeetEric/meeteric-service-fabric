namespace MeetEric.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.ServiceFabric.Services.Client;
    using Serialization;

    public class ServiceFabricServiceResolver
    {
        public ServiceFabricServiceResolver()
        {
            Serializer = MeetEricFactory.GetService<ISerializationService>();
            Resolver = ServicePartitionResolver.GetDefault();
        }

        private ServicePartitionResolver Resolver { get; }

        private ISerializationService Serializer { get; }

        private ResolvedServicePartition ActivePartition { get; set; }

        public async Task<Uri> GetService(Uri serviceUri, long basefileKey, CancellationToken cancelToken)
        {
            var partitionKey = new ServicePartitionKey(basefileKey);

            if (ActivePartition != null)
            {
                ActivePartition = await Resolver.ResolveAsync(ActivePartition, cancelToken);
            }
            else
            {
                ActivePartition = await Resolver.ResolveAsync(serviceUri, partitionKey, CancellationToken.None);
            }

            var endPoint = ActivePartition.Endpoints
                .Where(x => x.Role == ServiceEndpointRole.StatefulPrimary)
                .Select(x => Serializer.Deserialize<EndPointCollection>(x.Address))
                .FirstOrDefault();

            return new Uri(endPoint.Endpoints.Values.First());
        }

        [DataContract]
        private class EndPointCollection
        {
            [DataMember]
            public Dictionary<string, string> Endpoints { get; private set; }
        }
    }
}

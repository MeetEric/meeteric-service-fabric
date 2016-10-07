namespace MeetEric.Services
{
    using System;
    using System.Fabric;
    using Common;
    using Diagnostics;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public class MeetEricActorService : ActorService
    {
        public MeetEricActorService(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null)
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            MeetEricFactory.RegisterService<IVersionService>(() => new StaticVersionService(context.CodePackageActivationContext.CodePackageVersion));
            MeetEricFactory.RegisterService<IWatchdogLoggingFactory>(() => new ServiceFabricWatchdogLogFactory(new FabricClient(), context));
        }
    }
}

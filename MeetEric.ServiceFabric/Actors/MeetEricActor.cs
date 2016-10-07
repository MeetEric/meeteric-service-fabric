namespace MeetEric.BaseFiles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MeetEric.BaseFiles;
    using MeetEric.Common;
    using MeetEric.Messaging;
    using Collections;
    using Diagnostics;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Security;

    public abstract class MeetEricActor : Actor
    {
        public MeetEricActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            ReliableObjects = new ReliableActorObjectFactory(() => this.StateManager);
            CancelSource = new CancellationTokenSource();
            Log = MeetEricFactory.GetService<ILoggingService>().CreateLoggingContext();
        }

        protected IReliableObjectFactory ReliableObjects { get; }

        protected CancellationTokenSource CancelSource { get; }

        protected ILoggingContext Log { get; }

        protected override Task OnDeactivateAsync()
        {
            Log.LogCancel($"Actor '{this.Id}' is being deactivated");
            CancelSource.Cancel();
            return base.OnDeactivateAsync();
        }
    }
}

namespace MeetEric.Actors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Common;
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public abstract class ServiceActor<T> : Actor
        where T : class
    {
        private readonly Lazy<T> _lazyService;

        protected ServiceActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _lazyService = new Lazy<T>(MeetEricFactory.GetService<T>);
        }

        protected T Service
        {
            get
            {
                return _lazyService.Value;
            }
        }
    }
}

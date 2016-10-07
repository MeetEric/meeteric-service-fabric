namespace MeetEric.Actors
{
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;

    public abstract class ActorFactory<T>
         where T : ActorBase
    {
        public abstract T CreateActor(ActorService service, ActorId id);
    }
}

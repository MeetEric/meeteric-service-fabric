namespace MeetEric.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class ActorRunner : IServiceRunner
    {
        public ActorRunner(params Func<Task>[] actors)
        {
            Actors = actors;
            ServiceName = this.GetType().Name;
        }

        public string ServiceName { get; }

        private IList<Func<Task>> Actors { get; }

        public async Task RegisterService()
        {
            foreach (var actor in Actors)
            {
                await actor();
            }
        }

        public virtual void BlockProcessFromExit()
        {
            Thread.Sleep(Timeout.Infinite);
        }
    }
}

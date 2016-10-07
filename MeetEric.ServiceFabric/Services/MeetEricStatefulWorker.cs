namespace MeetEric.Services
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Messaging;

    public abstract class MeetEricStatefulWorker : MeetEricStatefulService
    {
        protected MeetEricStatefulWorker(StatefulServiceContext serviceContext)
            : base(serviceContext)
        {
        }

        protected override async Task RunServiceAsync(CancellationToken cancellationToken)
        {
            var worker = await CreateWorker();
            await worker.Start();
            await worker.Run(cancellationToken);
        }

        protected abstract Task<IWorkerRole> CreateWorker();
    }
}

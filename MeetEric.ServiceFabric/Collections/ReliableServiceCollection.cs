namespace MeetEric.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Diagnostics;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Threading.Tasks;

    public class ReliableServiceCollection<TValue>
    {
        public ReliableServiceCollection(ITransaction tx, IReliableCollection<TValue> data, ILoggingContext log)
        {
            Data = data;
            Tx = tx;
            Log = log;
        }

        public Task<long> Count
        {
            get
            {
                using (TimedEvent.LongOperation("Count", Log))
                {
                    return Data.GetCountAsync(Tx);
                }
            }
        }

        protected ITransaction Tx { get; }

        protected bool HasChanges { get; set; }

        protected ILoggingContext Log { get; }

        private IReliableCollection<TValue> Data { get; }

        public async Task Clear(CancellationToken cancelToken)
        {
            using (TimedEvent.Log("Clear", typeof(TValue).Name, Log, skipStart: true))
            {
                await Data.ClearAsync();
            }
        }

        public void Dispose()
        {
            if (HasChanges)
            {
                MeetEricTask.Run(async () => await Save(CancellationToken.None));
            }

            Tx.Dispose();
        }

        public async Task Save(CancellationToken cancelToken)
        {
            using (TimedEvent.Log("Save", typeof(TValue).Name, Log, skipStart: true))
            {
                if (HasChanges)
                {
                    using (TimedEvent.Log("Commit", typeof(TValue).Name, Log, skipStart: true))
                    {
                        HasChanges = false;
                        await Tx.CommitAsync();
                    }
                }
            }
        }
    }
}

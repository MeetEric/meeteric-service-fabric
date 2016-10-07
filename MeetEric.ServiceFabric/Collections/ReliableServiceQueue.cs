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

    public class ReliableServiceQueue<TValue> : ReliableServiceCollection<TValue>, IAsyncQueue<TValue>
    {
        public ReliableServiceQueue(ITransaction tx, IReliableQueue<TValue> data, ILoggingContext log)
            : base(tx, data, log)
        {
            Data = data;
        }

        private IReliableQueue<TValue> Data { get; }

        public async Task<TValue> Dequeue(CancellationToken cancel)
        {
            var result = await Data.TryDequeueAsync(Tx);

            if (result.HasValue)
            {
                HasChanges = true;
            }

            return result.Value;
        }

        public async Task Enqueue(TValue item, CancellationToken cancel)
        {
            await Data.EnqueueAsync(Tx, item);
            HasChanges = true;
        }

        public async Task<TValue> Peek(CancellationToken cancel)
        {
            var result = await Data.TryPeekAsync(Tx);
            return result.Value;
        }
    }
}

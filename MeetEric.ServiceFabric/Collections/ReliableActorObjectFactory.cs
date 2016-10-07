namespace MeetEric.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MeetEric.Security;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Threading.Tasks;

    public class ReliableActorObjectFactory : IReliableObjectFactory
    {
        public ReliableActorObjectFactory(Func<IActorStateManager> manager)
        {
            StateManager = manager;
            Dictionaries = new ConcurrentDictionary<IIdentifier, IAsyncCollection>();
            Queues = new ConcurrentDictionary<IIdentifier, IAsyncCollection>();
            Sets = new ConcurrentDictionary<IIdentifier, IAsyncCollection>();
        }

        private Func<IActorStateManager> StateManager { get; }

        private IDictionary<IIdentifier, IAsyncCollection> Sets { get; }

        private IDictionary<IIdentifier, IAsyncCollection> Dictionaries { get; }

        private IDictionary<IIdentifier, IAsyncCollection> Queues { get; }

        public async Task<IAsyncDictionary<TKey, TValue>> GetDictionary<TKey, TValue>(IIdentifier id, CancellationToken cancel)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            IAsyncDictionary<TKey, TValue> result = null;

            if (!Dictionaries.ContainsKey(id))
            {
                result = await TransactionalDictionary<TKey, TValue>.Create(id, StateManager(), cancel);
                Dictionaries[id] = result;
            }
            else
            {
                result = (IAsyncDictionary<TKey, TValue>)Dictionaries[id];
            }

            return result;
        }

        public async Task<IAsyncQueue<TItem>> GetQueue<TItem>(IIdentifier id, CancellationToken cancel)
        {
            IAsyncQueue<TItem> result = null;

            if (!Dictionaries.ContainsKey(id))
            {
                result = await TransactionalQueue<TItem>.Create(id, StateManager(), cancel);
                Queues[id] = result;
            }
            else
            {
                result = (IAsyncQueue<TItem>)Queues[id];
            }

            return result;
        }

        public Task<IAsyncSet<TValue>> GetSet<TValue>(IIdentifier id, CancellationToken cancel)
            where TValue : IComparable<TValue>, IEquatable<TValue>
        {
            throw new NotImplementedException();
        }
    }
}

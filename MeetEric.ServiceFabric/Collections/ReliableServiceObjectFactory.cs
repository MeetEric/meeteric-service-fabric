namespace MeetEric.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Diagnostics;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Security;

    public class ReliableServiceObjectFactory : IReliableObjectFactory
    {
        public ReliableServiceObjectFactory(IReliableStateManager stateManager, ILoggingContext log)
        {
            StateManager = stateManager;
            Log = log;
        }

        private IReliableStateManager StateManager { get; }

        private ILoggingContext Log { get; }

        public async Task<IAsyncDictionary<TKey, TValue>> GetDictionary<TKey, TValue>(IIdentifier id, CancellationToken cancel)
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<TKey, TValue>>(id.Moniker);
            return new ReliableServiceDictionary<TKey, TValue>(StateManager.CreateTransaction(), dictionary, Log);
        }

        public async Task<IAsyncQueue<TItem>> GetQueue<TItem>(IIdentifier id, CancellationToken cancel)
        {
            var queue = await StateManager.GetOrAddAsync<IReliableQueue<TItem>>(id.Moniker);
            return new ReliableServiceQueue<TItem>(StateManager.CreateTransaction(), queue, Log);
        }

        public async Task<IAsyncSet<TValue>> GetSet<TValue>(IIdentifier id, CancellationToken cancel)
            where TValue : IComparable<TValue>, IEquatable<TValue>
        {
            ITransaction tx = null;
            IReliableDictionary<TValue, bool> dictionary = null;

            using (TimedEvent.LongOperation("CreateTransaction", Log))
            {
                tx = StateManager.CreateTransaction();
            }

            using (TimedEvent.LongOperation("GetSet", Log))
            {
                dictionary = await StateManager.GetOrAddAsync<IReliableDictionary<TValue, bool>>($"{id.Moniker}-Set");
            }

            return new ReliableServiceDictionary<TValue, bool>(tx, dictionary, Log);
        }
    }
}

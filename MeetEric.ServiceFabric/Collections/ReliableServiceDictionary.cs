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

    public class ReliableServiceDictionary<TKey, TValue> : ReliableServiceCollection<KeyValuePair<TKey, TValue>>, IAsyncDictionary<TKey, TValue>, IAsyncSet<TKey>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public ReliableServiceDictionary(ITransaction tx, IReliableDictionary<TKey, TValue> data, ILoggingContext log)
            : base(tx, data, log)
        {
            Data = data;
        }

        private IReliableDictionary<TKey, TValue> Data { get; }

        public async Task Add(TKey key, TValue value, CancellationToken cancel)
        {
            using (TimedEvent.Log("Write", "Add", Log, skipStart: true))
            {
                await Data.AddAsync(Tx, key, value);
                HasChanges = true;
            }
        }

        public async Task<bool> ContainsKey(TKey key, CancellationToken cancel)
        {
            using (TimedEvent.Log("Read", "ContainsKey", Log, skipStart: true))
            {
                return await Data.ContainsKeyAsync(Tx, key);
            }
        }

        public async Task<TValue> Get(TKey key, CancellationToken cancel)
        {
            using (TimedEvent.Log("Read", "Get", Log, skipStart: true))
            {
                var result = await Data.TryGetValueAsync(Tx, key);
                return result.Value;
            }
        }

        public async Task<IEnumerable<KeyValuePair<TKey, TValue>>> GetItems(CancellationToken cancel)
        {
            using (TimedEvent.Log("Read", "GetItems", Log, skipStart: true))
            {
                var results = new List<KeyValuePair<TKey, TValue>>();
                var items = (await Data.CreateEnumerableAsync(Tx)).GetAsyncEnumerator();

                while (await items.MoveNextAsync(cancel))
                {
                    results.Add(items.Current);
                }

                return results;
            }
        }

        public async Task<IEnumerable<TKey>> GetKeys(CancellationToken cancel)
        {
            using (TimedEvent.Log("Read", "GetKeys", Log, skipStart: true))
            {
                return (await GetItems(cancel)).Select(x => x.Key);
            }
        }

        public async Task<bool> Remove(TKey key, CancellationToken cancel)
        {
            using (TimedEvent.Log("Write", "Remove", Log, skipStart: true))
            {
                var result = await Data.TryRemoveAsync(Tx, key);

                if (result.HasValue)
                {
                    HasChanges = true;
                }

                return result.HasValue;
            }
        }

        public async Task Set(TKey key, TValue value, CancellationToken token)
        {
            using (TimedEvent.Log("Write", "Set", Log, skipStart: true))
            {
                await Data.SetAsync(Tx, key, value);
                HasChanges = true;
            }
        }

        async Task<bool> IAsyncSet<TKey>.Add(TKey value, CancellationToken cancel)
        {
            bool result = !await Data.ContainsKeyAsync(Tx, value);

            if (result)
            {
                await Add(value, default(TValue), cancel);
            }

            return result;
        }

        Task<bool> IAsyncSet<TKey>.Contains(TKey value, CancellationToken cancel)
        {
            return ContainsKey(value, cancel);
        }

        async Task<IEnumerable<TKey>> IAsyncSet<TKey>.GetItems(CancellationToken cancel)
        {
            return (await GetItems(cancel)).Select(x => x.Key);
        }
    }
}

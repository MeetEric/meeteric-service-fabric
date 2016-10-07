namespace MeetEric.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Microsoft.ServiceFabric.Data;
    using Security;
    using Threading.Tasks;

    public class TransactionalDictionary<TKey, TValue> : IAsyncDictionary<TKey, TValue>
    {
        private TransactionalDictionary(IIdentifier id, IDictionary<TKey, TValue> dictionary, OnSave<IDictionary<TKey, TValue>> onSave)
        {
            Identity = id;
            Dictionary = dictionary;
            SaveHandler = onSave;
        }

        private delegate Task OnSave<TPayload>(IIdentifier id, TPayload payload, CancellationToken cancelToken);

        public Task<long> Count
        {
            get
            {
                return Task.FromResult<long>(Dictionary.Count);
            }
        }

        protected bool HasUnsavedChanges { get; set; }

        private IDictionary<TKey, TValue> Dictionary { get; }

        private IIdentifier Identity { get; }

        private OnSave<IDictionary<TKey, TValue>> SaveHandler { get; }

        public static async Task<IAsyncDictionary<TKey, TValue>> Create(IIdentifier id, IActorStateManager manager, CancellationToken token)
        {
            var state = await manager.TryGetStateAsync<IDictionary<TKey, TValue>>(id.Moniker, token);
            IDictionary<TKey, TValue> dictionary = state.HasValue ? state.Value : new Dictionary<TKey, TValue>();

            return new TransactionalDictionary<TKey, TValue>(id, dictionary, (saveId, payload, cancel) => manager.SetStateAsync(saveId.Moniker, payload, cancel));
        }

        public Task Clear(CancellationToken cancelToken)
        {
            if (Dictionary.Count > 0)
            {
                HasUnsavedChanges = true;
            }

            Dictionary.Clear();
            return Task.FromResult(0);
        }

        public async Task Save(CancellationToken cancelToken)
        {
            if (HasUnsavedChanges)
            {
                await SaveHandler(Identity, Dictionary, cancelToken);
                HasUnsavedChanges = false;
            }
        }

        public void Dispose()
        {
            if (HasUnsavedChanges)
            {
                MeetEricTask.Run(() => Save(CancellationToken.None));
            }
        }

        public Task Add(TKey key, TValue value, CancellationToken cancel)
        {
            Dictionary.Add(key, value);
            HasUnsavedChanges = true;
            return Task.FromResult(0);
        }

        public Task Set(TKey key, TValue value, CancellationToken cancel)
        {
            Dictionary[key] = value;
            HasUnsavedChanges = true;
            return Task.FromResult(0);
        }

        public Task<TValue> Get(TKey key, CancellationToken cancel)
        {
            TValue result = default(TValue);
            Dictionary.TryGetValue(key, out result);
            return Task.FromResult(result);
        }

        public Task<bool> Remove(TKey key, CancellationToken cancel)
        {
            bool result = Dictionary.Remove(key);

            if (result)
            {
                HasUnsavedChanges = true;
            }

            return Task.FromResult(result);
        }

        public Task<IEnumerable<KeyValuePair<TKey, TValue>>> GetItems(CancellationToken cancel)
        {
            IEnumerable<KeyValuePair<TKey, TValue>> result = Dictionary;
            return Task.FromResult(result);
        }

        public Task<bool> ContainsKey(TKey key, CancellationToken cancel)
        {
            var result = Dictionary.ContainsKey(key);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<TKey>> GetKeys(CancellationToken cancel)
        {
            IEnumerable<TKey> keys = Dictionary.Keys.ToList();
            return Task.FromResult(keys);
        }
    }
}

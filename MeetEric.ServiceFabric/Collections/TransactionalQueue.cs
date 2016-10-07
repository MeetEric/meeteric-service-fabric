namespace MeetEric.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Security;
    using Threading.Tasks;

    public class TransactionalQueue<T> : IAsyncQueue<T>
    {
        private TransactionalQueue(IIdentifier id, Queue<T> queue, OnSave<Queue<T>> onSave)
        {
            Identity = id;
            Queue = queue;
            SaveHandler = onSave;
        }

        private delegate Task OnSave<TPayload>(IIdentifier id, TPayload payload, CancellationToken cancelToken);

        public Task<long> Count
        {
            get
            {
                return Task.FromResult<long>(Queue.Count);
            }
        }

        protected bool HasUnsavedChanges { get; set; }

        private Queue<T> Queue { get; }

        private IIdentifier Identity { get; }

        private OnSave<Queue<T>> SaveHandler { get; }

        public static async Task<IAsyncQueue<T>> Create(IIdentifier id, IActorStateManager manager, CancellationToken token)
        {
            var state = await manager.TryGetStateAsync<Queue<T>>(id.Moniker, token);
            Queue<T> queue = state.HasValue ? state.Value : new Queue<T>();

            return new TransactionalQueue<T>(id, queue, (saveId, payload, cancel) => manager.SetStateAsync(saveId.Moniker, payload, cancel));
        }

        public Task Clear(CancellationToken cancelToken)
        {
            if (Queue.Count > 0)
            {
                HasUnsavedChanges = true;
            }

            Queue.Clear();
            return Task.FromResult(0);
        }

        public async Task Save(CancellationToken cancelToken)
        {
            await SaveHandler(Identity, Queue, cancelToken);
            HasUnsavedChanges = false;
        }

        public void Dispose()
        {
            if (HasUnsavedChanges)
            {
                MeetEricTask.Run(() => Save(CancellationToken.None));
            }
        }

        public Task Enqueue(T item, CancellationToken cancel)
        {
            Queue.Enqueue(item);
            HasUnsavedChanges = true;
            return Task.FromResult(0);
        }

        public Task<T> Dequeue(CancellationToken cancel)
        {
            T result = default(T);

            if (Queue.Count > 0)
            {
                result = Queue.Dequeue();
                HasUnsavedChanges = true;
            }

            return Task.FromResult(result);
        }

        public Task<T> Peek(CancellationToken cancel)
        {
            T result = default(T);

            if (Queue.Count > 0)
            {
                result = Queue.Peek();
            }

            return Task.FromResult(result);
        }
    }
}

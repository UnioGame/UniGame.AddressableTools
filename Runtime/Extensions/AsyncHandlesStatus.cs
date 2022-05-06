using System;
using System.Collections.Generic;
using System.Linq;
using UniModules.UniCore.Runtime.DataFlow;
using UniModules.UniCore.Runtime.ObjectPool.Runtime.Extensions;
using UniModules.UniCore.Runtime.Rx.Extensions;
using UniModules.UniGame.Core.Runtime.Rx;
using UniRx;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UniModules.UniGame.SerializableContext.Runtime.Addressables
{
    public class AsyncHandlesStatus : IPoolableAsyncHandleStatus
    {
        private LifeTimeDefinition _lifeTime;
        private long _totalBytes;
        private List<IAsyncHandleStatus> _handles;
        private RecycleReactiveProperty<IAsyncHandleStatus> _value = new RecycleReactiveProperty<IAsyncHandleStatus>();

        /// <summary>
        /// Returns the computed percent complete as a float value between 0 &amp; 1.  If TotalBytes == 0, 1 is returned.
        /// </summary>
        public float Percent => (TotalBytes > 0) ? (DownloadedBytes / (float)TotalBytes) : (IsDone ? 1.0f : 0f);

        public AsyncOperationStatus Status
        {
            get
            {
                var doneCounter = 0;
                foreach (var handle in _handles)
                {
                    switch (handle.Status)
                    {
                        case AsyncOperationStatus.Succeeded:
                            doneCounter++;
                            continue;
                        case AsyncOperationStatus.Failed:
                            return AsyncOperationStatus.Failed;
                    }
                }

                return doneCounter == _handles.Count ? 
                    AsyncOperationStatus.Succeeded : 
                    AsyncOperationStatus.None;
            }
        }

        public long TotalBytes => _totalBytes;
        public long DownloadedBytes => _handles.Sum(x => x.DownloadedBytes);
        public bool IsDone => _handles.All(x => x.IsDone);

        public Exception OperationException =>
            _handles.FirstOrDefault(x => x.OperationException != null)?.OperationException;

        public void DespawnHandleStatus()
        {
            foreach (var handle in _handles)
            {
                if(handle is IPoolableAsyncHandleStatus poolableAsyncHandleStatus)
                    poolableAsyncHandleStatus.Despawn();
            }
            _handles.Clear();
            
            this.Despawn();
        }       
        
        public IPoolableAsyncHandleStatus BindToHandle(List<IAsyncHandleStatus> handles)
        {
            _lifeTime ??= new LifeTimeDefinition();
            
            Release();
            
            _value.AddTo(_lifeTime);
            _totalBytes = 0;
            foreach (var handle in handles)
            {
                _totalBytes += handle.TotalBytes;
            }

            _handles.CombineLatest()
                .RxSubscribe(x => _value.SetValueForce(this))
                .AddTo(_lifeTime);

            return this;
        }

        public IDisposable Subscribe(IObserver<IAsyncHandleStatus> observer)
        {
            if (_handles == null)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }

            return _value.Subscribe(observer);
        }

        
        public void Release()
        {
            _lifeTime?.Release();
            _handles = null;
            _totalBytes = 0;
        }

    }
}
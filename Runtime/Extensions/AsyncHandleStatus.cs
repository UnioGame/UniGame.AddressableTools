using System;
using UniModules.UniCore.Runtime.DataFlow;
using UniModules.UniCore.Runtime.ObjectPool.Runtime.Extensions;
using UniModules.UniCore.Runtime.Rx.Extensions;
using UniRx;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UniModules.UniGame.AddressableTools.Runtime.Extensions
{
    public struct HandleStatus
    {
        public AsyncOperationStatus Status;
        public long TotalBytes;
        public long DownloadedBytes;
        public bool IsDone;
        public Exception OperationException;
        
        public float Percent => (TotalBytes > 0) ? ((float)DownloadedBytes / (float)TotalBytes) : (IsDone ? 1.0f : 0f);
    }
    
    public class AsyncHandleStatus  : IPoolableAsyncHandleStatus
    {
        private readonly LifeTimeDefinition _lifeTime = new LifeTimeDefinition();
        
        private Subject<IAsyncHandleStatus> _status = new Subject<IAsyncHandleStatus>();
        private AsyncOperationHandle _handle;
        private DownloadStatus _statusValue;
        private Exception _exception;
        private AsyncOperationStatus _operationStatus;

        public long TotalBytes => _statusValue.TotalBytes;
        
        public long DownloadedBytes => _statusValue.DownloadedBytes;
        public bool IsDone => _statusValue.IsDone;
        public float Percent => _statusValue.Percent;

        public AsyncOperationStatus Status => _operationStatus;
        
        public Exception OperationException => _exception;
        
        public AsyncHandleStatus BindToHandle(AsyncOperationHandle handle)
        {
            Release();

            _status = new Subject<IAsyncHandleStatus>().AddTo(_lifeTime);
            _exception = null;
            _handle = handle;
            _statusValue = new DownloadStatus();

            Observable.EveryUpdate()
                .Subscribe(x => OnUpdate())
                .AddTo(_lifeTime);
            
            _handle.Completed += OnComplete;

            return this;
        }

        public void Release()
        {
            _lifeTime.Release();
        }

        public void DespawnHandleStatus()
        {
            this.Despawn();
        }

        private void OnUpdate()
        {
            //maybe some error?
            if (!_handle.IsValid())
                return;

            _operationStatus = _handle.Status;
            _statusValue = _handle.GetDownloadStatus();
            _exception = _handle.OperationException;
            _status.OnNext(this);
        }

        public IDisposable Subscribe(IObserver<IAsyncHandleStatus> observer)
        {
            return _status.Subscribe(observer);
        }

        private void OnComplete(AsyncOperationHandle handle)
        {
            OnUpdate();
            
            if (handle.OperationException != null)
                _status.OnError(handle.OperationException);
            else
            {
                _status.OnCompleted();
            }

            Release();
        }
        
    }
    
    public class AsyncHandleStatus<TObject>  : IPoolableAsyncHandleStatus
    {
        private readonly LifeTimeDefinition _lifeTime = new LifeTimeDefinition();
        
        private Subject<IAsyncHandleStatus> _status = new Subject<IAsyncHandleStatus>();
        private AsyncOperationHandle _handle;
        private DownloadStatus _statusValue;
        private Exception _exception;
        private AsyncOperationStatus _operationStatus;
        
        public long TotalBytes => _statusValue.TotalBytes;
        
        public long DownloadedBytes => _statusValue.DownloadedBytes;
        public bool IsDone => _statusValue.IsDone;
        public float Percent => _statusValue.Percent;

        public AsyncOperationStatus Status => _operationStatus;
        public Exception OperationException => _exception;
        
        public AsyncHandleStatus<TObject> BindToHandle(AsyncOperationHandle<TObject> handle)
        {
            Release();

            _status = new Subject<IAsyncHandleStatus>().AddTo(_lifeTime);
            _exception = null;
            _handle = handle;
            _statusValue = new DownloadStatus();

            Observable.EveryUpdate()
                .Subscribe(x => OnUpdate())
                .AddTo(_lifeTime);
            
            _handle.Completed += OnComplete;

            return this;
        }

        public void Release()
        {
            _lifeTime.Release();
        }

        private void OnUpdate()
        {
            //maybe some error?
            if (!_handle.IsValid())
                return;

            _operationStatus = _handle.Status;
            _statusValue = _handle.GetDownloadStatus();
            _exception = _handle.OperationException;
            _status.OnNext(this);
        }

        public void DespawnHandleStatus()
        {
            this.Despawn();
        }
        
        public IDisposable Subscribe(IObserver<IAsyncHandleStatus> observer)
        {
            return _status.Subscribe(observer);
        }

        private void OnComplete(AsyncOperationHandle handle)
        {
            OnUpdate();
            
            if (handle.OperationException != null)
                _status.OnError(handle.OperationException);
            else
            {
                _status.OnCompleted();
            }

            Release();
        }
        
    }
}
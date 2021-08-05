using System;
using UniModules.UniCore.Runtime.DataFlow;
using UniModules.UniCore.Runtime.Rx.Extensions;
using UniRx;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UniModules.UniGame.AddressableTools.Runtime.Extensions
{
    public class AsyncHandleDownloadHandle  : IAsyncHandleStatus
    {
        private readonly LifeTimeDefinition _lifeTime = new LifeTimeDefinition();
        
        private Subject<IAsyncHandleStatus> _status = new Subject<IAsyncHandleStatus>();
        private AsyncOperationHandle _handle;
        private DownloadStatus _statusValue;
        private Exception _exception;

        public long TotalBytes => _statusValue.TotalBytes;
        
        public long DownloadedBytes => _statusValue.DownloadedBytes;
        public bool IsDone => _statusValue.IsDone;
        public float Percent => _statusValue.Percent;

        public Exception OperationException => _exception;
        
        public void BindToHandle(AsyncOperationHandle handle)
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
    
    public class AsyncHandleDownloadHandle<TObject>  : IAsyncHandleStatus
    {
        private readonly LifeTimeDefinition _lifeTime = new LifeTimeDefinition();
        
        private Subject<IAsyncHandleStatus> _status = new Subject<IAsyncHandleStatus>();
        private AsyncOperationHandle _handle;
        private DownloadStatus _statusValue;
        private Exception _exception;

        public long TotalBytes => _statusValue.TotalBytes;
        
        public long DownloadedBytes => _statusValue.DownloadedBytes;
        public bool IsDone => _statusValue.IsDone;
        public float Percent => _statusValue.Percent;

        public Exception OperationException => _exception;
        
        public void BindToHandle(AsyncOperationHandle<TObject> handle)
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
}
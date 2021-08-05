using System;
using UniModules.UniCore.Runtime.ObjectPool.Runtime.Interfaces;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UniModules.UniGame.AddressableTools.Runtime.Extensions
{
    public interface IAsyncHandleStatus : IObservable<IAsyncHandleStatus>
    {
        AsyncOperationStatus Status{ get;}
        
        long TotalBytes { get; }
     
        long DownloadedBytes { get; }
        
        bool  IsDone { get; }
        
        float Percent { get; }
        
        Exception OperationException { get; }
    }

    public interface IPoolableAsyncHandleStatus : 
        IAsyncHandleStatus,
        IPoolable
    {
        void DespawnHandleStatus();
    }
    
    
}
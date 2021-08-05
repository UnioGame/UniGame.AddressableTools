using System;

namespace UniModules.UniGame.AddressableTools.Runtime.Extensions
{
    public interface IAsyncHandleStatus : IObservable<IAsyncHandleStatus>
    {
        long TotalBytes { get; }
     
        long DownloadedBytes { get; }
        
        bool  IsDone { get; }
        
        float Percent { get; }
        
        Exception OperationException { get; }
    }
}
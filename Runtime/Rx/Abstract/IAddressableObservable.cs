namespace UniGame.Addressables.Reactive
{
    using System;
    using UniRx;
    using UnityEngine.ResourceManagement.AsyncOperations;

    public interface IAddressableObservable<TData> : 
        IObservable<TData>, 
        IDisposable
    {
        IReadOnlyReactiveProperty<TData> Value { get; }
    }
    
}
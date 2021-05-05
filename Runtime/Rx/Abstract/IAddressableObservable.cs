using UnityEngine.AddressableAssets;

namespace UniGame.Addressables.Reactive
{
    using System;
    using UniRx;
    using UnityEngine.ResourceManagement.AsyncOperations;

    public interface IAddressableObservable<TData> : 
        IObservable<TData>, 
        IDisposable
    {
        #region unity editor

        public AssetReference AssetReference { get; }

        #endregion
        
    }
    
}
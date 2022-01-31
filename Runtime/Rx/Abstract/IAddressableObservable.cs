using System;
using UnityEngine.AddressableAssets;

namespace UniGame.Addressables.Reactive.Abstract
{
    public interface IAddressableObservable<TData> : 
        IObservable<TData>, 
        IDisposable
    {
        #region unity editor

        public AssetReference AssetReference { get; }

        #endregion
        
    }
    
}
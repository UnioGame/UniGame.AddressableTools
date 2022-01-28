using System;
using UnityEngine.AddressableAssets;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.Rx.Abstract
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
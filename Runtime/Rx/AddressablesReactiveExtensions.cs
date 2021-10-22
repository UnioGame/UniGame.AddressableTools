namespace UniGame.Addressables.Reactive
{
    using UniModules.UniCore.Runtime.ObjectPool.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

    public static class AddressablesReactiveExtensions 
    {
        public static IAddressableObservable<TApi> ToObservable<TData,TApi>(this AssetReferenceT<TData> reference) 
            where TData : Object , TApi
            where TApi : class
        {
            var observable = new AddressableObservable<AssetReferenceT<TData>, TData, TApi>(reference);
            return observable;
        }
        
        public static IAddressableObservable<TData> ToObservable<TData>(this AssetReferenceT<TData> reference) 
            where TData : Object
        {
            var observable = new AddressableObservable<AssetReferenceT<TData>, TData, TData>(reference);
            return observable;
        }
        
        public static IAddressableObservable<TApi> ToObservable<TApi>(this AssetReference reference) 
            where TApi : class
        {
            var observable = new AddressableObservable<AssetReference, Object, TApi>(reference);
            return observable;
        }
        
        public static IAddressableObservable<TApi> ToObservable<TApi>(this AssetReferenceGameObject reference) 
            where TApi : class
        {
            var observable = new AddressableObservable<AssetReference, GameObject,TApi>(reference);
            return observable;
        }
        
        public static IAddressableObservable<Object> ToObservable(this AssetReference reference)
        {
            var observable = new AddressableObservable<AssetReference, Object, Object>(reference);
            return observable;
        }

    }
}

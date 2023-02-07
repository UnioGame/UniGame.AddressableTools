using UniGame.Addressables.Reactive.Abstract;
using UniGame.Core.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UniGame.Addressables.Reactive
{
    using Object = UnityEngine.Object;

    public static class AddressablesReactiveExtensions 
    {
        public static IAddressableObservable<TApi> ToObservable<TData,TApi>(this AssetReferenceSprite reference,ILifeTime lifeTime) 
            where TData : Object , TApi
            where TApi : class
        {
            var observable = new AddressableObservable<AssetReferenceT<Sprite>, TData, TApi>(reference).AddTo(lifeTime);
            return observable;
        }
        
        public static IAddressableObservable<TApi> ToObservable<TData,TApi>(this AssetReferenceT<TData> reference,ILifeTime lifeTime) 
            where TData : Object , TApi
            where TApi : class
        {
            var observable = new AddressableObservable<AssetReferenceT<TData>, TData, TApi>(reference).AddTo(lifeTime);
            return observable;
        }
        
        public static IAddressableObservable<TData> ToObservable<TData>(this AssetReferenceT<TData> reference,ILifeTime lifeTime) 
            where TData : Object
        {
            var observable = new AddressableObservable<AssetReferenceT<TData>, TData, TData>(reference).AddTo(lifeTime);
            return observable;
        }
        
        public static IAddressableObservable<TApi> ToObservable<TApi>(this AssetReference reference,ILifeTime lifeTime) 
            where TApi : class
        {
            var observable = new AddressableObservable<AssetReference, Object, TApi>(reference).AddTo(lifeTime);
            return observable;
        }
        
        public static IAddressableObservable<TApi> ToObservable<TApi>(this AssetReferenceGameObject reference,ILifeTime lifeTime) 
            where TApi : class
        {
            var observable = new AddressableObservable<AssetReference, GameObject,TApi>(reference).AddTo(lifeTime);
            return observable;
        }
        
        public static IAddressableObservable<Object> ToObservable(this AssetReference reference,ILifeTime lifeTime)
        {
            var observable = new AddressableObservable<AssetReference, Object, Object>(reference).AddTo(lifeTime);
            return observable;
        }

    }
}

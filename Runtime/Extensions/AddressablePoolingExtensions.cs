
namespace UniGame.Context.Runtime
{
    using Cysharp.Threading.Tasks;
    using UniGame.Runtime.ObjectPool.Extensions;
    using AddressableTools.Runtime;
    using Core.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public static class AddressablePoolingExtensions
    {

        public static async UniTask<GameObject> CreatePool(this AssetReferenceT<GameObject> assetReference, ILifeTime lifeTime,int preload = 0)
        {
            var source = await assetReference.LoadAssetTaskAsync(lifeTime);
            source.AttachPoolToLifeTime(lifeTime, true, preload);
            return source;
        }
    
        public static async UniTask<TComponent> CreatePool<TComponent>(this AssetReferenceT<TComponent> assetReference, ILifeTime lifeTime,int preload = 0)
            where TComponent : Component
        {
            var source = await assetReference.LoadAssetTaskAsync(lifeTime);
            var sourceObject = source.gameObject;
            sourceObject.AttachPoolToLifeTime(lifeTime, true, preload);
            return source;
        }
    
        public static async UniTask<GameObject> Spawn(this AssetReferenceT<GameObject> assetReference, ILifeTime lifeTime)
        {
            var source = await assetReference.LoadAssetTaskAsync(lifeTime);
            return source.Spawn();
        }
    
        public static async UniTask<TComponent> Spawn<TComponent>(this AssetReferenceT<TComponent> assetReference, ILifeTime lifeTime)
            where TComponent : Component
        {
            var source = await assetReference.LoadAssetTaskAsync(lifeTime);
            return source.Spawn();
        }
    
    }
}
namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.Pooling
{
    using System;
    using Cysharp.Threading.Tasks;
    using global::UniGame.Runtime.ObjectPool.Extensions;
    using global::UniGame.AddressableTools.Runtime;
    using global::UniGame.Core.Runtime;
    using global::UniGame.Context.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public static class AddressablePoolingExtensions
    {
        public static readonly Vector3 WarmupPosition = new Vector3(10000,10000,10000);
        
        public static async UniTask AttachPoolLifeTimeAsync(this AssetReferenceT<GameObject> objectSource,ILifeTime lifeTime,int preloadCount = 0)
        {
            var source = await objectSource.LoadAssetTaskAsync(lifeTime);
            source.CreatePool(preloadCount);
            source.AttachPoolToLifeTime(lifeTime, true);
        }
        
        public static async UniTask<GameObject> SpawnAsync(this AssetReferenceT<GameObject> objectSource,
            ILifeTime lifeTime,
            Vector3 position,
            Quaternion rotation, 
            Transform parent = null, 
            bool stayPosition = false)
        {
            var source = await objectSource.LoadAssetTaskAsync(lifeTime);
            return source.Spawn(position,rotation,parent,stayPosition);
        }

        public static async UniTask<GameObject> WarmUpReference(this AssetReferenceGameObject view,
            ILifeTime lifeTime,
            int count = 0,
            bool activate = false,
            float killDelay = 0.5f)
        {
            return await WarmUp(view,lifeTime, count, activate, killDelay);
        }

        public static async UniTask<GameObject> WarmUp(this AssetReferenceT<GameObject> view,
            ILifeTime lifeTime,
            int count = 0,
            bool activate = false,
            float killDelay = 0.5f)
        {
            var asset = await view.CreatePool(lifeTime,count);
            if (activate && asset !=null)
            {
                WarmUpSource(asset,lifeTime,killDelay)
                    .AttachExternalCancellation(lifeTime.Token)
                    .Forget();
            }
            
            return asset;
        }
        
        public static async UniTask WarmUpSource(GameObject asset,ILifeTime lifeTime,float delay)
        {
            var pawn = asset.Spawn(WarmupPosition, Quaternion.identity);
            pawn.SetActive(true);
            
            await UniTask.Delay(TimeSpan.FromSeconds(delay))
                .AttachExternalCancellation(lifeTime.Token);
            
            if(pawn == null) return;
            
            pawn.Despawn();
        }
        
        public static async UniTask<GameObject> SpawnActiveAsync(
            this AssetReferenceT<GameObject> objectSource,
            ILifeTime lifeTime,
            Vector3 position,
            Quaternion rotation, 
            Transform parent = null, 
            bool stayPosition = false)
        {
            var source = await objectSource.LoadAssetTaskAsync(lifeTime);
            return source.Spawn(true,position,rotation,parent,stayPosition);
        }
        
        public static async UniTask<GameObject> SpawnActiveAsync(
            this AssetReferenceT<GameObject> objectSource,
            ILifeTime lifeTime, 
            Transform parent = null, 
            bool stayPosition = false)
        {
            var source = await objectSource.LoadAssetTaskAsync(lifeTime);
            var sourceTransform = source.transform;
            return source.Spawn(true,sourceTransform.position,sourceTransform.rotation,parent,stayPosition);
        }

        public static async UniTask<GameObject> SpawnAsync(this AssetReferenceT<GameObject> objectSource,
            ILifeTime lifeTime, 
            Transform parent = null, 
            bool stayPosition = false)
        {
            var source = await objectSource.LoadAssetTaskAsync(lifeTime);
            return source.Spawn(parent,stayPosition);
        }
    
        public static async UniTask<T> SpawnAsync<T>(this AssetReferenceT<T> objectSource,ILifeTime lifeTime, Transform parent = null, bool stayPosition = false)
            where T : Component
        {
            var source = await objectSource.LoadAssetTaskAsync(lifeTime);
            return source.Spawn(parent,stayPosition);
        }
        
        public static async UniTask<T> SpawnAsync<T>(this AssetReferenceT<T> objectSource,ILifeTime lifeTime,Vector3 position,Quaternion rotation, Transform parent = null, bool stayPosition = false)
            where T : Component
        {
            var source = await objectSource.LoadAssetTaskAsync(lifeTime);
            return source.Spawn(position,rotation,parent,stayPosition);
        }
        
        public static async UniTask<T> SpawnActiveAsync<T>(this AssetReferenceT<T> objectSource,ILifeTime lifeTime,Vector3 position,Quaternion rotation, Transform parent = null, bool stayPosition = false)
            where T : Component
        {
            var source = await objectSource.LoadAssetTaskAsync(lifeTime);
            return source.SpawnActive(position,rotation,parent,stayPosition);
        }
        
    }
}

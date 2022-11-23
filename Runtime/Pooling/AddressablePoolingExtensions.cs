﻿namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.Pooling
{
    using Cysharp.Threading.Tasks;
    using global::UniGame.Runtime.ObjectPool.Extensions;
    using global::UniGame.AddressableTools.Runtime;
    using global::UniGame.Core.Runtime;
    using global::UniGame.Context.Runtime;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public static class AddressablePoolingExtensions
    {
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

﻿using UniCore.Runtime.ProfilerTools;
using UniGame.Context.Runtime;

namespace UniGame.AddressableTools.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Cysharp.Threading.Tasks;
    using UniGame.Runtime.ObjectPool;
    using UniGame.Runtime.ObjectPool.Extensions;
    using Core.Runtime;
    using Core.Runtime.Extension;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Pool;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.ResourceManagement.ResourceLocations;
    using UnityEngine.ResourceManagement.ResourceProviders;
    using UnityEngine.SceneManagement;
    using Object = UnityEngine.Object;

#if UNITY_EDITOR
    using UnityEditor;
#endif
    

    public static class AddressableExtensions
    {
        private static object EvaluateKey(object obj)
        {
            if (obj is IKeyEvaluator evaluator)
                return evaluator.RuntimeKey;
            return obj;
        }

        private static HashSet<IResourceLocation> _resourceLocations = new();
        
        public static bool GetResourceLocations(object key, List<IResourceLocation> locations)
        {
            var resourceLocators = Addressables.ResourceLocators;
            var requiredType = typeof(Object);

            key = EvaluateKey(key);

            _resourceLocations.Clear();
            
            foreach (var locator in resourceLocators)
            {
                if (!locator.Locate(key, requiredType, out var locs))
                    continue;
                _resourceLocations.UnionWith(locs);
            }

            locations.AddRange(_resourceLocations);
            _resourceLocations.Clear();
            
            return true;
        }
        
        public static async UniTask<SceneInstance> LoadSceneTaskAsync(
            this AssetReference sceneReference,
            ILifeTime lifeTime,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single,
            bool activateOnLoad = true,
            int priority = 100)
        {
            if (sceneReference.RuntimeKeyIsValid() == false)
            {
                GameLog.LogError($"AssetReference key is NULL {sceneReference}");
                return default;
            }

            var scenePreviouslyRequested = sceneReference.OperationHandle.IsValid();
            var sceneHandle = scenePreviouslyRequested ?
                sceneReference.OperationHandle.Convert<SceneInstance>() :
                sceneReference.LoadSceneAsync(loadSceneMode, activateOnLoad, priority);
            
            //add to resource unloading
            sceneHandle.AddTo(lifeTime, scenePreviouslyRequested);

            await sceneHandle.ToUniTask();

            if (sceneHandle.Status == AsyncOperationStatus.Succeeded)
            {
                lifeTime.AddCleanUpAction(() => sceneReference.UnLoadScene());
            }

            return sceneHandle.Status == AsyncOperationStatus.Succeeded 
                ? sceneHandle.Result 
                : default;
        }

        public static void UnloadReference(this AssetReference reference)
        {
#if UNITY_EDITOR
            var targetAsset = reference.editorAsset;
            GameLog.Log($"UNLOAD AssetReference {targetAsset?.name} : {reference.AssetGUID}");
#endif
            // if(reference.Asset is IDisposable disposable)
            //     disposable.Dispose();
            //
            reference.ReleaseAsset();
        }

        public static async UniTask<List<TResult>> LoadScriptableAssetsTaskAsync<TResult>(
            this IEnumerable<AssetReference> assetReference,
            ILifeTime lifeTime)
            where TResult : class
        {
            var container = new List<TResult>();
            await assetReference.LoadAssetsTaskAsync<ScriptableObject, TResult, AssetReference>(container, lifeTime);
            return container;
        }
        
        public static async UniTask<IEnumerable<TSource>> LoadAssetsTaskAsync<TSource, TAsset>(
            this IEnumerable<TAsset> assetReference,
            List<TSource> resultContainer, ILifeTime lifeTime)
            where TAsset : AssetReference
            where TSource : Object
        {
            return await assetReference.LoadAssetsTaskAsync<TSource, TSource, TAsset>(resultContainer, lifeTime);
        }

        public static async UniTask<IEnumerable<TResult>> LoadAssetsTaskAsync<TSource, TResult, TAsset>(
            this IEnumerable<TAsset> assetReference,
            IList<TResult> resultContainer, 
            ILifeTime lifeTime)
            where TResult : class
            where TAsset : AssetReference
            where TSource : Object
        {
            var taskList = ClassPool.Spawn<List<UniTask<TSource>>>();

            foreach (var asset in assetReference)
            {
                var assetTask = asset.LoadAssetTaskAsync<TSource>(lifeTime);
                taskList.Add(assetTask);
            }

            var result = await UniTask.WhenAll(taskList);
            for (var j = 0; j < result.Length; j++)
            {
                if (result[j] is TResult item) resultContainer.Add(item);
            }

            taskList.Despawn();

            return resultContainer;
        }

        public static async UniTask<IReadOnlyList<TSource>> LoadAssetsTaskAsync<TSource, TAsset>(
            this IReadOnlyList<TAsset> assetReference,
            List<TSource> resultContainer, ILifeTime lifeTime)
            where TAsset : AssetReference
            where TSource : Object
        {
            return await assetReference.LoadAssetsTaskAsync<TSource, TSource, TAsset>(resultContainer, lifeTime);
        }

        public static async UniTask<IReadOnlyList<TResult>> LoadAssetsTaskAsync<TSource, TResult, TAsset>(
            this IReadOnlyList<TAsset> assetReference,
            List<TResult> resultContainer, ILifeTime lifeTime)
            where TResult : class
            where TAsset : AssetReference
            where TSource : Object
        {
            var taskList = ClassPool.Spawn<List<UniTask<TSource>>>();

            for (var i = 0; i < assetReference.Count; i++)
            {
                var asset = assetReference[i];
                var assetTask = asset.LoadAssetTaskAsync<TSource>(lifeTime);
                taskList.Add(assetTask);
            }

            var result = await UniTask.WhenAll(taskList);
            for (var j = 0; j < result.Length; j++)
            {
                if (result[j] is TResult item) resultContainer.Add(item);
            }

            taskList.Despawn();

            return resultContainer;
        }

        /// <summary>
        /// request local cache update by actual catalog
        /// </summary>
        /// <returns>list of updated ids</returns>
        public static async UniTask<List<string>> ResetAddressablesCacheForUpdatedContent(this object _)
        {
            var handle = Addressables.CheckForCatalogUpdates();
            var updatedIds = await handle.ToUniTask();
            if (updatedIds == null || updatedIds.Count == 0)
                return updatedIds;
            Addressables.ClearDependencyCacheAsync(updatedIds);
            return updatedIds;
        }

        public static async UniTask<T> LoadAssetInstanceTaskAsync<T>(this AssetReferenceT<T> assetReference,
            ILifeTime lifeTime,
            bool destroyInstanceWithLifetime,
            bool downloadDependencies = false,
            IProgress<float> progress = null)
            where T : Object
        {
            var reference = assetReference as AssetReference;
            return await LoadAssetInstanceTaskAsync<T>(reference, lifeTime,
                destroyInstanceWithLifetime,downloadDependencies, progress);
        }
        
        public static async UniTask<T> LoadAssetInstanceTaskAsync<T>(this AssetReferenceT<T> assetReference,
            ILifeTime lifeTime,
            bool destroyWith,
            Action<T> result)
            where T : Object
        {
            var reference = assetReference as AssetReference;
            var resultValue = await LoadAssetInstanceTaskAsync<T>(reference, lifeTime, destroyWith);
            
            result(resultValue);
            return resultValue;
        }

        public static async UniTask<T> LoadAssetInstanceTaskAsync<T>(this AssetReference assetReference,
            ILifeTime lifeTime,
            bool destroyInstanceWithLifetime,
            bool downloadDependencies = false,
            IProgress<float> progress = null)
            where T : Object
        {
            var asset = await assetReference.LoadAssetTaskAsync<T>(lifeTime,downloadDependencies, progress);
            if (asset == null) return default;

            var instance = asset switch
            {
                GameObject gameObjectAsset => gameObjectAsset.Spawn() as T,
                Component gameComponent => gameComponent.gameObject.Spawn().GetComponent<T>(),
                _ => Object.Instantiate(asset)
            };

            if(destroyInstanceWithLifetime) 
                instance.DestroyWith(lifeTime);
            
            return instance;
        }
        
        
#if !UNITY_WEBGL
        public static T LoadAssetInstanceForCompletion<T>(
            this AssetReferenceT<T> assetReference,
            ILifeTime lifeTime,
            bool destroyInstanceWithLifetime = true)
            where T : Object
        {
            var asset = assetReference.LoadAssetForCompletion<T>(lifeTime);
            if (asset == null) return null;

            var isPawn = false;

            Object instance = null;
            
            switch (asset)
            {
                case Component component:
                    instance = component.gameObject.Spawn<T>();
                    isPawn = true;
                    break;
                case GameObject gameObjectAsset:
                    instance = gameObjectAsset.Spawn();
                    isPawn = true;
                    break;
                default:
                    instance = Object.Instantiate(asset);
                    break;
            }

            if (!destroyInstanceWithLifetime) return instance as T;
            
            if (isPawn)
            {
                instance.DestroyWith(lifeTime);
            }
            else
            {
                instance.DestroyWith(lifeTime); 
            }
            
            return instance as T;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LoadAssetForCompletion<T>(this AssetReferenceT<T> assetReference, ILifeTime lifeTime)
            where T : Object
        {
            return LoadAssetForCompletion<T>(assetReference as AssetReference, lifeTime);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T LoadAssetForCompletion<T>(this AssetReference assetReference, ILifeTime lifeTime)
            where T : Object
        {
            if (lifeTime.IsTerminated) return default(T);
            
            if (assetReference == null || assetReference.RuntimeKeyIsValid() == false)
            {
                GameLog.LogError($"AssetReference key is NULL {assetReference}");
                return null;
            }
            
            var isComponent = typeof(T).IsComponent();

            Object asset = isComponent 
                ? LoadAssetSync<GameObject>(assetReference, lifeTime)
                : LoadAssetSync<T>(assetReference, lifeTime);
            
            if (asset == null)
                return default(T);

            var result = asset is GameObject gameObjectAsset && isComponent ?
                gameObjectAsset.GetComponent<T>() :
                asset as T;
            
            return result;
        }
#endif

#if UNITY_EDITOR
        [MenuItem("UniGame/Addressables/Clear Bundle Cache")]
#endif
        public static bool ClearBundleCache()
        {
#if UNITY_WEBGL
            return false;
#else
            return Caching.ClearCache();
#endif
            return false;
        }
        
        public static async UniTask<bool> ClearCacheAsync()
        {
            var handle = Addressables.CleanBundleCache();
            var result = await handle.ToUniTask();
            return result;
        }

        public static async UniTask DownloadDependenciesAsync(
            this IEnumerable targets,
            ILifeTime lifeTime,
            Type type = null,
            IProgress<float> process = null)
        {
            await DownloadDependenciesAsync(targets, lifeTime, Addressables.MergeMode.Union, type,process);
        }

        /// <summary>
        /// download all dependencies to the cache
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="lifeTime"></param>
        /// <param name="type"></param>
        /// <param name="process"></param>
        /// <param name="mergeMode"></param>
        public static async UniTask DownloadDependenciesAsync(
            this IEnumerable targets,
            ILifeTime lifeTime,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Union,
            Type type = null,
            IProgress<float> process = null)
        {
            var locators = await Addressables
                .LoadResourceLocationsAsync(targets,mergeMode,type)
                .ToUniTask();
            
            var handle = Addressables.DownloadDependenciesAsync(locators, mergeMode);
            
            if(handle.IsDone) return;

            handle.AddTo(lifeTime);
            
            var downloadSize = handle.GetDownloadStatus().TotalBytes;
            if (downloadSize <= 0)
            {
                GameLog.LogFormat("Addressable: {0} :: nothing to download",nameof(DownloadDependenciesAsync));
                return;
            }
            
            await handle.ToUniTask(process)
                .AttachExternalCancellation(lifeTime.Token)
                .SuppressCancellationThrow();
        }

        /// <summary>
        /// download single dependencies to the cache
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="lifeTime"></param>
        /// <param name="autoReleaseHandle"></param>
        /// <param name="type"></param>
        /// <param name="process"></param>
        public static async UniTask DownloadDependencyAsync(
            this object targets,
            ILifeTime lifeTime,
            Type type = null,
            IProgress<float> process = null)
        {
            var resource = ListPool<object>.Get();
            resource.Clear();
            resource.Add(targets);
            
            await DownloadDependenciesAsync(resource,lifeTime,type,process);
            
            resource.Despawn();
        }

        /// <summary>
        /// download single dependencies to the cache
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="lifeTime"></param>
        /// <param name="autoReleaseHandle"></param>
        /// <param name="mode"></param>
        /// <param name="type"></param>
        /// <param name="process"></param>
        public static async UniTask DownloadDependencyAsync(
            this object targets,
            ILifeTime lifeTime,
            Addressables.MergeMode mode = Addressables.MergeMode.Union,
            Type type = null,
            IProgress<float> process = null)
        {
            var resource = ListPool<object>.Get();
            resource.Clear();
            resource.Add(targets);
            
            await DownloadDependenciesAsync(resource,lifeTime,mode,type,process);
            
            resource.Despawn();
        }
        
        /// <summary>
        /// download single dependencies to the cache
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="lifeTime"></param>
        /// <param name="autoReleaseHandle"></param>
        /// <param name="mode"></param>
        /// <param name="type"></param>
        /// <param name="process"></param>
        public static async UniTask DownloadDependencyAsync(
            this object targets,
            ILifeTime lifeTime,
            Addressables.MergeMode mode = Addressables.MergeMode.Union,
            IProgress<float> process = null)
        {
            var resource = ListPool<object>.Get();
            resource.Clear();
            resource.Add(targets);
            
            await DownloadDependenciesAsync(resource,lifeTime,mode,null,process);
            
            resource.Despawn();
        }

#if !UNITY_WEBGL
        
        private static T LoadAssetSync<T>(
            this AssetReference assetReference,
            ILifeTime lifeTime)
            where T : Object
        {
            var handle = assetReference.LoadAssetAsyncOrExposeHandle<T>(out var yetRequested);
            handle.AddTo(lifeTime, yetRequested);

            var asset = handle.WaitForCompletion();
            return asset;
        }
        
#endif
        
        public static async UniTask<T> LoadAssetTaskAsync<T>(this AssetReference assetReference, 
            ILifeTime lifeTime, 
            bool downloadDependencies = false,
            IProgress<float> progress = null)
            where T : Object
        {
            if (lifeTime.IsTerminated)
                return default(T);
            
            if (assetReference == null || assetReference.RuntimeKeyIsValid() == false)
            {
                GameLog.LogError($"AssetReference key is NULL {assetReference}");
                return null;
            }
            
            var isComponent = typeof(T).IsComponent();

            var asset = isComponent 
                ? await LoadAssetTaskWithProgressAsync<GameObject>(assetReference, lifeTime,downloadDependencies, progress)
                : await LoadAssetTaskWithProgressAsync<T>(assetReference, lifeTime,downloadDependencies, progress);
            
            if (asset == null)
                return default(T);

            var result = asset is GameObject gameObjectAsset && isComponent ?
                gameObjectAsset.GetComponent<T>() :
                asset as T;
            
            return result;
        }

        public static async UniTask<AddressableResourceResult<T>> LoadAssetTaskAsync<T>(
            this string referenceKey, 
            ILifeTime lifeTime, 
            bool downloadDependencies = false,
            IProgress<float> progress = null)
        {
            if (lifeTime.IsTerminated)
                return AddressableResourceResult<T>.FailedResourceResult;
            
            if (string.IsNullOrEmpty(referenceKey))
            {
                GameLog.LogError($"AssetReference key is NULL {referenceKey}");
                return AddressableResourceResult<T>.FailedResourceResult;
            }
            
            var isComponent = typeof(T).IsComponent();

            var asset = isComponent 
                ? await LoadAssetTaskInternalAsync<GameObject>(referenceKey, lifeTime,downloadDependencies, progress)
                : await LoadAssetTaskInternalAsync<T>(referenceKey, lifeTime,downloadDependencies, progress);
            
            if (asset == null) return AddressableResourceResult<T>.FailedResourceResult;

            var resultData = AddressableResourceResult<T>.CompleteResourceResult;

            var resultValue = asset switch
            {
                T assetResult => assetResult,
                GameObject gameObjectAsset when isComponent => gameObjectAsset.GetComponent<T>(),
                _ => default
            };

            resultData.Result = resultValue;
            return resultData;
        }
        
        private static async UniTask<Object> LoadAssetTaskWithProgressAsync<T>(
            this AssetReference assetReference,
            ILifeTime lifeTime,
            bool downloadDependencies = false,
            IProgress<float> progress = null)
            where T : Object
        {
            if (downloadDependencies)
            {
                var dependencies = Addressables
                    .DownloadDependenciesAsync(assetReference)
                    .AddTo(lifeTime);
                await dependencies.ToUniTask(PlayerLoopTiming.Update,lifeTime.Token);
            }
            
            var handle = assetReference.LoadAssetAsyncOrExposeHandle<T>(out var yetRequested);
            var asset = await LoadAssetAsync(handle, yetRequested, lifeTime,progress);
            return asset;
        }
        
        private static async UniTask<Object> LoadAssetTaskInternalAsync<T>(
            this string assetReference,
            ILifeTime lifeTime,
            bool downloadDependencies = false,
            IProgress<float> progress = null)
        {
            if (downloadDependencies)
            {
                var dependencies = Addressables
                    .DownloadDependenciesAsync(assetReference)
                    .AddTo(lifeTime);
                await dependencies.ToUniTask(PlayerLoopTiming.Update,lifeTime.Token);
            }
            
            var handle = assetReference.LoadAssetAsyncOrExposeHandle<T>(out var yetRequested);
            var asset = await LoadAssetAsync(handle, yetRequested, lifeTime,progress);
            return asset;
        }

        
        public static void NotifyProgress(IAsyncHandleStatus progressData,IProgress<HandleStatus> progress )
        {
            progress.Report(new HandleStatus()
            {
                Status = progressData.Status,
                DownloadedBytes = progressData.DownloadedBytes,
                IsDone = progressData.IsDone,
                OperationException = progressData.OperationException,
                TotalBytes = progressData.TotalBytes,
            });
        }


        public static async UniTask<T> ConvertToUniTask<T>(this AsyncOperationHandle<T> handle, ILifeTime lifeTime) where T : class
        {
            handle.AddTo(lifeTime);
            return await handle.ToUniTask();
        }
        
                
        public static async UniTask<IList<Object>> LoadAssetsTaskAsync(this string label, ILifeTime lifeTime,IProgress<float> progress = null)
        {
            var handle = Addressables.LoadAssetsAsync<Object>(label, null);
            handle.AddTo(lifeTime);
            return await handle.ToUniTask(progress,cancellationToken:lifeTime.Token);
        }

        public static async UniTask<(TAsset asset, TResult result)> LoadAssetTaskAsync<TAsset, TResult>(this AssetReference assetReference,
            ILifeTime lifeTime)
            where TAsset : Object
            where TResult : class
        {
            var result = await assetReference.LoadAssetTaskAsync<TAsset>(lifeTime);
            return (result, result as TResult);
        }

        public static async UniTask<TResult> LoadAssetTaskApiAsync<TAsset, TResult>(this AssetReference assetReference, ILifeTime lifeTime)
            where TAsset : Object
            where TResult : class
        {
            var result = await assetReference.LoadAssetTaskAsync<TAsset>(lifeTime);
            return result as TResult;
        }

        public static async UniTask<T> LoadAssetTaskAsync<T>(
            this AssetReferenceGameObject assetReference,
            ILifeTime lifeTime)
            where T : class
        {
            var result = await LoadAssetTaskAsync<GameObject>(assetReference as AssetReference, lifeTime);
            if (result is T tResult) return tResult;
            return result != null ? result.GetComponent<T>() : null;
        }

        public static async UniTask<T> LoadAssetTaskAsync<T>(
            this AssetReferenceScriptableObject<T> assetReference,
            ILifeTime lifeTime)
            where T : class
        {
            var result = await LoadAssetTaskAsync<ScriptableObject>(assetReference as AssetReference, lifeTime);
            return result as T;
        }
        
        public static async UniTask<TApi> LoadAssetTaskAsync<T, TApi>(
            this AssetReferenceScriptableObject<T, TApi> assetReference,
            ILifeTime lifeTime)
            where T : ScriptableObject
            where TApi : class
        {
            var result = await LoadAssetTaskAsync<ScriptableObject>(assetReference, lifeTime);
            return result as TApi;
        }

        public static async UniTask<T> LoadAssetTaskAsync<T>(
            this AssetReferenceScriptableObject assetReference,
            ILifeTime lifeTime)
            where T : class
        {
            var result = await LoadAssetTaskAsync<ScriptableObject>(assetReference as AssetReference, lifeTime);
            return result as T;
        }

        public static async UniTask<T> LoadAssetTaskAsync<T>(this AssetReferenceT<T> assetReference, ILifeTime lifeTime)
            where T : Object
        {
            return await LoadAssetTaskAsync<T>(assetReference as AssetReference, lifeTime);
        }

        public static async UniTask<GameObject> LoadGameObjectAssetTaskAsync(this AssetReference assetReference, ILifeTime lifeTime)
        {
            var result = await LoadAssetTaskAsync<GameObject>(assetReference, lifeTime);
            return result;
        }
        
        public static async UniTask<T> LoadGameObjectAssetTaskAsync<T>(this AssetReferenceT<T> assetReference, ILifeTime lifeTime)
            where T : Component
        {
            var result = await LoadAssetTaskAsync<GameObject>(assetReference, lifeTime);
            return result ?
                result.GetComponent<T>() :
                null;
        }

        public static async UniTask<T> LoadGameObjectAssetTaskAsync<T>(this AssetReference assetReference, ILifeTime lifeTime)
            where T : class
        {
            var result = await LoadAssetTaskAsync<GameObject>(assetReference, lifeTime);
            return result ?
                result.GetComponent<T>() :
                null;
        }

        public static AsyncOperationHandle<TResult> LoadAssetAsyncOrExposeHandle<TResult>(this string assetReference, out bool yetRequested)
        {
            var handle = Addressables.LoadAssetAsync<TResult>(assetReference);
            yetRequested = handle.IsValid();
            return handle;
        }
        
        public static AsyncOperationHandle<TResult> LoadAssetAsyncOrExposeHandle<TResult>(
            this AssetReference assetReference, out bool yetRequested)
            where TResult : class
        {
            yetRequested = assetReference.OperationHandle.IsValid();
            var handle = yetRequested ? 
                assetReference.OperationHandle.Convert<TResult>():
                assetReference.LoadAssetAsync<TResult>();
            return handle;
        }

        #region lifetime

        public static AsyncOperationHandle<TAsset> AddTo<TAsset>(
            this AsyncOperationHandle<TAsset> handle, 
            ILifeTime lifeTime, bool incrementRefCount = true)
        {
            if (incrementRefCount)
                Addressables.ResourceManager.Acquire(handle);
            lifeTime.AddCleanUpAction(() => ReleaseHandle(handle).Forget());
            return handle;
        }

        public static async UniTask<TAsset> LoadAddressableByResourceAsync<TAsset>(
            this string resource,
            ILifeTime lifeTime)
        {
            var asset = await Addressables
                .LoadAssetAsync<TAsset>(resource)
                .AddToAsUniTask(lifeTime);
            return asset;
        }

        public static UniTask<TAsset> AddToAsUniTask<TAsset>(
            this AsyncOperationHandle<TAsset> handle, 
            ILifeTime lifeTime, 
            bool incrementRefCount = true)
        {
            var operation = handle.AddTo(lifeTime,incrementRefCount);
            return operation.ToUniTask();
        }

        public static async UniTask ReleaseHandle<TAsset>(this AsyncOperationHandle<TAsset> handle)
        {
            if (handle.IsValid() == false)
                return;
            await UniTask.SwitchToMainThread();
            Addressables.Release(handle);
        }
        
        public static ILifeTime AddTo<TAsset>(this AssetReferenceT<TAsset> handle, ILifeTime lifeTime)
            where TAsset : Object
        {
            lifeTime.AddCleanUpAction(() =>
            {
                if (handle.IsValid() == false)
                    return;
                Addressables.Release(handle);
            });
            return lifeTime;
        }

        public static AsyncOperationHandle AddTo(this AsyncOperationHandle handle, ILifeTime lifeTime)
        {
            lifeTime.AddCleanUpAction(() =>
            {
                if (handle.IsValid() == false)
                    return;
                Addressables.Release(handle);
            });
            return handle;
        }

        public static ILifeTime AddTo(this AssetReference handle, ILifeTime lifeTime)
        {
            lifeTime.AddCleanUpAction(() =>
            {
                if (handle.IsValid() == false)
                    return;
                Addressables.Release(handle);
            });
            return lifeTime;
        }

        public static async UniTask<Object> LoadAssetAsync<TResult>(
            AsyncOperationHandle<TResult> handle,
            bool yetRequested, 
            ILifeTime lifeTime,
            IProgress<float> progress = null)
        {
            handle.AddTo(lifeTime, yetRequested);
            var result = await handle.ToUniTask(progress,PlayerLoopTiming.Update,lifeTime.Token);
            return result as Object;
        }
        
        #endregion
        
    }
}

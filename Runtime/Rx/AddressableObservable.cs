using Cysharp.Threading.Tasks;
using UniModules.UniCore.Runtime.Common;
using UniModules.UniCore.Runtime.ObjectPool.Runtime;

namespace UniGame.Addressables.Reactive
{
    using System;
    using UniCore.Runtime.ProfilerTools;
    using UniModules.UniCore.Runtime.DataFlow;
    using UniModules.UniCore.Runtime.ObjectPool.Runtime.Extensions;
    using UniModules.UniCore.Runtime.ObjectPool.Runtime.Interfaces;
    using UniModules.UniGame.AddressableTools.Runtime.Extensions;
    using UniModules.UniGame.Core.Runtime.Extension;
    using UniModules.UniGame.Core.Runtime.Rx;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
    using UniRx;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.ResourceManagement.AsyncOperations;
    using Object = UnityEngine.Object;

    [Serializable]
    public class AddressableObservable<TAddressable,TData,TApi> : 
        IAddressableObservable<TApi> ,
        IPoolable
        where TAddressable : AssetReference 
        where TData : Object
        where TApi : class
    {
        #region inspector

        [SerializeField] protected RecycleReactiveProperty<TApi> value = new RecycleReactiveProperty<TApi>();

        [SerializeField] protected TAddressable reference;
        
        #endregion
        
        protected RecycleReactiveProperty<AsyncOperationStatus> status = new RecycleReactiveProperty<AsyncOperationStatus>();
        
        #region public properties

        public IReadOnlyReactiveProperty<TApi> Value => value;
        
        #endregion
        
        #region public methods
        
        /// <summary>
        /// initialize property with target Addressable Asset 
        /// </summary>
        /// <param name="addressable"></param>
        public IAddressableObservable<TApi> Initialize(TAddressable addressable)
        {
            status = status ?? new RecycleReactiveProperty<AsyncOperationStatus>();
            value = value ?? new RecycleReactiveProperty<TApi>();
            
            reference = addressable;
            
            return this;
        }

        public IDisposable Subscribe(IObserver<TApi> observer)
        {
            if (!ValidateReference()) {
                value.Value = default;
                return Disposable.Empty;
            }

            var disposableActon = ClassPool.Spawn<DisposableAction>();
            var lifeTime = LifeTime.Create();
            var disposableValue = value.Subscribe(observer);
            
            disposableActon.Initialize(() =>
            {
                lifeTime.Despawn();
                disposableValue.Dispose();
            });
            
            LoadReference(lifeTime).Forget();
            
            return disposableActon;
        }

        public void Dispose() => this.Despawn();
        
        public void Release() => CleanUp();

        #endregion
        
        #region private methods

        private bool ValidateReference()
        {
            if (reference == null || reference.RuntimeKeyIsValid() == false) {
                GameLog.LogWarning($"AddressableObservable : LOAD Addressable Failed {reference}");
                status.Value = AsyncOperationStatus.Failed;
                return false;
            }

            return true;
        }
        
        private async UniTask LoadReference(ILifeTime lifeTime)
        {
            if (!ValidateReference()) {
                value.Value = default;
                return;
            }

            var targetType = typeof(TData);
            var apiType = typeof(TApi);

            var isComponent = targetType.IsComponent() || apiType.IsComponent();

            var valueData = isComponent
                ? await reference.LoadGameObjectAssetTaskAsync<TApi>(lifeTime)
                : await reference.LoadAssetTaskApiAsync<TData,TApi>(lifeTime);

            await UniTask.SwitchToMainThread();
            
            value.Value = valueData;
        }


        private void CleanUp()
        {
            status.Release();
            status.Value = AsyncOperationStatus.None;
            reference = null;
            value.Release();
        }
        
        #endregion

        #region deconstructor
        
        ~AddressableObservable() => Release();
        
        #endregion
    }
}

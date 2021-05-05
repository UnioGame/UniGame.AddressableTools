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
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
    using UniRx;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
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

        [SerializeField] 
        protected TAddressable reference;
        
        #endregion

        private LifeTimeDefinition _lifeTime = new LifeTimeDefinition();
        
        #region public methods
        
        /// <summary>
        /// initialize property with target Addressable Asset 
        /// </summary>
        /// <param name="addressable"></param>
        public IAddressableObservable<TApi> Initialize(TAddressable addressable)
        {
            _lifeTime = _lifeTime ?? new LifeTimeDefinition();
            
            reference = addressable;
            
            _lifeTime.AddCleanUpAction(() => reference = null);
            return this;
        }

        public AssetReference AssetReference => reference;
        
        public IDisposable Subscribe(IObserver<TApi> observer)
        {
            if (!ValidateReference()) {
                return Disposable.Empty;
            }

            var disposableActon = ClassPool.Spawn<DisposableAction>();
            var lifeTime = LifeTime.Create();

            disposableActon.Initialize(() => lifeTime.Despawn());
            
            LoadReference(observer,lifeTime)
                .AttachExternalCancellation(_lifeTime.AsCancellationToken())
                .Forget();
            
            return disposableActon;
        }

        public void Dispose() => this.Despawn();
        
        public void Release() => _lifeTime.Release();

        #endregion
        
        #region private methods

        private bool ValidateReference()
        {
            if (reference != null && reference.RuntimeKeyIsValid()) 
                return true;
            
            GameLog.LogWarning($"AddressableObservable : LOAD Addressable Failed {reference}");

            return false;

        }
        
        private async UniTask LoadReference(IObserver<TApi> observer,ILifeTime lifeTime)
        {
            if (!ValidateReference()) {
                observer.OnError(new MissingReferenceException($"Asset reference of {this.GetType().Name} is wrong"));
                return;
            }

            var targetType = typeof(TData);
            var apiType = typeof(TApi);

            var isComponent = targetType.IsComponent() || apiType.IsComponent();

            var valueData = isComponent
                ? await reference.LoadGameObjectAssetTaskAsync<TApi>(lifeTime)
                : await reference.LoadAssetTaskApiAsync<TData,TApi>(lifeTime);

            await UniTask.SwitchToMainThread();
            
            observer.OnNext(valueData);
        }
        
        
        #endregion

    }
}

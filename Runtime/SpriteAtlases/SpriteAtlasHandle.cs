using System;
using Cysharp.Threading.Tasks;
using UniModules.UniCore.Runtime.DataFlow;
using UniModules.UniGame.AddressableTools.Runtime.Extensions;
using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UniModules.UniGame.Core.Runtime.Interfaces;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public class SpriteAtlasHandle : IDisposable, ILifeTimeContext
    {
        private LifeTimeDefinition _lifeTime = new LifeTimeDefinition();

        public string tag;
        public string guid;
        public SpriteAtlas spriteAtlas;

        public ILifeTime LifeTime => _lifeTime;

        public async UniTask<SpriteAtlasHandle> Load(AssetReferenceT<SpriteAtlas> assetReference)
        {
            guid = assetReference.AssetGUID;
            spriteAtlas = await assetReference.LoadAssetTaskAsync(_lifeTime);
            tag = spriteAtlas.tag;

            return this;
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void Dispose() => _lifeTime?.Terminate();
    }
}
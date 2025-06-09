using UniGame.AddressableTools.Runtime;
using UniGame.Core.Runtime;

namespace UniGame.AddressableAtlases
{
    using Abstract;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using Cysharp.Threading.Tasks;
    using Context.Runtime;
    
    [CreateAssetMenu(menuName = "UniGame/Services/AddressableAtlases/AddressableAtlases Source",
        fileName = "Addressable Atlases Source")]
    public class AddressableAtlasesSource : ServiceDataSourceAsset<IAddressableAtlasService>
    {
        public AssetReferenceT<AddressableAtlasesSettingsAsset> configuration;

        protected override async UniTask<IAddressableAtlasService> CreateServiceInternalAsync(IContext context)
        {
            var config = await configuration
                .LoadAssetInstanceTaskAsync(context.LifeTime,true);

            var service = new AddressableSpriteAtlasService(config.settings);
            
            return service;
        }
    }
}

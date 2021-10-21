
namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;

    public static class AddressableAtlasExtensions
    {
        public static ILifeTime BindAtlasLifeTime(this ILifeTime lifeTime, IAddressableAtlasesState atlasesState)
        {
            var service = AddressableSpriteAtlasAsset.AtlasService;
            if (service == null || service.LifeTime.IsTerminated)
                return lifeTime;

            var tags = atlasesState.AtlasTags;
            foreach (var tag in tags)
                service.BindAtlasLifeTime(tag,lifeTime);

            return lifeTime;
        }
        
        public static IAddressableAtlasesState BindAtlasLifeTime(this IAddressableAtlasesState atlasesState,ILifeTime lifeTime)
        {
            lifeTime.BindAtlasLifeTime(atlasesState);
            return atlasesState;
        }
        
    }
}

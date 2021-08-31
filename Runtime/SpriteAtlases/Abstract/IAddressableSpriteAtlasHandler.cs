namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using Cysharp.Threading.Tasks;
    using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
    using Abstract;

    public interface IAddressableSpriteAtlasHandler : IAddressablesAtlasesLoader
    {
        void BindAtlasesLifeTime(ILifeTime lifeTime, IAddressableAtlasesState atlasesState);

        UniTask Execute();
        
        void Unload();
        
    }
}
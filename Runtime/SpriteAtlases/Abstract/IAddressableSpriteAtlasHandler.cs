namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using Cysharp.Threading.Tasks;
    using Abstract;

    public interface IAddressableSpriteAtlasHandler : IAddressablesAtlasesLoader
    {
        UniTask Execute();
        
    }
}
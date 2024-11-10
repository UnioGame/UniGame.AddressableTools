using UniGame.GameFlow.Runtime.Interfaces;

namespace UniGame.AddressableAtlases.Abstract
{
    using Cysharp.Threading.Tasks;
    using UnityEngine.U2D;

    public interface IAddressableAtlasService : IGameService
    {
        UniTask<SpriteAtlas> LoadAtlasAsync(string tag);
        void RegisterSpriteAtlas(SpriteAtlas atlas);
    }
}
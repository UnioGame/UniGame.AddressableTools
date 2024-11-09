using UniGame.GameFlow.Runtime.Interfaces;

namespace UniGame.AddressableAtlases.Abstract
{
    using UnityEngine.U2D;

    public interface IAddressableAtlasService : IGameService
    {
        void RegisterSpriteAtlas(SpriteAtlas atlas);
    }
}
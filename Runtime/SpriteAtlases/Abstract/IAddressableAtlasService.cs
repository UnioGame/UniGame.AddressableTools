using UniGame.Core.Runtime;
using UniGame.GameFlow.Runtime.Interfaces;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract
{
    public interface IAddressableAtlasService : IGameService
    {
        void BindAtlasLifeTime(string atlasTag, ILifeTime lifeTime);
    }
}
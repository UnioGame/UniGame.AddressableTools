using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UniGame.GameFlow.Runtime.Interfaces;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract
{
    public interface IAddressableAtlasService : IGameService
    {
        void BindAtlasLifeTime(string atlasTag, ILifeTime lifeTime);
    }
}
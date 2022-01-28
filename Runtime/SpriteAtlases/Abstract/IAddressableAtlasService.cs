using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UniModules.UniGameFlow.GameFlow.Runtime.Interfaces;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract
{
    public interface IAddressableAtlasService : IGameService
    {
        void BindAtlasLifeTime(string atlasTag, ILifeTime lifeTime);
    }
}
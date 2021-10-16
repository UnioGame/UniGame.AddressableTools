using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;
using UniModules.UniGameFlow.GameFlow.Runtime.Interfaces;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    public interface IAddressableAtlasService : IGameService
    {
        void BindAtlasLifeTime(string atlasTag, ILifeTime lifeTime);
    }
}
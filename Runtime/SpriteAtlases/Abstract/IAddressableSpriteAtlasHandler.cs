using UniModules.UniGame.Core.Runtime.DataFlow.Interfaces;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using System;
    using Abstract;

    public interface IAddressableSpriteAtlasHandler : IAddressablesAtlasesLoader
    {
        void BindAtlasesLifeTime(ILifeTime lifeTime, IAddressableAtlasesState atlasesState);
        
        IDisposable Execute();
        
        void Unload();
        
    }
}
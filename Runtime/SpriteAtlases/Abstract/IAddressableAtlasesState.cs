using System.Collections.Generic;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    public interface IAddressableAtlasesState
    {
        IReadOnlyList<string> AtlasTags { get; }
    }
}
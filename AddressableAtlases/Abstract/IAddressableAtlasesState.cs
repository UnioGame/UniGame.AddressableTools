using System.Collections.Generic;

namespace UniGame.AddressableAtlases.Abstract
{
    public interface IAddressableAtlasesState
    {
        IReadOnlyList<string> AtlasTags { get; }
    }
}
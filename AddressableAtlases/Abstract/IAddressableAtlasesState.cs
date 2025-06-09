using System.Collections.Generic;

namespace UniGame.AddressableAtlases.Runtime
{
    public interface IAddressableAtlasesState
    {
        IReadOnlyList<string> AtlasTags { get; }
    }
}
﻿using System.Collections.Generic;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract
{
    public interface IAddressableAtlasesState
    {
        IReadOnlyList<string> AtlasTags { get; }
    }
}
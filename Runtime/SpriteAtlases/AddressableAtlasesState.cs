using System;
using System.Collections.Generic;
using UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract;
using UnityEngine;

namespace UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using Abstract;

    [Serializable]
    public class AddressableAtlasesState : IAddressableAtlasesState
    {
        [SerializeField] public List<string> atlasTags = new List<string>();

        public IReadOnlyList<string> AtlasTags => atlasTags;
    }
}
using System;
using System.Collections.Generic;
using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract;
using UnityEngine;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public class AddressableAtlasesState : IAddressableAtlasesState
    {
        [SerializeField] public List<string> atlasTags = new List<string>();

        public IReadOnlyList<string> AtlasTags => atlasTags;
    }
}
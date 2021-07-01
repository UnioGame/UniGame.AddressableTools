﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public class AddressableAtlasesState : IAddressableAtlasesState
    {
        [SerializeField] public List<string> atlasTags = new List<string>();

        public IReadOnlyList<string> AtlasTags => atlasTags;
    }
}
using System;
using UnityEngine.U2D;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public class SpriteAtlasHandle
    {
        public string tag;
        public string guid;
        public SpriteAtlas spriteAtlas;
    }
}
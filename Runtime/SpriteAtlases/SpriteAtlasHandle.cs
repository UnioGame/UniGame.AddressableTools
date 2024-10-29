using System;
using UnityEngine.U2D;

namespace UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    public class SpriteAtlasHandle
    {
        public string tag;
        public string guid;
        public SpriteAtlas spriteAtlas;
    }
}
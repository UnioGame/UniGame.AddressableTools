namespace UniGame.AddressableAtlases
{
    using System;
    using Core.Runtime;
    using UnityEngine.U2D;

    [Serializable]
    public class AddressableAtlasState
    {
        public string tag;
        public bool isLoaded;
        public SpriteAtlas atlas;
        public ILifeTime lifeTime;
        
        public AddressableAtlasData atlasData;
    }
}
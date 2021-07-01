namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    using System;
    using AssetReferencies;
    using UniModules.UniGame.Core.Runtime.DataStructure;

    [Serializable]
    public class AddressblesAtlasesTagsMap : SerializableDictionary<string, AtlasReference>
    {
        
    }

    [Serializable]
    public struct AtlasReference
    {
        public string tag;
        public AssetReferenceSpriteAtlas assetReference;
    }
}
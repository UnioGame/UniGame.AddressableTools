namespace UniGame.AddressableAtlases
{
    using System;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;

    [Serializable]
    public class AddressableAtlasSettings
    {
        [ListDrawerSettings(ListElementLabelName = "@tag")]
        public List<AddressableAtlasData> atlases = new();
    }
}
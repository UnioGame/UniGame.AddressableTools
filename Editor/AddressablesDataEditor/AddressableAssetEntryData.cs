using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UniModules.UniGame.AddressableTools.Editor.AddressableDataEditor
{
    [Serializable]
    public class AddressableAssetEntryData
    {
        public Object asset;
        public string guid;
        public string address;
        public string groupName;
        public bool readOnly;
        public List<string> labels = new List<string>();
    }
}
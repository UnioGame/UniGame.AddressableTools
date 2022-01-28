namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using System;
    using System.Collections.Generic;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [Serializable]
    public class AddressableEntryTree
    {
        
#if ODIN_INSPECTOR
        [Searchable]
#endif
        public List<AddressableAssetEntryData> entryData = new List<AddressableAssetEntryData>();

        public void Reset()
        {
            entryData.Clear();
        }
        
    }
}
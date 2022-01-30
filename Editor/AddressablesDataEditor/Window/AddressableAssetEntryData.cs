#if ODIN_INSPECTOR

using System.Linq;
using UniModules.Editor.OdinTools.GameEditor.Categories;
using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor.Window;
using UnityEditor;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.AddressablesDataEditor
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    using System;
    using System.Collections.Generic;
    using Object = UnityEngine.Object;
    
    [Serializable]
    public class AddressableAssetEntryData
#if ODIN_INSPECTOR
        : ISearchFilterable
#endif
    {
        private const string baseGroup = "entry data";
        private const string infoGroup = "entry data/info";
        
        [VerticalGroup(baseGroup)]
        public string guid;
        [VerticalGroup(baseGroup)]
        public bool isRemote;
        
        [FoldoutGroup(infoGroup)]
        [VerticalGroup(baseGroup)]
        public string address;
        
        [FoldoutGroup(infoGroup)]
        [VerticalGroup(baseGroup)]
        public string groupName;
        
        [FoldoutGroup(infoGroup)]
        [VerticalGroup(baseGroup)]
        public string buildPath;
        
        [FoldoutGroup(infoGroup)]
        [VerticalGroup(baseGroup)]
        public bool readOnly;
        
        [FoldoutGroup(infoGroup)]
        [VerticalGroup(baseGroup)]
        public List<string> labels = new List<string>();

#if ODIN_INSPECTOR
        [PropertyOrder(-1)]
        [InlineEditor]
        [HideIf(nameof(IsNullAsset))]
        [VerticalGroup(baseGroup)]
#endif
        public Object asset;

#if ODIN_INSPECTOR
        [InlineProperty]
        [VerticalGroup("dependencies")]
        [InlineButton(nameof(ShowDependencies),"show")]
#endif
        public List<ResourceLocationData> dependencies = new List<ResourceLocationData>();

#if ODIN_INSPECTOR
        [InlineProperty]
        [VerticalGroup("dependencies")]
        [InlineButton(nameof(ShowEntryDependencies),"show entries")]
#endif
        public List<AddressableAssetEntryData> entryDependencies = new List<AddressableAssetEntryData>();

        public bool IsNullAsset => asset == null;
        
        public bool HasDependencies => dependencies.Count > 0;

        public override int GetHashCode() => guid == null ? 0 : guid.GetHashCode();

        public void ShowEntryDependencies()
        {
            var window = EditorWindow.GetWindow<ObjectViewWindow>();
            window.UpdateView(entryDependencies);
            window.Show();
        }
        
        public void ShowDependencies()
        {
            var window = EditorWindow.GetWindow<ObjectViewWindow>();
            window.UpdateView(dependencies);
            window.Show();
        }
        
        public override bool Equals(object obj)
        {
            if (obj is AddressableAssetEntryData entryData)
                return entryData.guid == guid;
            return base.Equals(obj);
        }

        public override string ToString()
        {
            var assetName = asset == null ? string.Empty : asset.name;
            return $"ENTRY: GUID: {guid} | NAME: [{assetName}] [{address}] | GROUP: {groupName} | IS_REMOTE: {isRemote}";
        }

        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return true;
            
            var result = !string.IsNullOrEmpty(guid) && guid.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(address) && address.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(groupName) && groupName.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= !string.IsNullOrEmpty(buildPath) && buildPath.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            result |= labels.Any(x => x.IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase) >= 0);
            result |= isRemote && nameof(isRemote).IndexOf(searchString,StringComparison.InvariantCultureIgnoreCase)>=0;
            result |= dependencies.Any(x => x.IsMatch(searchString));
            result |= dependencies.Any(x => x.IsMatch(searchString));

            return result;
        }

    }
}
#endif
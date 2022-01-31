using System;
using System.Collections.Generic;
using UniModules.UniGame.Core.Runtime.ScriptableObjects;
using UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases.Abstract;
using UniRx;
using UnityEngine;

namespace UniModules.UniGame.AddressableTools.Runtime.SpriteAtlases
{
    [Serializable]
    [CreateAssetMenu(menuName = "UniGame/Addressables/AddressableAtlasesState",
        fileName = nameof(AddressableAtlasesState))]
    public class AddressableAtlasesStateAsset : LifetimeScriptableObject, IAddressableAtlasesState
    {
        #region inspector

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FolderPath]
#endif
        public List<string> assetFolders = new List<string>();

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FilePath]
#endif
        public List<string> assets = new List<string>();

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineProperty] [Sirenix.OdinInspector.HideLabel]
#endif
        public AddressableAtlasesState atlases = new AddressableAtlasesState();

        #endregion

        public IReadOnlyList<string> AtlasTags => atlases.AtlasTags;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void Reimport()
        {
            MessageBroker.Default.Publish(new UpdateAddressableAtlasesMessage());
        }

    }
}
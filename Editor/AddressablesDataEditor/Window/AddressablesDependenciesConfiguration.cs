namespace UniModules.UniGame.AddressableTools.Editor.AddressablesDependecies
{
    using UniModules.UniGame.Core.Editor.EditorProcessors;
    using UnityEngine;

    [CreateAssetMenu(menuName = "UniGame/Addressables/" + nameof(AddressablesDependenciesConfiguration))]
    [GeneratedAssetInfo("Editors/Editor")]
    public class AddressablesDependenciesConfiguration : GeneratedAsset<AddressablesDependenciesConfiguration>
    {
        public Object sceneAsset;

        public string logPath = "Logs/addressables_dependencies.log";
    }
}

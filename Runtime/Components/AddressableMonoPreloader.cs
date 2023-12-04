namespace UniGame.AddressableTools.Runtime
{
    using Sirenix.OdinInspector;
    using UniModules.UniGame.Core.Runtime.DataFlow.Extensions;
    using UnityEngine;

    public class AddressableMonoPreloader : MonoBehaviour
    {
        [InlineProperty]
        [HideLabel]
        public AddressableResourcePreloader preloader = new();
        
        public bool activateOnStart = true;
    
        // Start is called before the first frame update
        public void Start()
        {
            if (!activateOnStart) return;
            
            WarmUp();
        }

        [Button]
        public void WarmUp()
        {
            preloader.WarmUp(this.GetAssetLifeTime());
        }

    }
}
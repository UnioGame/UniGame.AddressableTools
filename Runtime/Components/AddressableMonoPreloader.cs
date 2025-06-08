namespace UniGame.AddressableTools.Runtime
{
    
    using Core.Runtime;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    public class AddressableMonoPreloader : MonoBehaviour
    {
#if ODIN_INSPECTOR
        [InlineProperty]
        [HideLabel]
#endif
        public AddressableResourcePreloader preloader = new();
        
        public bool activateOnStart = true;
    
        // Start is called before the first frame update
        public void Start()
        {
            if (!activateOnStart) return;
            
            WarmUp();
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void WarmUp()
        {
            preloader.WarmUp(this.GetAssetLifeTime());
        }

    }
}
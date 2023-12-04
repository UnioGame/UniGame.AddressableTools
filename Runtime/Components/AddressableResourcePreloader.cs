namespace UniGame.AddressableTools.Runtime
{
    using System;
    using System.Collections.Generic;
    using Core.Runtime;
    using Cysharp.Threading.Tasks;
    using Sirenix.OdinInspector;
    using UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.Pooling;

    [Serializable]
    public class AddressableResourcePreloader
    {
        [ListDrawerSettings(ListElementLabelName = "@Label")]
        public List<AddressablePreloadValue> warmupReferences = new();
        
        public void WarmUp(ILifeTime lifeTime)
        {
            foreach (var value in warmupReferences)
            {
                if(value == null || value.reference.RuntimeKeyIsValid() == false)
                    continue;
                
                var reference = value.reference;
                reference.WarmUpReference(lifeTime, value.count, value.prewarm, value.killDelay)
                    .AttachExternalCancellation(lifeTime.Token)
                    .Forget();
            }
        }
    }
}
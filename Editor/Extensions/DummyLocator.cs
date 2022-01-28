using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Editor.Extensions
{
    public class DummyLocator : IResourceLocator
    {
        public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
        {
            locations = new List<IResourceLocation>();
            return false;
        }

        public string LocatorId => nameof(DummyLocator);

        public IEnumerable<object> Keys => Enumerable.Empty<object>();
    }
}
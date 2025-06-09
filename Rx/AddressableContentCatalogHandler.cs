using System.Linq;
using Cysharp.Threading.Tasks;
 
using UnityEngine;
using UnityEngine.Networking;

namespace UniGame.AddressableTools.Runtime
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    public class AddressableContentCatalogHandler
    {
        public const string REMOTE_KEY = "AddressablesMainContentCatalogRemoteHash";
        public const string LOCAL_KEY  = "AddressablesMainContentCatalogCacheHash";

        private string _remoteUri;
        private string _localUri;
        private string _localHash;
        private float lastTime;
        private bool _isReady = false;
        private bool _isCatalogUpdated = false;

        public async UniTask UpdateResourceLocations()
        {
            //detect addressables location from locators
            foreach (var locators in Addressables.ResourceLocators) {
                locators.Locate(REMOTE_KEY, typeof(string), out var locations);
                _remoteUri = locations?.FirstOrDefault()?.InternalId;
            
                locators.Locate(LOCAL_KEY, typeof(string), out var cacheLocations);
                _localUri = locations?.FirstOrDefault()?.InternalId;
            }
        
            //load url data from player prefs
            if (string.IsNullOrEmpty(this._remoteUri) || string.IsNullOrEmpty(_localUri)) {
                if (PlayerPrefs.HasKey(REMOTE_KEY) && PlayerPrefs.HasKey(LOCAL_KEY)) {
                    this._remoteUri = PlayerPrefs.GetString(REMOTE_KEY);
                    _localUri       = PlayerPrefs.GetString(LOCAL_KEY);
                }
            }

            //update playerPrefs addressable location key
            if (!string.IsNullOrEmpty(this._remoteUri) && !string.IsNullOrEmpty(_localUri)) {
                PlayerPrefs.SetString(REMOTE_KEY, this._remoteUri);
                PlayerPrefs.SetString(LOCAL_KEY, _localUri);

                var www = UnityWebRequest.Get(_localUri);
                var wwwResult = await www.SendWebRequest().ToUniTask();
                _localHash = wwwResult.downloadHandler.text;
                _isReady     = true;
            }
        
        }

        public async UniTask<bool> CheckForUpdates()
        {
            if (_isReady == false) {
                await UpdateResourceLocations();
            }
        
            var request = UnityWebRequest.Get(_remoteUri);
            var result  = await request.SendWebRequest().ToUniTask();
            var remoteHash = result.downloadHandler.text;

            if (remoteHash != _localHash) {
                _isCatalogUpdated = false;
            }

            return _isCatalogUpdated;
        }
    }
}
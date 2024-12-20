namespace UniModules.UniGame.CoreModules.UniGame.AddressableTools.Runtime.Remote
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Threading;
	using Cysharp.Threading.Tasks;
	using UnityEngine;
	using UnityEngine.AddressableAssets;
	using UnityEngine.AddressableAssets.ResourceLocators;
	using UnityEngine.ResourceManagement.AsyncOperations;
	using UnityEngine.ResourceManagement.ResourceLocations;

	public class AddressableContentLoader
    {
        private AsyncOperationHandle _downloadHandle;
		private const float ToMb = 1048576.0f;
		private AsyncOperationHandle<IList<IResourceLocation>> _validateAddressHandle;
		private bool _downloadHandleStatus;
		private IResourceLocator _resourceLocator;

		public event Action<float,string> OnLoadingProgressUpdate;
		public event Action OnStartLoading;
		public event Action OnEndLoading;
		public event Action<string> OnLoadingError;

		public async UniTask InitializeAsync(CancellationToken ct)
		{
			Debug.Log("AddressablesLoader InitializeAsync");
			if(_resourceLocator!=null)
			{
				Debug.LogWarning("AddressablesLoader Already initialized!");
				return;
			}

			_resourceLocator = await Addressables
				.InitializeAsync()
				.ToUniTask(cancellationToken: ct);
			
			_validateAddressHandle = Addressables
				.LoadResourceLocationsAsync(_resourceLocator.Keys, Addressables.MergeMode.Union);
			
			await _validateAddressHandle.Task;
		}

		public async UniTask<bool> LoadAssetBundlesAsync(CancellationToken ct)
		{
			Debug.Log("AddressablesLoader LoadAssetBundlesAsync");
			
			if (_downloadHandleStatus)
			{
				Debug.LogWarning("AddressablesLoader AssetBundles already loaded");
				return true;
			}

			if (!_validateAddressHandle.IsValid() || 
			    _validateAddressHandle.Status != AsyncOperationStatus.Succeeded)
			{
				return false;
			}
			
			//AssetBundle.UnloadAllAssetBundles(true); //for testing
			//await Addressables.ClearDependencyCacheAsync(_validateAddressHandle.Result, false); //for testing

			OnStartLoading?.Invoke();
			
			_downloadHandle = Addressables.DownloadDependenciesAsync(_validateAddressHandle.Result, false);

			var downLoadSize = await Addressables.GetDownloadSizeAsync(_validateAddressHandle.Result) / ToMb;
			var downLoadSizeStr = Math.Round(downLoadSize, 1)
				.ToString(CultureInfo.InvariantCulture);
			
			try
			{
				while (_downloadHandle.Status == AsyncOperationStatus.None)
				{
					var status = _downloadHandle.GetDownloadStatus();
					var progress = status.Percent;
					var loadedMb = Math.Round(status.DownloadedBytes / ToMb, 1).ToString(CultureInfo.InvariantCulture);
					OnLoadingProgressUpdate?.Invoke(progress, $"Downloading: {loadedMb}/{downLoadSizeStr}");
					//_loadingScreen.ReportWithText(progress, $"Downloading: {loadedMb}/{downLoadSizeStr}");
					await UniTask.DelayFrame(1, PlayerLoopTiming.Update, ct);
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				OnLoadingError?.Invoke(e.Message);
				OnEndLoading?.Invoke();
				return false;
			}

			_downloadHandleStatus = _downloadHandle.Status == AsyncOperationStatus.Succeeded;

			Addressables.Release(_downloadHandle);

			OnEndLoading?.Invoke();
			
			return _downloadHandleStatus;
		}
    }
}
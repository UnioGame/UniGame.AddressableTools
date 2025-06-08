# UniGame.AddressableTools

A comprehensive toolkit for working with Unity Addressables system, providing convenient extensions, components, and services for resource management.

## 📋 Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Components](#core-components)
- [Extensions](#extensions)
- [Sprite Atlases](#sprite-atlases)
- [Object Pooling](#object-pooling)
- [Reactive Extensions](#reactive-extensions)
- [Editor Tools](#editor-tools)
- [Usage Examples](#usage-examples)

## 🚀 Features

### Core Features
- ✅ Simplified work with Addressable resources
- ✅ Automatic resource lifecycle management
- ✅ Typed resource references
- ✅ Object pooling system
- ✅ Sprite atlas support
- ✅ Reactive extensions for asynchronous work
- ✅ Dependency analysis tools
- ✅ Remote content and updates

### Editor Tools
- ✅ Addressable resource dependency analyzer
- ✅ Error validation and fixing
- ✅ Automatic atlas configuration
- ✅ Cache and data cleanup

## 📦 Installation

The module is part of UniGame.CoreModules and is automatically included in the project.

Add the following dependencies to your `Packages/manifest.json` file:

```json
  "dependencies": {
    "com.unigame.addressablestools" : "https://github.com/UnioGame/unigame.addressables.git",
  }
```

### Dependencies

```json
{
    "com.unity.addressables": "2.6.0",
    "com.cysharp.unitask" : "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.unigame.unicore": "https://github.com/UnioGame/unigame.core.git",
    "com.unigame.rx": "https://github.com/UnioGame/unigame.rx.git"
}
```

## ⚡ Quick Start

### Basic Resource Loading

```csharp
using UniGame.AddressableTools.Runtime;
using UniGame.Core.Runtime;
using Cysharp.Threading.Tasks;

public class ResourceLoader : MonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject prefabReference;
    private LifeTimeDefinition _lifeTime = new();

    private async void Start()
    {
        // Load and create object
        var gameObject = await prefabReference.LoadAssetTaskAsync<GameObject>(_lifeTime);
        var instance = await prefabReference.SpawnObjectAsync<GameObject>(
            transform.position, 
            transform, 
            _lifeTime);
            
        // Object will be automatically released when _lifeTime terminates
    }

    private void OnDestroy() => _lifeTime.Terminate();
}
```

### Working with Components

```csharp
[SerializeField] private AssetReferenceComponent<PlayerController> playerReference;

private async void SpawnPlayer()
{
    var player = await playerReference.SpawnObjectAsync<PlayerController>(
        spawnPoint.position,
        parent: gameWorld,
        lifeTime: _lifeTime);
        
    // player already contains the required component
    player.Initialize();
}
```

## 🧩 Core Components

### AddressableInstancer

Component for automatic creation of objects from Addressable resources.

```csharp
public class AddressableInstancer : MonoBehaviour
{
    [SerializeField] private List<AddressableInstance> links;
    [SerializeField] private bool createOnStart = true;
    [SerializeField] private bool unloadOnDestroy = true;
    
    // Transform settings
    [SerializeField] private Transform parent;
    [SerializeField] private Vector3 position;
    [SerializeField] private Quaternion rotation;
}
```

### AddressableMonoPreloader

Component for resource preloading.

```csharp
public class AddressableMonoPreloader : MonoBehaviour
{
    [SerializeField] private AddressableResourcePreloader preloader;
    [SerializeField] private bool activateOnStart = true;
}
```

## 🔧 Extensions

### Main Loading Methods

```csharp
// Load single resource
var asset = await assetReference.LoadAssetTaskAsync<Texture2D>(lifeTime);

// Load with instance creation
var instance = await assetReference.LoadAssetInstanceTaskAsync<GameObject>(
    lifeTime, 
    destroyInstanceWithLifetime: true);

// Create object in world
var spawned = await assetReference.SpawnObjectAsync<GameObject>(
    position: Vector3.zero,
    parent: transform,
    lifeTime: lifeTime);

// Load list of resources
var assets = await assetReferences.LoadAssetsTaskAsync<Sprite>(lifeTime);
```

### Working with Dependencies

```csharp
// Preload dependencies
await assetReference.DownloadDependencyAsync(lifeTime);

// Load with progress
var progress = new Progress<float>(p => Debug.Log($"Progress: {p:P}"));
await assetReference.LoadAssetTaskAsync<GameObject>(lifeTime, true, progress);

// Clear cache
await AddressableExtensions.ClearCacheAsync();
```

### Synchronous Loading

```csharp
// For cases when resource is already loaded
var texture = assetReference.LoadAssetForCompletion<Texture2D>(lifeTime);
var instance = assetReference.LoadAssetInstanceForCompletion<GameObject>(lifeTime);
```

## 🎨 Sprite Atlases

### Atlas Service Setup

```csharp
public class AddressableSpriteAtlasService : IAddressableAtlasService
{
    public async UniTask<SpriteAtlas> LoadAtlasAsync(string tag);
    public void RegisterSpriteAtlas(SpriteAtlas atlas);
}
```

### Using Atlases

```csharp
// Automatic registration through service
public class AtlasUser : MonoBehaviour
{
    [SerializeField] private AtlasReference atlasRef;
    
    private async void Start()
    {
        var atlas = await atlasService.LoadAtlasAsync(atlasRef.tag);
        var sprite = atlas.GetSprite("spriteName");
    }
}
```

### Editor Setup

1. Create `AddressableAtlasesSettingsAsset`
2. Configure `AddressableAtlasesSource`
3. Use "Reimport" button for automatic atlas collection

## 🎱 Object Pooling

### Pool Creation

```csharp
// Create pool with preloading
await bulletPrefab.AttachPoolLifeTimeAsync(lifeTime, preloadCount: 50);

// Warm up pool
await bulletPrefab.WarmUp(lifeTime, count: 20, activate: false);
```

### Using Pool

```csharp
// Create object from pool
var bullet = await bulletPrefab.SpawnAsync(lifeTime, firePoint.position, firePoint.rotation);

// Create active object
var activeBullet = await bulletPrefab.SpawnActiveAsync(lifeTime, firePoint);

// Objects automatically return to pool when deactivated
```

## ⚡ Reactive Extensions

### Creating Observable from AssetReference

```csharp
// Create reactive stream
var textureObservable = textureReference.ToObservable<Texture2D>(lifeTime);

// Subscribe to changes
textureObservable.Subscribe(texture => {
    if (texture != null)
        renderer.material.mainTexture = texture;
});

// Combine with other streams
var combinedStream = textureObservable
    .Where(t => t != null)
    .Select(t => new MaterialData(t))
    .Subscribe(data => ApplyMaterial(data));
```

## 🛠️ Editor Tools

### Dependency Analyzer

**Menu:** `UniGame/Tools/Addressables/Addressables Dependencies Window`

Features:
- Analyze dependencies between Addressable resources
- Find local resources with remote dependencies
- Filter and search problematic resources
- Export reports

### Validation and Fixing

**Menu:** `UniGame/Addressables/`

- `Validate Addressables Errors` - Check for errors
- `Fix Addressables Errors` - Automatic fixing
- `Remove Missing References` - Remove broken references
- `Remote Empty Groups` - Remove empty groups

### Cache Cleanup

- `Clean Library Cache` - Clean library cache
- `Clean Default Context Builder` - Clean context builder
- `Clean All` - Complete cleanup

## 📚 Usage Examples

### Level Loader

```csharp
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private AssetReference levelScene;
    [SerializeField] private List<AssetReferenceGameObject> levelPrefabs;
    private LifeTimeDefinition _levelLifeTime = new();

    public async UniTask LoadLevel()
    {
        // Load scene
        var sceneInstance = await levelScene.LoadSceneTaskAsync(
            _levelLifeTime, 
            LoadSceneMode.Additive);

        // Preload level resources
        await levelPrefabs.LoadAssetsTaskAsync<GameObject>(_levelLifeTime);
        
        // Create level objects
        foreach (var prefabRef in levelPrefabs)
        {
            await prefabRef.SpawnObjectAsync<GameObject>(
                Vector3.zero, 
                null, 
                _levelLifeTime);
        }
    }

    public void UnloadLevel() => _levelLifeTime.Release();
}
```

### Inventory System with Icons

```csharp
public class InventoryItem : MonoBehaviour
{
    [SerializeField] private AssetReferenceSprite iconReference;
    [SerializeField] private Image iconImage;
    private LifeTimeDefinition _lifeTime = new();

    private async void Start()
    {
        // Reactive icon loading
        var iconObservable = iconReference.ToObservable<Sprite>(_lifeTime);
        iconObservable.Subscribe(sprite => iconImage.sprite = sprite);
    }

    private void OnDestroy() => _lifeTime.Terminate();
}
```

### Audio Resource Manager

```csharp
public class AudioManager : MonoBehaviour
{
    [SerializeField] private List<AssetReferenceAudioClip> musicTracks;
    [SerializeField] private AudioSource musicSource;
    private LifeTimeDefinition _lifeTime = new();

    public async UniTask PlayRandomTrack()
    {
        var randomTrack = musicTracks[Random.Range(0, musicTracks.Count)];
        var audioClip = await randomTrack.LoadAssetTaskAsync<AudioClip>(_lifeTime);
        
        musicSource.clip = audioClip;
        musicSource.Play();
    }

    private void OnDestroy() => _lifeTime.Terminate();
}
```

## 🔗 Typed References

The module provides numerous typed references:

- `AssetReferenceGameObject` - for GameObject
- `AssetReferenceComponent<T>` - for components
- `AssetReferenceSprite` - for sprites
- `AssetReferenceSpriteAtlas` - for atlases
- `AssetReferenceParticleSystem` - for particle systems
- `AssetReferenceScriptableObject<T>` - for ScriptableObject
- `AddressableValue<T>` - wrapper with inspector preview

## ⚠️ Important Notes

### Lifecycle Management

Always use `ILifeTime` for automatic resource cleanup:

```csharp
// ✅ Correct
var asset = await reference.LoadAssetTaskAsync<GameObject>(lifeTime);

// ❌ Incorrect - resource won't be released
var asset = await reference.LoadAssetTaskAsync<GameObject>(null);
```

### Dependency Preloading

For critical resources use preloading:

```csharp
// Preload before use
await criticalAssets.DownloadDependenciesAsync(lifeTime);
```

### Pooling for Frequently Created Objects

```csharp
// Setup pool for bullets, effects, etc.
await bulletPrefab.AttachPoolLifeTimeAsync(gameLifeTime, 100);
```

## 📄 License

MIT License - see LICENSE file for details.

## 🤝 Support

For questions and suggestions, contact the UniGame team.
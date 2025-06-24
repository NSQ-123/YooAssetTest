using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace GF.AstSystem
{

    /// <summary>
    /// 统一资源管理系统
    /// </summary>
    public class AstSystem : MonoBehaviour
    {
        private static AstSystem _instance;
        public static AstSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AstSystem");
                    _instance = go.AddComponent<AstSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [SerializeField] private ResourceBackend _defaultBackend = ResourceBackend.YooAsset;
        
        // 资源句柄缓存
        private Dictionary<string, IResourceHandle> _handleCache = new Dictionary<string, IResourceHandle>();
        
        // 场景卸载句柄集合
        private Dictionary<Scene, List<IResourceHandle>> _sceneHandles = new Dictionary<Scene, List<IResourceHandle>>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                _instance = null;
            }
        }

        /// <summary>
        /// 场景卸载时释放相关资源
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            if (_sceneHandles.TryGetValue(scene, out var handles))
            {
                foreach (var handle in handles)
                {
                    if (handle.UnloadMode == UnloadMode.SceneUnload)
                    {
                        handle.Release();
                    }
                }
                _sceneHandles.Remove(scene);
            }
        }

        /// <summary>
        /// 设置默认后端
        /// </summary>
        public void SetDefaultBackend(ResourceBackend backend)
        {
            _defaultBackend = backend;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public async Task<IResourceHandle> LoadAssetAsync<T>(string address, UnloadMode unloadMode = UnloadMode.None, ResourceBackend? backend = null) where T : UnityEngine.Object
        {
            return await LoadAssetAsync(address, typeof(T), unloadMode, backend);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public async Task<IResourceHandle> LoadAssetAsync(string address, Type type = null, UnloadMode unloadMode = UnloadMode.None, ResourceBackend? backend = null)
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError("Asset address cannot be null or empty");
                return null;
            }

            // 检查缓存
            string cacheKey = $"{address}_{type?.Name ?? "Object"}";
            if (_handleCache.TryGetValue(cacheKey, out var cachedHandle))
            {
                cachedHandle.AddRef();
                return cachedHandle;
            }

            var targetBackend = backend ?? _defaultBackend;
            IResourceHandle handle = null;

            switch (targetBackend)
            {
                case ResourceBackend.YooAsset:
                    handle = await LoadAssetAsyncWithYooAsset(address, type, unloadMode);
                    break;
                case ResourceBackend.Addressables:
#if UNITY_ADDRESSABLES
                    handle = await LoadAssetAsyncWithAddressables(address, type, unloadMode);
#else
                    Debug.LogError("Addressables is not available in this project");
                    return null;
#endif
                    break;
            }

            if (handle != null)
            {
                _handleCache[cacheKey] = handle;
                
                // 如果是场景卸载模式，添加到场景句柄集合
                if (unloadMode == UnloadMode.SceneUnload)
                {
                    var currentScene = SceneManager.GetActiveScene();
                    if (!_sceneHandles.ContainsKey(currentScene))
                    {
                        _sceneHandles[currentScene] = new List<IResourceHandle>();
                    }
                    _sceneHandles[currentScene].Add(handle);
                }
                
                // 自动释放监听
                if (unloadMode == UnloadMode.AutoRelease)
                {
                    handle.Completed += OnAutoReleaseCompleted;
                }
            }

            return handle;
        }

        /// <summary>
        /// 异步加载资源并直接返回资源对象
        /// </summary>
        public async Task<T> LoadAssetDirectAsync<T>(string address, UnloadMode unloadMode = UnloadMode.None, ResourceBackend? backend = null) where T : UnityEngine.Object
        {
            var handle = await LoadAssetAsync<T>(address, unloadMode, backend);
            return await handle.GetAssetAsync<T>();
        }

        /// <summary>
        /// 批量异步加载资源
        /// </summary>
        public async Task<List<IResourceHandle>> LoadAssetsAsync<T>(List<string> addresses, UnloadMode unloadMode = UnloadMode.None, ResourceBackend? backend = null) where T : UnityEngine.Object
        {
            var tasks = new List<Task<IResourceHandle>>();
            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, unloadMode, backend));
            }
            
            var handles = await Task.WhenAll(tasks);
            return new List<IResourceHandle>(handles);
        }

        /// <summary>
        /// 批量异步加载资源并直接返回资源对象
        /// </summary>
        public async Task<List<T>> LoadAssetsDirectAsync<T>(List<string> addresses, UnloadMode unloadMode = UnloadMode.None, ResourceBackend? backend = null) where T : UnityEngine.Object
        {
            var handles = await LoadAssetsAsync<T>(addresses, unloadMode, backend);
            var tasks = new List<Task<T>>();
            
            foreach (var handle in handles)
            {
                tasks.Add(handle.GetAssetAsync<T>());
            }
            
            var assets = await Task.WhenAll(tasks);
            return new List<T>(assets);
        }

        private void OnAutoReleaseCompleted(IResourceHandle handle)
        {
            // 可以在这里添加自动释放逻辑
            // 比如延迟释放或基于使用频率的释放策略
        }

        /// <summary>
        /// 使用YooAsset加载资源
        /// </summary>
        private async Task<IResourceHandle> LoadAssetAsyncWithYooAsset(string address, Type type, UnloadMode unloadMode)
        {
            AssetHandle yooHandle;
            
            if (type != null)
            {
                yooHandle = YooAssets.LoadAssetAsync(address, type);
            }
            else
            {
                yooHandle = YooAssets.LoadAssetAsync<UnityEngine.Object>(address);
            }

            var handle = new YooAssetHandle(yooHandle, address, unloadMode);
            return handle;
        }

#if UNITY_ADDRESSABLES
        /// <summary>
        /// 使用Addressables加载资源
        /// </summary>
        private async Task<IResourceHandle> LoadAssetAsyncWithAddressables(string address, Type type, UnloadMode unloadMode)
        {
            var addressablesHandle = Addressables.LoadAssetAsync<UnityEngine.Object>(address);
            var handle = new AddressablesHandle(addressablesHandle, address, unloadMode);
            return handle;
        }
#endif

        /// <summary>
        /// 同步加载资源（不推荐使用）
        /// </summary>
        public IResourceHandle LoadAssetSync<T>(string address, UnloadMode unloadMode = UnloadMode.None, ResourceBackend? backend = null) where T : UnityEngine.Object
        {
            var task = LoadAssetAsync<T>(address, unloadMode, backend);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        public void ReleaseHandle(IResourceHandle handle)
        {
            handle?.Release();
            
            // 从缓存中移除引用计数为0的句柄
            if (handle?.RefCount <= 0)
            {
                string cacheKey = $"{handle.Address}_{handle.Asset?.GetType().Name ?? "Object"}";
                _handleCache.Remove(cacheKey);
            }
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        public async Task PreloadAssetsAsync(List<string> addresses, ResourceBackend? backend = null)
        {
            var tasks = new List<Task<IResourceHandle>>();
            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync(address, null, UnloadMode.None, backend));
            }
            
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 预加载资源并等待所有资源加载完成
        /// </summary>
        public async Task PreloadAndWaitAssetsAsync(List<string> addresses, ResourceBackend? backend = null)
        {
            var handles = new List<IResourceHandle>();
            
            foreach (var address in addresses)
            {
                var handle = await LoadAssetAsync(address, null, UnloadMode.None, backend);
                if (handle != null)
                {
                    handles.Add(handle);
                }
            }
            
            var waitTasks = new List<Task>();
            foreach (var handle in handles)
            {
                waitTasks.Add(handle.WaitForCompletionAsync());
            }
            
            await Task.WhenAll(waitTasks);
        }

        /// <summary>
        /// 清理所有资源
        /// </summary>
        public void ClearAll()
        {
            foreach (var handle in _handleCache.Values)
            {
                handle.Dispose();
            }
            _handleCache.Clear();
            _sceneHandles.Clear();
        }

        /// <summary>
        /// 清理未使用的资源
        /// </summary>
        public void ClearUnused()
        {
            var toRemove = new List<string>();
            foreach (var kvp in _handleCache)
            {
                if (kvp.Value.RefCount <= 0)
                {
                    kvp.Value.Dispose();
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var key in toRemove)
            {
                _handleCache.Remove(key);
            }
        }

        /// <summary>
        /// 获取资源统计信息
        /// </summary>
        public void GetResourceStats(out int totalHandles, out int activeHandles)
        {
            totalHandles = _handleCache.Count;
            activeHandles = 0;
            
            foreach (var handle in _handleCache.Values)
            {
                if (handle.RefCount > 0)
                {
                    activeHandles++;
                }
            }
        }
    }
}

#define UNITY_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading;
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
        
        // 资源引用数据缓存 - 按包名和地址组织
        private Dictionary<string, Dictionary<string, AstRefData>> _refDataCache = new Dictionary<string, Dictionary<string, AstRefData>>();
        
        // 场景资源引用数据
        private Dictionary<Scene, List<AstRefData>> _sceneRefData = new Dictionary<Scene, List<AstRefData>>();

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
            if (_sceneRefData.TryGetValue(scene, out var refDataList))
            {
                foreach (var refData in refDataList)
                {
                    if (refData.UnloadMode == UnloadMode.SceneUnload)
                    {
                        refData.Release();
                    }
                }
                _sceneRefData.Remove(scene);
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
        /// 异步加载资源（使用默认后端）
        /// </summary>
        public async Task<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode = UnloadMode.None, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await LoadAssetAsync<T>(address, unloadMode, _defaultBackend, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（指定后端）
        /// </summary>
        public async Task<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode, ResourceBackend backend, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await LoadAssetAsync<T>(address, unloadMode, backend, null, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（YooAsset包名）
        /// </summary>
        public async Task<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await LoadAssetAsync<T>(address, unloadMode, ResourceBackend.YooAsset, packageName, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（完整参数）
        /// </summary>
        public async Task<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode, ResourceBackend backend, string packageName = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogError("Asset address cannot be null or empty");
                return null;
            }

            var cacheKey = GetCacheKey(address, typeof(T), backend, packageName);
            var refData = GetOrCreateRefData(cacheKey, address, typeof(T), backend, packageName, unloadMode);

            if (refData != null)
            {
                // 检查句柄有效性
                if (!IsHandleValid(refData))
                {
                    throw new Exception("资源引用异常");
                }

                // 等待加载完成
                await WaitForHandleCompletion(refData, cancellationToken);

                refData.AddRef();
                return GetAssetFromRefData<T>(refData);
            }

            return null;
        }

        /// <summary>
        /// 批量异步加载资源
        /// </summary>
        public async Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, UnloadMode unloadMode = UnloadMode.None, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var tasks = new List<Task<T>>();
            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, unloadMode, cancellationToken));
            }
            
            var assets = await Task.WhenAll(tasks);
            return new List<T>(assets);
        }

        /// <summary>
        /// 批量异步加载资源（指定后端）
        /// </summary>
        public async Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, UnloadMode unloadMode, ResourceBackend backend, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var tasks = new List<Task<T>>();
            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, unloadMode, backend, cancellationToken));
            }
            
            var assets = await Task.WhenAll(tasks);
            return new List<T>(assets);
        }

        /// <summary>
        /// 批量异步加载资源（YooAsset包名）
        /// </summary>
        public async Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, UnloadMode unloadMode, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var tasks = new List<Task<T>>();
            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, unloadMode, packageName, cancellationToken));
            }
            
            var assets = await Task.WhenAll(tasks);
            return new List<T>(assets);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void ReleaseAsset<T>(string address, ResourceBackend? backend = null, string packageName = null)
        {
            var targetBackend = backend ?? _defaultBackend;
            var cacheKey = GetCacheKey(address, typeof(T), targetBackend, packageName);
            ReleaseAssetByCacheKey(cacheKey);
        }

        /// <summary>
        /// 释放包内所有资源
        /// </summary>
        public void ReleasePackage(string packageName)
        {
            if (_refDataCache.TryGetValue(packageName, out var packageCache))
            {
                foreach (var refData in packageCache.Values)
                {
                    refData.ForceRelease();
                }
                packageCache.Clear();
            }
        }

        /// <summary>
        /// 清空所有资源
        /// </summary>
        public void ClearAll()
        {
            foreach (var packageCache in _refDataCache.Values)
            {
                foreach (var refData in packageCache.Values)
                {
                    refData.ForceRelease();
                }
                packageCache.Clear();
            }
            _refDataCache.Clear();
            _sceneRefData.Clear();
        }

        /// <summary>
        /// 获取资源统计信息
        /// </summary>
        public void GetResourceStats(out int totalRefData, out int activeRefData)
        {
            totalRefData = 0;
            activeRefData = 0;

            foreach (var packageCache in _refDataCache.Values)
            {
                totalRefData += packageCache.Count;
                foreach (var refData in packageCache.Values)
                {
                    if (refData.RefCount > 0)
                    {
                        activeRefData++;
                    }
                }
            }
        }

        /// <summary>
        /// 获取缓存键
        /// </summary>
        private string GetCacheKey(string address, Type assetType, ResourceBackend backend, string packageName)
        {
            var backendStr = backend.ToString();
            var typeStr = assetType?.Name ?? "Object";
            var packageStr = packageName ?? "default";
            return $"{backendStr}_{packageStr}_{address}_{typeStr}";
        }

        /// <summary>
        /// 获取或创建资源引用数据
        /// </summary>
        private AstRefData GetOrCreateRefData(string cacheKey, string address, Type assetType, ResourceBackend backend, string packageName, UnloadMode unloadMode)
        {
            var packageKey = packageName ?? backend.ToString();
            
            if (!_refDataCache.TryGetValue(packageKey, out var packageCache))
            {
                packageCache = new Dictionary<string, AstRefData>();
                _refDataCache[packageKey] = packageCache;
            }

            if (packageCache.TryGetValue(cacheKey, out var existingRefData))
            {
                return existingRefData;
            }

            // 创建新的资源引用数据
            var refData = new AstRefData
            {
                BackendType = backend,
                PackageName = packageName,
                AssetType = assetType,
                UnloadMode = unloadMode
            };

            // 根据后端类型创建句柄
            switch (backend)
            {
                case ResourceBackend.YooAsset:
                    CreateYooAssetHandle(refData, address, assetType, packageName);
                    break;
                case ResourceBackend.Addressables:
#if UNITY_ADDRESSABLES
                    CreateAddressablesHandle(refData, address, assetType);
#else
                    Debug.LogError("Addressables is not available in this project");
                    return null;
#endif
                    break;
            }

            if (IsHandleValid(refData))
            {
                packageCache[cacheKey] = refData;
                
                // 如果是场景卸载模式，添加到场景句柄集合
                if (unloadMode == UnloadMode.SceneUnload)
                {
                    var currentScene = SceneManager.GetActiveScene();
                    if (!_sceneRefData.ContainsKey(currentScene))
                    {
                        _sceneRefData[currentScene] = new List<AstRefData>();
                    }
                    _sceneRefData[currentScene].Add(refData);
                }
            }

            return refData;
        }

        /// <summary>
        /// 创建YooAsset句柄
        /// </summary>
        private void CreateYooAssetHandle(AstRefData refData, string address, Type assetType, string packageName)
        {
            var package =  YooAssets.TryGetPackage(packageName);
            if (package == null)
            {
                Debug.LogError($"YooAsset package not found: {packageName}");
                return;
            }

            if (assetType != null)
            {
                refData.HandleYoo = package.LoadAssetAsync(address, assetType);
            }
            else
            {
                refData.HandleYoo = package.LoadAssetAsync<UnityEngine.Object>(address);
            }
        }

#if UNITY_ADDRESSABLES
        /// <summary>
        /// 创建Addressables句柄
        /// </summary>
        private void CreateAddressablesHandle(AstRefData refData, string address, Type assetType)
        {
            refData.HandleAA = Addressables.LoadAssetAsync<UnityEngine.Object>(address);
        }
#endif

        /// <summary>
        /// 检查句柄是否有效
        /// </summary>
        private bool IsHandleValid(AstRefData refData)
        {
            switch (refData.BackendType)
            {
                case ResourceBackend.YooAsset:
                    return refData.HandleYoo.IsValid;
                case ResourceBackend.Addressables:
#if UNITY_ADDRESSABLES
                    return refData.HandleAA.IsValid();
#endif
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 等待句柄完成
        /// </summary>
        private async Task WaitForHandleCompletion(AstRefData refData, CancellationToken cancellationToken)
        {
            switch (refData.BackendType)
            {
                case ResourceBackend.YooAsset:
                    if (!refData.HandleYoo.IsDone)
                    {
                        await refData.HandleYoo.Task;
                    }
                    break;
                case ResourceBackend.Addressables:
//#if UNITY_ADDRESSABLES
                    if (!refData.HandleAA.IsDone)
                    {
                        await refData.HandleAA.Task;
                    }
//#endif
                    break;
            }
        }

        /// <summary>
        /// 从引用数据获取资源
        /// </summary>
        private T GetAssetFromRefData<T>(AstRefData refData) where T : UnityEngine.Object
        {
            switch (refData.BackendType)
            {
                case ResourceBackend.YooAsset:
                    return refData.HandleYoo.AssetObject as T;
                case ResourceBackend.Addressables:
#if UNITY_ADDRESSABLES
                    return refData.HandleAA.Result as T;
#endif
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 通过缓存键释放资源
        /// </summary>
        private void ReleaseAssetByCacheKey(string cacheKey)
        {
            foreach (var packageCache in _refDataCache.Values)
            {
                if (packageCache.TryGetValue(cacheKey, out var refData))
                {
                    refData.Release();
                    break;
                }
            }
        }
    }
}

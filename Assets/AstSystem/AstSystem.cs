//#define UNITY_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        
        // 精灵图集映射
        private SpriteAtlasMapping _spriteAtlasMapping;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneUnloaded += OnSceneUnloaded;
                InitializeSpriteAtlasMapping();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化精灵图集映射
        /// </summary>
        private void InitializeSpriteAtlasMapping()
        {
            if (_spriteAtlasMapping == null)
            {
                _spriteAtlasMapping = Resources.Load<SpriteAtlasMapping>("SpriteAtlasMapping");
                if (_spriteAtlasMapping != null)
                {
                    _spriteAtlasMapping.Initialize();
                    Debug.Log($"精灵图集映射加载成功，共 {_spriteAtlasMapping.Count} 个精灵");
                }
                else
                {
                    Debug.LogWarning("未找到精灵图集映射文件，请在编辑器中生成映射");
                }
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
                    refData.ForceRelease();
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
        public async UniTask<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode = UnloadMode.None, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await LoadAssetAsync<T>(address, unloadMode, _defaultBackend, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（指定后端）
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode, ResourceBackend backend, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await LoadAssetAsync<T>(address, unloadMode, backend, null, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（YooAsset包名）
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await LoadAssetAsync<T>(address, unloadMode, ResourceBackend.YooAsset, packageName, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（完整参数）
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode, ResourceBackend backend, string packageName = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
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
        public async UniTask<List<T>> LoadAssetsAsync<T>(List<string> addresses, UnloadMode unloadMode = UnloadMode.None, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var tasks = new List<UniTask<T>>();
            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, unloadMode, cancellationToken));
            }
            
            var assets = await UniTask.WhenAll(tasks);
            return new List<T>(assets);
        }

        /// <summary>
        /// 批量异步加载资源（指定后端）
        /// </summary>
        public async UniTask<List<T>> LoadAssetsAsync<T>(List<string> addresses, UnloadMode unloadMode, ResourceBackend backend, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var tasks = new List<UniTask<T>>();
            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, unloadMode, backend, cancellationToken));
            }
            
            var assets = await UniTask.WhenAll(tasks);
            return new List<T>(assets);
        }

        /// <summary>
        /// 批量异步加载资源（YooAsset包名）
        /// </summary>
        public async UniTask<List<T>> LoadAssetsAsync<T>(List<string> addresses, UnloadMode unloadMode, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var tasks = new List<UniTask<T>>();
            foreach (var address in addresses)
            {
                tasks.Add(LoadAssetAsync<T>(address, unloadMode, packageName, cancellationToken));
            }
            
            var assets = await UniTask.WhenAll(tasks);
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
                AssetType = assetType
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
        private async UniTask WaitForHandleCompletion(AstRefData refData, CancellationToken cancellationToken)
        {
            switch (refData.BackendType)
            {
                case ResourceBackend.YooAsset:
                    if (!refData.HandleYoo.IsDone)
                    {
                        await refData.HandleYoo.ToUniTask(cancellationToken: cancellationToken);
                    }
                    break;
                case ResourceBackend.Addressables:
#if UNITY_ADDRESSABLES
                    if (!refData.HandleAA.IsDone)
                    {
                        await refData.HandleAA.ToUniTask(cancellationToken: cancellationToken);
                    }
#endif
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
        
        #region 智能Sprite加载
        
        /// <summary>
        /// 智能加载Sprite（自动判断图集或散图）
        /// </summary>
        public async UniTask<Sprite> LoadSpriteAsync(string spriteName, UnloadMode unloadMode = UnloadMode.None, CancellationToken cancellationToken = default)
        {
            return await LoadSpriteAsync(spriteName, unloadMode, _defaultBackend, null, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载Sprite（指定后端）
        /// </summary>
        public async UniTask<Sprite> LoadSpriteAsync(string spriteName, UnloadMode unloadMode, ResourceBackend backend, CancellationToken cancellationToken = default)
        {
            return await LoadSpriteAsync(spriteName, unloadMode, backend, null, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载Sprite（YooAsset包名）
        /// </summary>
        public async UniTask<Sprite> LoadSpriteAsync(string spriteName, UnloadMode unloadMode, string packageName, CancellationToken cancellationToken = default)
        {
            return await LoadSpriteAsync(spriteName, unloadMode, ResourceBackend.YooAsset, packageName, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载Sprite（完整参数）
        /// 逻辑：1.先查图集映射 2.有则从图集加载 3.无则当散图直接加载
        /// </summary>
        public async UniTask<Sprite> LoadSpriteAsync(string spriteName, UnloadMode unloadMode, ResourceBackend backend, string packageName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                Debug.LogError("Sprite name cannot be null or empty");
                return null;
            }
            
            // 确保映射已初始化
            InitializeSpriteAtlasMapping();
            
            // 1. 先检查是否在图集中
            if (_spriteAtlasMapping != null && _spriteAtlasMapping.IsInAtlas(spriteName))
            {
                // 2. 从图集加载
                var atlasAddress = _spriteAtlasMapping.GetAtlasAddress(spriteName);
                return await LoadSpriteFromAtlasInternal(atlasAddress, spriteName, unloadMode, backend, packageName, cancellationToken);
            }
            else
            {
                // 3. 散图直接加载
                return await LoadAssetAsync<Sprite>(spriteName, unloadMode, backend, packageName, cancellationToken);
            }
        }
        
        /// <summary>
        /// 批量智能加载Sprite
        /// </summary>
        public async UniTask<List<Sprite>> LoadSpritesAsync(List<string> spriteNames, UnloadMode unloadMode = UnloadMode.None, CancellationToken cancellationToken = default)
        {
            var tasks = new List<UniTask<Sprite>>();
            foreach (var spriteName in spriteNames)
            {
                tasks.Add(LoadSpriteAsync(spriteName, unloadMode, cancellationToken));
            }
            
            var sprites = await UniTask.WhenAll(tasks);
            return new List<Sprite>(sprites);
        }
        
        /// <summary>
        /// 从图集内部加载精灵
        /// 注意：图集可能已经被其他资源（如预制体）依赖加载，此时单独加载图集精灵不会重复加载AB
        /// </summary>
        private async UniTask<Sprite> LoadSpriteFromAtlasInternal(string atlasAddress, string spriteName, UnloadMode unloadMode, ResourceBackend backend, string packageName, CancellationToken cancellationToken)
        {
            // 构造图集中精灵的地址格式：atlas[sprite]
            var spriteInAtlasAddress = $"{atlasAddress}[{spriteName}]";
            
            // 对于图集精灵，使用特殊的缓存键来区分单独加载和依赖加载
            var spriteKey = GetCacheKey(spriteInAtlasAddress, typeof(Sprite), backend, packageName);
            var atlasKey = GetCacheKey(atlasAddress, typeof(UnityEngine.U2D.SpriteAtlas), backend, packageName);
            
            // 检查图集是否已经被其他方式加载（如预制体依赖）
            var existingAtlasRefData = FindExistingRefData(atlasKey);
            if (existingAtlasRefData != null)
            {
                Debug.Log($"图集 {atlasAddress} 已存在（可能被预制体依赖加载），直接获取精灵");
                // 图集已存在，直接加载精灵，这不会触发新的AB加载
            }
            
            return await LoadAssetAsync<Sprite>(spriteInAtlasAddress, unloadMode, backend, packageName, cancellationToken);
        }
        
        /// <summary>
        /// 查找已存在的引用数据
        /// </summary>
        private AstRefData FindExistingRefData(string cacheKey)
        {
            foreach (var packageCache in _refDataCache.Values)
            {
                if (packageCache.TryGetValue(cacheKey, out var refData))
                {
                    return refData;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 释放Sprite资源
        /// </summary>
        public void ReleaseSprite(string spriteName, ResourceBackend? backend = null, string packageName = null)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                Debug.LogError("Sprite name cannot be null or empty");
                return;
            }
            
            var targetBackend = backend ?? _defaultBackend;
            
            // 确保映射已初始化
            InitializeSpriteAtlasMapping();
            
            // 1. 先检查是否在图集中
            if (_spriteAtlasMapping != null && _spriteAtlasMapping.IsInAtlas(spriteName))
            {
                // 2. 释放图集中的精灵
                var atlasAddress = _spriteAtlasMapping.GetAtlasAddress(spriteName);
                var spriteInAtlasAddress = $"{atlasAddress}[{spriteName}]";
                var cacheKey = GetCacheKey(spriteInAtlasAddress, typeof(Sprite), targetBackend, packageName);
                ReleaseAssetByCacheKey(cacheKey);
                
                Debug.Log($"释放图集精灵: {spriteName} (来自图集: {atlasAddress})");
            }
            else
            {
                // 3. 释放散图
                var cacheKey = GetCacheKey(spriteName, typeof(Sprite), targetBackend, packageName);
                ReleaseAssetByCacheKey(cacheKey);
                
                Debug.Log($"释放散图精灵: {spriteName}");
            }
        }
        
        /// <summary>
        /// 获取精灵信息（调试用）
        /// </summary>
        public SpriteAtlasMapping.AtlasSpriteInfo GetSpriteInfo(string spriteName)
        {
            InitializeSpriteAtlasMapping();
            return _spriteAtlasMapping?.GetAtlasSpriteInfo(spriteName);
        }
        
        /// <summary>
        /// 检查精灵是否在图集中
        /// </summary>
        public bool IsSpriteInAtlas(string spriteName)
        {
            InitializeSpriteAtlasMapping();
            return _spriteAtlasMapping?.IsInAtlas(spriteName) ?? false;
        }
        
        /// <summary>
        /// 获取精灵的图集地址
        /// </summary>
        public string GetSpriteAtlasAddress(string spriteName)
        {
            InitializeSpriteAtlasMapping();
            return _spriteAtlasMapping?.GetAtlasAddress(spriteName);
        }
        
        #endregion
    }
}

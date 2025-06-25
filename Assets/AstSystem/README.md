# AstSystem 统一资源管理系统

## 概述

AstSystem 是一个统一的资源管理系统，支持 YooAsset 和 Addressables 两种后端。系统采用直接管理原始句柄的方式，避免了过度抽象，更加实用和高效。

## 设计理念

### 简化设计
- **直接管理原始句柄**：不强制统一 `AsyncOperationHandle` 和 `AssetHandle`
- **最小化抽象**：避免过度抽象，保持代码直观性
- **实用优先**：专注于解决实际问题

### 参考 MyAst.cs 设计
```csharp
public class AstRefData
{
    public AsyncOperationHandle HandleA;  // Addressables句柄
    public AssetHandle HandleB;           // YooAsset句柄
    public int RefCount;
    // ...
}
```

## 核心组件

### AstRefData 类
```csharp
public class AstRefData
{
    public ResourceBackend BackendType { get; set; }
    public string PackageName { get; set; }
    public AsyncOperationHandle HandleA { get; set; }  // Addressables
    public AssetHandle HandleB { get; set; }           // YooAsset
    public Type AssetType { get; set; }
    public int RefCount { get; private set; }
    public UnloadMode UnloadMode { get; set; }
    
    public void AddRef();
    public void Release();
    public void ForceRelease();
}
```

### AstSystem 主类
```csharp
public class AstSystem : MonoBehaviour
{
    public static AstSystem Instance { get; }
    
    // 加载方法
    public async Task<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode = UnloadMode.None, CancellationToken cancellationToken = default);
    public async Task<T> LoadAssetAsync<T>(string address, UnloadMode unloadMode, string packageName, CancellationToken cancellationToken = default);
    
    // 批量加载
    public async Task<List<T>> LoadAssetsAsync<T>(List<string> addresses, UnloadMode unloadMode = UnloadMode.None, CancellationToken cancellationToken = default);
    
    // 释放方法
    public void ReleaseAsset<T>(string address, ResourceBackend? backend = null, string packageName = null);
    public void ReleasePackage(string packageName);
    public void ClearAll();
}
```

## 使用方法

### 基本使用
```csharp
var astSystem = AstSystem.Instance;

// 基本加载
var asset = await astSystem.LoadAssetAsync<GameObject>("Sphere");

// 包名加载（YooAsset）
var asset = await astSystem.LoadAssetAsync<GameObject>("Sphere", UnloadMode.None, "DefaultPackage");

// 场景卸载模式
var asset = await astSystem.LoadAssetAsync<GameObject>("Sphere", UnloadMode.SceneUnload);
```

### 扩展方法
```csharp
// 简化加载
var asset = await astSystem.LoadAsset<GameObject>("Sphere");

// YooAsset 包名加载
var asset = await astSystem.LoadYooAsset<GameObject>("Sphere", "DefaultPackage");

// Addressables 加载
var asset = await astSystem.LoadAddressable<GameObject>("Sphere");
```

### 批量加载
```csharp
var addresses = new List<string> { "Sphere", "Cube" };
var assets = await astSystem.LoadAssets<GameObject>(addresses);
```

### 取消令牌
```csharp
var cts = new CancellationTokenSource();
try
{
    var asset = await astSystem.LoadAssetAsync<GameObject>("Sphere", UnloadMode.None, cts.Token);
}
catch (OperationCanceledException)
{
    Debug.Log("加载被取消");
}
```

### 资源释放
```csharp
// 释放单个资源
astSystem.ReleaseAsset<GameObject>("Sphere");

// 释放包内所有资源
astSystem.ReleasePackage("DefaultPackage");

// 清空所有资源
astSystem.ClearAll();
```

## 卸载模式

- **None**：不自动卸载
- **SceneUnload**：场景卸载时释放
- **NextFrame**：下一帧释放
- **AutoRelease**：自动释放

## 主要特性

- ✅ 统一接口支持 YooAsset 和 Addressables
- ✅ 引用计数管理
- ✅ 包名支持
- ✅ 取消令牌支持
- ✅ 多种卸载模式
- ✅ 批量加载
- ✅ 扩展方法
- ✅ 简化的设计

## 注意事项

1. 系统直接管理原始句柄类型，不强制统一抽象
2. 建议在不需要资源时主动释放
3. 长时间加载操作建议使用 CancellationToken
4. 使用 YooAsset 时确保包名正确且包已初始化 
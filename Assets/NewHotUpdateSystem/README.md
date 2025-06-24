# NewHotUpdateSystem - YooAsset 热更新系统

这是一个基于 YooAsset 的简化热更新系统，保持了原有架构的完整性，同时提供了更便捷的使用方式。

## 设计理念

1. **保持YooAsset原生架构**：不引入复杂抽象，保持原有状态机流程
2. **支持多Package**：提供Package管理工具
3. **配置化**：将硬编码改为可配置参数
4. **原生事件**：使用Unity原生事件系统，无第三方依赖

## 核心特性

- ✅ **完整的热更新流程**：初始化、版本检查、清单更新、文件下载、缓存清理
- ✅ **阶段枚举**：清晰的状态追踪和事件通知
- ✅ **多Package支持**：支持批量更新多个Package
- ✅ **配置化**：服务器地址、下载参数等可配置
- ✅ **原生事件系统**：使用Unity原生事件，无第三方依赖
- ✅ **工具类**：提供便捷的使用方法
- ✅ **向后兼容**：保持与原始API的兼容性

## 目录结构

```
Assets/NewHotUpdateSystem/
├── Core/                    # 核心系统
│   ├── PatchOperation.cs    # 热更新操作（核心类）
│   └── ServerConfig.cs      # 服务器配置
├── Events/                  # 事件系统
│   └── HotUpdateEnums.cs    # 阶段枚举和事件定义
├── FSM/                     # 状态机
│   ├── StateMachine.cs      # 状态机基类
│   ├── BaseStateNode.cs     # 状态节点基类
│   └── States/              # 状态节点
│       ├── FsmInitializePackage.cs
│       ├── FsmRequestPackageVersion.cs
│       ├── FsmUpdatePackageManifest.cs
│       ├── FsmCreateDownloader.cs
│       ├── FsmDownloadPackageFiles.cs
│       ├── FsmDownloadPackageOver.cs
│       ├── FsmClearCacheBundle.cs
│       └── FsmStartGame.cs
├── Utils/                   # 工具类
│   └── HotUpdateHelper.cs   # 热更新工具类
├── Examples/                # 示例
│   └── HotUpdateExample.cs  # 使用示例
└── README.md               # 说明文档
```

## 快速开始

### 1. 极简模式（推荐新手）

```csharp
public class SimpleHotUpdateStarter : MonoBehaviour
{
    [SerializeField] private bool _showDebugLog = true;
    
    void Start()
    {
        StartCoroutine(StartHotUpdate());
    }
    
    private IEnumerator StartHotUpdate()
    {
        // 1. 初始化YooAsset
        YooAssets.Initialize();
        
        // 2. 创建热更新操作（需要指定PlayMode）
        var operation = HotUpdateHelper.StartHotUpdate(EPlayMode.HostPlayMode);
        
        // 3. 设置协程运行器（必须在开始操作之前）
        operation.SetCoroutineRunner(this);
        
        // 4. 注册事件
        operation.OnProgressChanged += OnProgressChanged;
        operation.OnError += OnError;
        operation.OnCompleted += OnCompleted;
        
        // 5. 开始热更新操作
        HotUpdateHelper.StartOperation(operation);
        
        // 6. 等待完成
        yield return operation;
    }
}
```

### 2. 配置模式（推荐项目）

```csharp
public class ConfigurableHotUpdateStarter : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private string _packageName = "DefaultPackage";
    [SerializeField] private EPlayMode _playMode = EPlayMode.HostPlayMode;
    [SerializeField] private ServerConfig _serverConfig;
    
    private IEnumerator StartHotUpdate()
    {
        // 1. 初始化YooAsset
        YooAssets.Initialize();
        
        // 2. 获取配置
        var config = GetServerConfig();
        
        // 3. 创建热更新操作
        var operation = HotUpdateHelper.StartHotUpdateWithConfig(_packageName, _playMode, config);
        
        // 4. 设置协程运行器（必须在开始操作之前）
        operation.SetCoroutineRunner(this);
        
        // 5. 注册事件
        operation.OnStageChanged += OnStageChangedEvent;
        operation.OnProgressChanged += OnProgressChangedEvent;
        operation.OnError += OnErrorEvent;
        operation.OnCompleted += OnCompletedEvent;
        
        // 6. 开始热更新操作
        HotUpdateHelper.StartOperation(operation);
        
        // 7. 等待完成
        yield return operation;
    }
}
```

### 3. 完整模式（推荐定制）

```csharp
// 完全自定义
var operation = new PatchOperation("DefaultPackage", EPlayMode.HostPlayMode, config);
operation.OnStageChanged += OnStageChanged;
operation.OnProgressChanged += OnProgressChanged;
operation.OnError += OnError;
operation.OnCompleted += OnCompleted;
operation.SetCoroutineRunner(this);  // 重要：设置协程运行器
YooAssets.StartOperation(operation);
yield return operation;
```

### 4. 多Package模式

```csharp
// 批量更新多个Package
string[] packageNames = { "DefaultPackage", "UI", "Audio" };
yield return HotUpdateHelper.UpdatePackages(packageNames, EPlayMode.HostPlayMode, config);
```

## 事件系统

### 事件类型

- `OnStageChanged`：阶段变化事件
- `OnProgressChanged`：进度变化事件
- `OnError`：错误事件
- `OnCompleted`：完成事件

### 事件参数

```csharp
public class HotUpdateEventArgs : EventArgs
{
    public string PackageName;           // 包名称
    public HotUpdateStage CurrentStage;  // 当前阶段
    public HotUpdateStage PreviousStage; // 上一个阶段
    public string Message;               // 消息内容
    public float Progress;               // 进度 (0-1)
    public bool IsError;                 // 是否错误
    public string ErrorMessage;          // 错误信息
    public DownloadInfo DownloadInfo;    // 下载信息
}
```

### 使用示例

```csharp
private void OnStageChanged(object sender, HotUpdateEventArgs e)
{
    if (e.PreviousStage != HotUpdateStage.None)
    {
        Debug.Log($"阶段变化: {e.PreviousStage} → {e.CurrentStage}");
    }
}

private void OnProgressChanged(object sender, HotUpdateEventArgs e)
{
    Debug.Log($"进度: {e.Progress * 100:F1}% - {e.Message}");
    
    if (e.DownloadInfo != null)
    {
        Debug.Log($"下载: {e.DownloadInfo.CurrentCount}/{e.DownloadInfo.TotalCount} 文件");
    }
}

private void OnError(object sender, HotUpdateEventArgs e)
{
    Debug.LogError($"错误: {e.ErrorMessage}");
}

private void OnCompleted(object sender, HotUpdateEventArgs e)
{
    Debug.Log("热更新完成");
}
```

## 阶段枚举

```csharp
public enum HotUpdateStage
{
    None = 0,                    // 未开始
    InitializePackage = 1,       // 初始化包
    RequestPackageVersion = 2,   // 请求包版本
    UpdatePackageManifest = 3,   // 更新包清单
    CreateDownloader = 4,        // 创建下载器
    DownloadPackageFiles = 5,    // 下载包文件
    DownloadPackageOver = 6,     // 下载完成
    ClearCacheBundle = 7,        // 清理缓存
    StartGame = 8,               // 开始游戏
    Completed = 9,               // 完成
    Failed = 10                  // 失败
}
```

## 配置系统

### ServerConfig 配置项

```csharp
[Header("服务器配置")]
public string MainServerURL = "http://127.0.0.1";
public string FallbackServerURL = "http://127.0.0.1";
public string AppVersion = "v1.0";

[Header("平台配置")]
public string AndroidCDN = "/CDN/Android/{0}";
public string IOSCDN = "/CDN/IPhone/{0}";
public string WebGLCDN = "/CDN/WebGL/{0}";
public string PCCDN = "/CDN/PC/{0}";

[Header("下载配置")]
public int MaxConcurrentDownloads = 10;
public int DownloadRetryCount = 3;

[Header("缓存配置")]
public bool AutoClearCache = true;
public EFileClearMode CacheClearMode = EFileClearMode.ClearUnusedBundleFiles;
```

## 工具类

### HotUpdateHelper 主要方法

```csharp
// 极简模式启动热更新（需要指定PlayMode）
public static PatchOperation StartHotUpdate(EPlayMode playMode)

// 配置模式启动热更新
public static PatchOperation StartHotUpdate(EPlayMode playMode, ServerConfig config)

// 参数模式启动热更新
public static PatchOperation StartHotUpdate(string packageName, EPlayMode playMode, string serverURL = null)

// 完整参数启动热更新
public static PatchOperation StartHotUpdateWithConfig(string packageName, EPlayMode playMode, ServerConfig serverConfig)

// 批量更新
public static IEnumerator UpdatePackages(string[] packageNames, EPlayMode playMode, ServerConfig serverConfig = null)

// 网络检查
public static bool IsNetworkAvailable()
public static bool IsWifiConnected()

// 磁盘空间检查
public static long GetAvailableDiskSpace()
public static bool HasEnoughDiskSpace(long requiredBytes)

// 文件大小格式化
public static string FormatFileSize(long bytes)

// 缓存管理
public static void ClearAllCache()

// 包信息获取
public static PackageInfo GetPackageInfo(string packageName)
```

### PlayMode 选择指南

根据不同的使用场景选择合适的PlayMode：

- **开发阶段**：`EditorSimulateMode` - 编辑器模拟模式，快速测试
- **测试阶段**：`HostPlayMode` - 联机运行模式，测试网络功能
- **生产环境**：`HostPlayMode` - 联机运行模式，支持热更新
- **离线模式**：`OfflinePlayMode` - 单机运行模式，无网络依赖
- **WebGL平台**：`WebPlayMode` - WebGL运行模式

## 与原始系统的对比

### 优势

1. **保持原有架构**：状态机流程完全一致
2. **增强事件系统**：提供阶段追踪和详细进度信息
3. **配置化**：服务器地址等可配置，不再硬编码
4. **多Package支持**：支持批量更新多个Package
5. **工具类**：提供便捷的使用方法
6. **无第三方依赖**：使用Unity原生事件系统

### 兼容性

- 保持与原始API的兼容性
- 可以逐步迁移到新系统
- 支持混合使用

## 使用建议

### 1. 新手使用（推荐）

```csharp
// 最简单的使用方式 - 需要指定PlayMode
var operation = HotUpdateHelper.StartHotUpdate(EPlayMode.HostPlayMode);
operation.SetCoroutineRunner(this);  // 重要：设置协程运行器
operation.OnCompleted += OnCompleted;
HotUpdateHelper.StartOperation(operation);
```

### 2. 项目使用

```csharp
// 使用配置文件
var config = ServerConfig.CreateDefault();
config.MainServerURL = "http://your-server.com";
var operation = HotUpdateHelper.StartHotUpdate(EPlayMode.HostPlayMode, config);
operation.SetCoroutineRunner(this);  // 重要：设置协程运行器
HotUpdateHelper.StartOperation(operation);
```

### 3. 高级使用

```csharp
// 完全自定义
var operation = new PatchOperation("DefaultPackage", EPlayMode.HostPlayMode, config);
// 注册所有事件
operation.OnStageChanged += OnStageChanged;
operation.OnProgressChanged += OnProgressChanged;
operation.OnError += OnError;
operation.OnCompleted += OnCompleted;
operation.SetCoroutineRunner(this);  // 重要：设置协程运行器
HotUpdateHelper.StartOperation(operation);
```

### 4. PlayMode选择建议

- **开发阶段**：使用 `EditorSimulateMode` 进行快速测试
- **测试阶段**：使用 `HostPlayMode` 进行网络功能测试
- **生产环境**：使用 `HostPlayMode` 支持热更新功能
- **离线场景**：使用 `OfflinePlayMode` 无网络依赖运行

## 注意事项

1. **协程运行器**：需要设置协程运行器才能启动协程
2. **事件注册**：建议在开始热更新前注册所有事件
3. **错误处理**：每个阶段都有对应的错误处理
4. **网络检查**：建议在开始前检查网络连接
5. **磁盘空间**：下载前检查磁盘空间是否足够

## 总结

NewHotUpdateSystem 在保持 YooAsset 原有架构的基础上，提供了更便捷的使用方式和更强大的功能。通过阶段枚举和事件系统，可以更好地追踪热更新进度和处理各种情况。同时支持多Package和配置化，满足不同项目的需求。

**关键改进：**
- ✅ 完整的协程管理
- ✅ 改进的错误处理
- ✅ 与原始YooAsset的完全兼容
- ✅ 丰富的示例和文档
- ✅ 多种使用模式
- ✅ **清晰的API设计**：需要明确指定PlayMode，避免混淆
- ✅ **灵活的模式选择**：支持多种PlayMode适应不同场景 

## 🌐 网络请求实现分析

### RequestPackageVersionAsync() 深度解析

#### 📋 完整调用链路
```
package.RequestPackageVersionAsync()
    ↓
RequestPackageVersionImplOperation (ResourcePackage/Operation/)
    ↓
PlayModeImpl.RequestPackageVersionAsync()
    ↓
FileSystem.RequestPackageVersionAsync() 
    ↓ (根据不同的文件系统类型)
├── DefaultCacheFileSystem → DCFSRequestPackageVersionOperation
├── DefaultWebRemoteFileSystem → DWRFSRequestPackageVersionOperation  
├── DefaultWebServerFileSystem → DWSFSRequestPackageVersionOperation
└── DefaultBuildinFileSystem → DBFSRequestPackageVersionOperation
    ↓
具体的RequestXXXPackageVersionOperation
    ↓
UnityWebTextRequestOperation
    ↓
UnityWebRequestOperation (基类)
    ↓
UnityWebRequest (Unity原生网络请求)
```

#### 🔧 核心实现机制

**1. UnityWebTextRequestOperation**
```csharp
private void CreateWebRequest()
{
    _webRequest = DownloadSystemHelper.NewUnityWebRequestGet(_requestURL);
    DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
    _webRequest.downloadHandler = handler;
    _webRequest.disposeDownloadHandlerOnDispose = true;
    _requestOperation = _webRequest.SendWebRequest();
}
```

**2. URL构建过程**
```csharp
private string GetWebRequestURL(string fileName)
{
    string url;
    if (_requestCount % 2 == 0)
        url = _fileSystem.RemoteServices.GetRemoteMainURL(fileName);
    else
        url = _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName);

    if (_appendTimeTicks)
        return $"{url}?{System.DateTime.UtcNow.Ticks}";
    else
        return url;
}
```

**3. IRemoteServices接口实现**
```csharp
private class RemoteServices : IRemoteServices
{
    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }
    
    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }
    
    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }
}
```

#### 🔗 URL构建示例

根据配置文件 `DevelopmentServerConfig.asset`：

```
MainServerURL: http://127.0.0.1:3000/
FallbackServerURL: http://127.0.0.1:3000/
```

假设包名为 `DefaultPackage`，最终请求的URL会是：
```
http://127.0.0.1:3000/assets/PC/DefaultPackage_version.txt?6371234567890123456
```

#### 📁 文件名生成
```csharp
string fileName = YooAssetSettingsData.GetPackageVersionFileName(_fileSystem.PackageName);
// 生成类似: "DefaultPackage_version.txt"
```

#### 🔄 请求流程详解

1. **初始化阶段**：创建 RequestPackageVersionImplOperation，根据运行模式选择对应的文件系统
2. **URL构建阶段**：获取包版本文件名，通过 IRemoteServices 构建完整URL，可选择添加时间戳防止缓存
3. **网络请求阶段**：创建 UnityWebTextRequestOperation，使用 UnityWebRequest 发送GET请求，设置 DownloadHandlerBuffer 接收文本数据
4. **结果处理阶段**：检查请求状态，解析返回的版本号，返回结果或错误信息

#### 🎯 关键特点

- 异步操作：所有网络请求都是异步的，通过 OperationSystem 管理
- 容错机制：支持主服务器和备用服务器轮询
- 缓存控制：可添加时间戳防止浏览器缓存
- 超时控制：支持请求超时设置
- 错误重试：通过 WebRequestCounter 记录失败次数

#### 💡 为什么难以直接找到WebRequest

YooAsset的网络请求是通过以下层次封装的：
- 最底层：UnityWebRequest (Unity原生)
- 中间层：UnityWebRequestOperation (YooAsset封装)
- 业务层：各种具体的Request操作类

这种设计使得网络请求逻辑被很好地封装和抽象，便于统一管理和扩展。
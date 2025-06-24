# YooAsset Runtime 架构学习笔记

## 📚 概述

YooAsset是一个Unity资源管理框架，本文档记录了对其Runtime结构的深入分析和学习成果。

## 🏗️ 整体架构层次

### 1. 入口层 (Entry Layer)

#### YooAssets.cs - 全局静态入口类
- **职责**: 管理所有资源包的生命周期
- **核心功能**:
  - 全局初始化和销毁
  - 资源包创建和管理
  - 维护默认资源包引用
  - 系统参数配置

#### YooAssetsExtension.cs - 扩展功能类
- **职责**: 提供便捷的资源加载API
- **支持资源类型**:
  - Asset (普通资源)
  - Scene (场景)
  - RawFile (原生文件)
  - SubAssets (子资源)
  - AllAssets (所有资源)

### 2. 资源包层 (Package Layer)

#### ResourcePackage.cs - 资源包核心类
- **职责**: 管理单个资源包的完整生命周期
- **支持运行模式**:
  - EditorSimulateMode (编辑器模拟)
  - OfflinePlayMode (离线模式)
  - HostPlayMode (主机模式)
  - WebPlayMode (Web模式)
  - CustomPlayMode (自定义模式)

## 🔧 核心系统组件

### 1. 文件系统 (FileSystem)
```
FileSystem/
├── Interface/                    # 文件系统接口定义
├── DefaultBuildinFileSystem/     # 内置文件系统实现
├── DefaultCacheFileSystem/       # 缓存文件系统实现
├── DefaultEditorFileSystem/      # 编辑器文件系统实现
├── DefaultWebRemoteFileSystem/   # 远程文件系统实现
├── DefaultWebServerFileSystem/   # Web服务器文件系统
├── DefaultUnpackFileSystem/      # 解包文件系统
└── Operation/                    # 文件操作实现
```

**核心功能**:
- 文件读取和写入
- 缓存管理
- 文件验证
- 跨平台文件访问

### 2. 下载系统 (DownloadSystem)
```
DownloadSystem/
├── Operation/                    # 下载操作实现
├── DownloadDefine.cs             # 下载相关定义
├── DownloadSystemHelper.cs       # 下载辅助工具
└── WebRequestCounter.cs          # 网络请求计数器
```

**核心功能**:
- 资源下载管理
- 断点续传
- 并发控制
- 错误重试

### 3. 资源管理器 (ResourceManager)
```
ResourceManager/
├── Handle/                       # 资源句柄实现
├── Operation/                    # 资源操作实现
├── Provider/                     # 资源提供者实现
└── ResourceManager.cs            # 资源管理器核心
```

**核心功能**:
- Provider和Loader生命周期管理
- 资源依赖关系处理
- 并发加载控制
- 资源卸载管理

### 4. 资源包 (ResourcePackage)
```
ResourcePackage/
├── Interface/                    # 包接口定义
├── Operation/                    # 包操作实现
├── PlayMode/                     # 运行模式实现
├── ResourcePackage.cs            # 资源包核心类
├── PackageManifest.cs            # 包清单管理
├── AssetInfo.cs                  # 资源信息
├── BundleInfo.cs                 # Bundle信息
└── ManifestTools.cs              # 清单工具
```

**核心功能**:
- 资源包生命周期管理
- 清单文件处理
- 资源信息查询
- 运行模式切换

### 5. 操作管理系统 (OperationSystem)
```
OperationSystem/
├── AsyncOperationBase.cs         # 异步操作基类
├── GameAsyncOperation.cs         # 游戏异步操作
├── OperationSystem.cs            # 操作调度器
└── EOperationStatus.cs           # 操作状态枚举
```

**核心功能**:
- 异步操作调度
- 优先级管理
- 时间切片控制
- 操作状态跟踪

## 🎯 关键设计特点

### 1. 多包支持
- **优势**: 支持创建多个独立的资源包
- **应用**: 每个包可以有不同的配置和运行模式
- **好处**: 包之间相互隔离，便于模块化管理

### 2. 多种运行模式
- **EditorSimulateMode**: 编辑器模拟模式，用于开发调试
- **OfflinePlayMode**: 离线模式，仅使用内置资源
- **HostPlayMode**: 主机模式，内置资源 + 缓存资源
- **WebPlayMode**: Web模式，适用于WebGL平台
- **CustomPlayMode**: 自定义模式，可扩展自定义逻辑

### 3. 异步操作管理
- **统一基类**: AsyncOperationBase提供统一的异步操作接口
- **优先级调度**: 支持操作优先级设置
- **时间切片**: 控制每帧执行时间，避免阻塞主线程
- **状态管理**: 完整的操作状态跟踪

### 4. 资源句柄系统
- **句柄类型**:
  - AssetHandle (普通资源句柄)
  - SceneHandle (场景句柄)
  - RawFileHandle (原生文件句柄)
  - SubAssetsHandle (子资源句柄)
  - AllAssetsHandle (所有资源句柄)
- **特性**: 支持同步和异步加载，自动管理资源生命周期

### 5. 缓存管理
- **内置缓存**: DefaultCacheFileSystem提供缓存功能
- **缓存清理**: 支持多种清理模式
- **缓存验证**: 文件完整性验证
- **智能策略**: 自动缓存管理策略

## 🔄 工作流程

### 1. 初始化阶段
```
YooAssets.Initialize() 
    ↓
创建驱动器 (YooAssetsDriver)
    ↓
初始化操作系统 (OperationSystem.Initialize())
```

### 2. 创建资源包
```
YooAssets.CreatePackage(packageName)
    ↓
创建ResourcePackage实例
    ↓
配置运行模式和参数
```

### 3. 初始化资源包
```
ResourcePackage.InitializeAsync(parameters)
    ↓
根据运行模式创建对应的文件系统
    ↓
加载和解析清单文件
    ↓
初始化资源管理器
```

### 4. 资源加载流程
```
资源请求 (LoadAssetAsync/LoadSceneAsync等)
    ↓
ResourceManager处理请求
    ↓
创建Provider (AssetProvider/SceneProvider等)
    ↓
Provider创建Loader (LoadBundleFileOperation)
    ↓
Loader加载Bundle文件
    ↓
Provider从Bundle中提取资源
    ↓
返回资源句柄
```

### 5. 异步执行流程
```
OperationSystem.Update()
    ↓
移除已完成的异步操作
    ↓
添加新的异步操作
    ↓
按优先级排序
    ↓
时间切片执行
    ↓
更新操作状态
```

## 🌐 版本管理和资源更新机制

### 1. **版本比对机制 (PackageVersion Comparison)**

#### 📋 **版本比对核心实现**
```csharp
// 在UpdatePackageManifestOperation.cs中的版本比对逻辑
if (_steps == ESteps.CheckActiveManifest)
{
    // 检测当前激活的清单对象	
    if (_impl.ActiveManifest != null && _impl.ActiveManifest.PackageVersion == _packageVersion)
    {
        _steps = ESteps.Done;
        Status = EOperationStatus.Succeed;  // 版本相同，无需更新
    }
    else
    {
        _steps = ESteps.LoadPackageManifest;  // 版本不同，加载新清单
    }
}
```

#### 🔧 **版本比对特点**
- **简单字符串比较**: 直接比较 `ActiveManifest.PackageVersion == _packageVersion`
- **无复杂语义化版本**: YooAsset使用简单的字符串比较，不支持语义化版本号
- **版本一致性检查**: 确保当前激活的清单版本与请求版本一致
- **即时生效**: 版本比对成功后立即更新ActiveManifest

#### 📋 **版本请求流程**
```csharp
// RequestPackageVersionOperation.cs - 版本请求实现
public abstract class RequestPackageVersionOperation : AsyncOperationBase
{
    /// <summary>
    /// 当前最新的包裹版本
    /// </summary>
    public string PackageVersion { protected set; get; }
}

// 具体实现通过文件系统请求版本
var mainFileSystem = _impl.GetMainFileSystem();
_requestPackageVersionOp = mainFileSystem.RequestPackageVersionAsync(_appendTimeTicks, _timeout);
```

### 2. **版本不一致处理机制**

#### 🔄 **版本更新流程**
```csharp
// 1. 加载新版本的清单
if (_loadPackageManifestOp == null)
{
    var mainFileSystem = _impl.GetMainFileSystem();
    _loadPackageManifestOp = mainFileSystem.LoadPackageManifestAsync(_packageVersion, _timeout);
    _loadPackageManifestOp.StartOperation();
    AddChildOperation(_loadPackageManifestOp);
}

// 2. 更新ActiveManifest
if (_loadPackageManifestOp.Status == EOperationStatus.Succeed)
{
    _steps = ESteps.Done;
    _impl.ActiveManifest = _loadPackageManifestOp.Manifest;  // 替换为新清单
    Status = EOperationStatus.Succeed;
}
```

#### 🎯 **处理步骤详解**
1. **清单加载**: 从文件系统加载指定版本的PackageManifest
2. **清单验证**: 确保新清单加载成功且数据完整
3. **ActiveManifest替换**: 将新清单设置为当前激活清单
4. **状态更新**: 更新操作状态为成功

### 3. **文件差异检测机制**

#### 🔍 **差异检测核心逻辑**
```csharp
// PlayModeImpl.cs - GetDownloadListByAll方法
public List<BundleInfo> GetDownloadListByAll(PackageManifest manifest)
{
    List<BundleInfo> result = new List<BundleInfo>(1000);
    foreach (var packageBundle in manifest.BundleList)
    {
        var fileSystem = GetBelongFileSystem(packageBundle);
        if (fileSystem == null)
            continue;

        if (fileSystem.NeedDownload(packageBundle))  // 关键：检查是否需要下载
        {
            var bundleInfo = new BundleInfo(fileSystem, packageBundle);
            result.Add(bundleInfo);
        }
    }
    return result;
}
```

#### 🔧 **NeedDownload判断机制**
```csharp
// DefaultCacheFileSystem.cs - 缓存文件系统的下载判断
public virtual bool NeedDownload(PackageBundle bundle)
{
    if (Belong(bundle) == false)
        return false;

    return Exists(bundle) == false;  // 文件不存在就需要下载
}

public virtual bool Exists(PackageBundle bundle)
{
    return _records.ContainsKey(bundle.BundleGUID);  // 检查缓存记录
}
```

#### 📊 **差异检测原理**
- **缓存记录检查**: 通过`_records`字典检查文件是否已缓存
- **文件存在性验证**: 如果缓存中没有记录，则需要下载
- **清单驱动**: 基于新清单的BundleList进行遍历检查
- **文件系统归属**: 通过`GetBelongFileSystem`确定文件所属系统

#### 🎯 **下载器创建流程**
```csharp
// PreDownloadContentOperation.cs - 创建下载器
public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
{
    if (Status != EOperationStatus.Succeed)
    {
        return ResourceDownloaderOperation.CreateEmptyDownloader(_impl.PackageName, downloadingMaxNumber, failedTryAgain, timeout);
    }

    List<BundleInfo> downloadList = _impl.GetDownloadListByAll(_manifest);
    var operation = new ResourceDownloaderOperation(_impl.PackageName, downloadList, downloadingMaxNumber, failedTryAgain, timeout);
    return operation;
}
```

### 4. **文件存储位置和结构**

#### 📁 **存储目录结构**
```csharp
// DefaultCacheFileSystem.cs - 文件路径定义
protected string _packageRoot;           // 包根目录
protected string _tempFilesRoot;         // 临时文件目录
protected string _cacheBundleFilesRoot;  // Bundle文件目录
protected string _cacheManifestFilesRoot; // 清单文件目录
```

```csharp
// YooAssetSettingsData.cs - 缓存路径规则
internal static string GetYooDefaultCacheRoot()
{
#if UNITY_EDITOR
    return GetYooEditorCacheRoot();           // 编辑器：项目根目录
#elif UNITY_STANDALONE_WIN
    return GetYooStandaloneWinCacheRoot();    // Windows：Application.dataPath
#elif UNITY_STANDALONE_LINUX
    return GetYooStandaloneLinuxCacheRoot();  // Linux：Application.dataPath
#elif UNITY_STANDALONE_OSX
    return GetYooStandaloneMacCacheRoot();    // Mac：Application.persistentDataPath
#else
    return GetYooMobileCacheRoot();           // 移动平台：Application.persistentDataPath
#endif
}
```

#### 📂 **完整目录结构**
```
Application.persistentDataPath/YooAsset/[PackageName]/
├── BundleFiles/                    # AB资源文件目录
│   ├── [Hash前2位]/               # 按哈希前2位分目录
│   │   ├── [BundleGUID]/          # Bundle唯一标识目录
│   │   │   ├── __data             # 实际数据文件
│   │   │   └── __info             # 文件信息记录
│   │   └── ...
│   └── ...
├── ManifestFiles/                  # 清单文件目录
│   ├── [PackageName]_[Version].bytes  # 新版本清单文件
│   └── [PackageName]_[Version].hash   # 清单哈希文件
├── TempFiles/                      # 临时文件目录
│   └── [BundleGUID]               # 下载临时文件
└── [PackageVersion]                # 版本文件
```

#### 🔧 **文件路径构建逻辑**
```csharp
// 清单文件路径构建
public string GetCachePackageManifestFilePath(string packageVersion)
{
    // 路径: [PackageRoot]/ManifestFiles/[PackageName]_[Version].bytes
    string fileName = YooAssetSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
    return PathUtility.Combine(_cacheManifestFilesRoot, fileName);
}

// Bundle文件路径构建
public string GetCacheBundleFileLoadPath(PackageBundle bundle)
{
    // 路径: [PackageRoot]/BundleFiles/[Hash前2位]/[BundleGUID]/__data
    string folderName = bundle.FileHash.Substring(0, 2);
    return PathUtility.Combine(_cacheBundleFilesRoot, folderName, bundle.BundleGUID, DefaultCacheFileSystemDefine.BundleDataFileName);
}
```

#### 📋 **文件记录管理**
```csharp
// 缓存记录结构
protected readonly Dictionary<string, RecordFileElement> _records = new Dictionary<string, RecordFileElement>(10000);

// 记录文件信息
public bool RecordBundleFile(string bundleGUID, RecordFileElement element)
{
    if (_records.ContainsKey(bundleGUID))
    {
        YooLogger.Error($"{nameof(DefaultCacheFileSystem)} has element : {bundleGUID}");
        return false;
    }

    _records.Add(bundleGUID, element);
    return true;
}
```

### 5. **新旧资源加载优先级机制**

#### 🎯 **文件系统优先级设计**
```csharp
// PlayModeImpl.cs - 文件系统归属判断
public IFileSystem GetBelongFileSystem(PackageBundle packageBundle)
{
    for (int i = 0; i < FileSystems.Count; i++)  // 按顺序遍历文件系统
    {
        IFileSystem fileSystem = FileSystems[i];
        if (fileSystem.Belong(packageBundle))  // 检查是否属于该文件系统
        {
            return fileSystem;  // 返回第一个匹配的文件系统
        }
    }
    return null;
}
```

#### 🔧 **各文件系统的Belong实现**
```csharp
// DefaultCacheFileSystem.cs - 缓存文件系统（优先级最高）
public virtual bool Belong(PackageBundle bundle)
{
    return true;  // 缓存文件系统保底加载！
}

// DefaultBuildinFileSystem.cs - 内置文件系统（优先级较低）
public virtual bool Belong(PackageBundle bundle)
{
    if (DisableCatalogFile)
        return true;
    return _wrappers.ContainsKey(bundle.BundleGUID);  // 检查是否在内置目录中
}
```

#### 📊 **加载优先级策略**
1. **缓存优先**: `DefaultCacheFileSystem` 总是返回 `true`，优先加载缓存文件
2. **内置兜底**: `DefaultBuildinFileSystem` 检查文件是否在内置目录中
3. **顺序查找**: 按文件系统列表顺序查找，找到第一个匹配的就返回
4. **版本隔离**: 不同版本的文件存储在不同目录，通过ActiveManifest控制

#### 🎯 **ActiveManifest控制机制**
```csharp
// 当前激活的清单决定了使用哪个版本
public PackageManifest ActiveManifest { set; get; }

// 资源加载时使用ActiveManifest
var packageBundle = ActiveManifest.GetMainPackageBundle(assetInfo.Asset);
var fileSystem = GetBelongFileSystem(packageBundle);  // 根据ActiveManifest查找文件系统
```

#### 🔄 **资源加载流程**
```csharp
// BundleInfo.cs - 创建BundleInfo时确定文件系统
public BundleInfo(IFileSystem fileSystem, PackageBundle bundle)
{
    _fileSystem = fileSystem;
    Bundle = bundle;
    _importFilePath = null;
}

// 加载Bundle文件
public FSLoadBundleOperation LoadBundleFile()
{
    return _fileSystem.LoadBundleFile(Bundle);  // 使用确定的文件系统加载
}
```

### 6. **版本管理完整流程**

#### 🔄 **完整更新流程**
```
1. 请求服务器版本 (RequestPackageVersionAsync)
   ↓
2. 版本比对 (UpdatePackageManifestOperation)
   ↓
3. 加载新清单 (LoadPackageManifestAsync)
   ↓
4. 更新ActiveManifest
   ↓
5. 创建下载器 (CreateResourceDownloader)
   ↓
6. 差异检测 (GetDownloadListByAll)
   ↓
7. 下载文件 (DownloaderOperation)
   ↓
8. 缓存文件 (WriteCacheBundleFile)
   ↓
9. 更新记录 (RecordBundleFile)
```

#### 🎯 **关键设计优势**
- **简单高效**: 版本比对使用简单字符串比较，性能优秀
- **增量更新**: 只下载发生变化的文件，节省带宽和时间
- **版本隔离**: 不同版本文件独立存储，避免冲突
- **优先级明确**: 缓存优先策略确保最新资源优先加载
- **自动管理**: 大部分版本管理逻辑自动处理，开发者无需关心细节

#### 💡 **实际应用场景**
- **热更新**: 游戏运行时更新资源，无需重新安装
- **版本回滚**: 支持回滚到之前的版本
- **增量发布**: 只发布变化的资源，减少包体大小
- **多版本并存**: 支持多个版本资源并存，便于A/B测试

## 💡 架构优势

### 1. 模块化设计
- **职责分离**: 各系统职责清晰，相互独立
- **易于扩展**: 接口设计良好，便于添加新功能
- **易于维护**: 代码结构清晰，便于调试和修改

### 2. 异步友好
- **全异步设计**: 所有耗时操作都是异步的
- **不阻塞主线程**: 通过时间切片避免卡顿
- **协程支持**: 与Unity协程系统良好集成

### 3. 多平台支持
- **跨平台**: 支持PC、移动、WebGL等多个平台
- **运行模式**: 针对不同平台提供专门的运行模式
- **文件系统**: 抽象文件系统接口，支持不同存储方式

### 4. 性能优化
- **智能缓存**: 自动缓存管理，减少重复加载
- **并发控制**: 控制并发数量，避免资源竞争
- **内存管理**: 自动资源卸载，防止内存泄漏
- **依赖管理**: 智能处理资源依赖关系

### 5. 易于使用
- **简洁API**: 提供简单易用的接口
- **自动管理**: 大部分细节自动处理
- **错误处理**: 完善的错误处理和日志系统
- **调试支持**: 丰富的调试信息和工具

## 📝 学习心得

### 1. 架构设计的重要性
YooAsset的架构设计非常优秀，通过分层和模块化的方式，使得整个系统既复杂又易于理解和使用。

### 2. 异步编程的实践
通过OperationSystem的设计，很好地展示了如何在Unity中进行异步编程，既保证了性能又提供了良好的用户体验。

### 3. 资源管理的复杂性
资源管理涉及文件系统、网络、缓存、依赖关系等多个方面，YooAsset通过抽象和封装，很好地处理了这些复杂性。

### 4. 扩展性的考虑
通过接口设计和插件化架构，YooAsset提供了很好的扩展性，可以根据具体需求进行定制。

## 🔮 后续学习计划

1. **深入Provider系统**: 了解不同类型的Provider实现
2. **学习Bundle管理**: 深入了解AssetBundle的加载和管理
3. **研究缓存策略**: 学习缓存系统的具体实现
4. **探索自定义扩展**: 了解如何扩展YooAsset的功能
5. **性能优化实践**: 学习在实际项目中的性能优化技巧

---

*本文档将持续更新，记录更多学习成果和实践经验。* 
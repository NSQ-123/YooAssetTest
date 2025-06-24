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
using System;
using System.Threading.Tasks;

namespace GF.AstSystem
{
    /// <summary>
    /// 统一资源句柄接口
    /// </summary>
    public interface IResourceHandle : IDisposable
    {
        /// <summary>
        /// 资源对象
        /// </summary>
        UnityEngine.Object Asset { get; }
        
        /// <summary>
        /// 是否加载完成
        /// </summary>
        bool IsDone { get; }
        
        /// <summary>
        /// 是否有效
        /// </summary>
        bool IsValid { get; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        string Error { get; }
        
        /// <summary>
        /// 资源地址
        /// </summary>
        string Address { get; }
        
        /// <summary>
        /// 资源类型
        /// </summary>
        Type AssetType { get; }
        
        /// <summary>
        /// 引用计数
        /// </summary>
        int RefCount { get; }
        
        /// <summary>
        /// 异步Task
        /// </summary>
        Task Task { get; }
        
        /// <summary>
        /// 增加引用计数
        /// </summary>
        void AddRef();
        
        /// <summary>
        /// 减少引用计数
        /// </summary>
        void Release();
        
        /// <summary>
        /// 强制释放资源
        /// </summary>
        void ForceRelease();
        
        /// <summary>
        /// 等待异步完成
        /// </summary>
        Task WaitForCompletionAsync();
        
        /// <summary>
        /// 等待异步完成并返回资源
        /// </summary>
        Task<T> GetAssetAsync<T>() where T : UnityEngine.Object;
    }
} 
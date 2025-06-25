//#define UNITY_ADDRESSABLES
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using YooAsset;

namespace GF.AstSystem
{
    /// <summary>
    /// 资源引用数据
    /// </summary>
    public class AstRefData
    {
        /// <summary>
        /// 后端类型
        /// </summary>
        public ResourceBackend BackendType { get; set; }
        
        /// <summary>
        /// 包名（YooAsset使用）
        /// </summary>
        public string PackageName { get; set; }
        
        /// <summary>
        /// Addressables句柄
        /// </summary>
#if UNITY_ADDRESSABLES
        public AsyncOperationHandle HandleAA { get; set; }
#endif
        
        /// <summary>
        /// YooAsset句柄
        /// </summary>
        public AssetHandle HandleYoo { get; set; }
        
        /// <summary>
        /// 资源类型
        /// </summary>
        public Type AssetType { get; set; }
        
        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { get; private set; }
        
        /// <summary>
        /// 是否已释放
        /// </summary>
        public bool IsDisposed { get; private set; }

        public AstRefData()
        {
            RefCount = 0;
            IsDisposed = false;
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void AddRef()
        {
            if (IsDisposed) return;
            RefCount++;
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void Release()
        {
            if (IsDisposed) return;
            
            RefCount--;
            if (RefCount <= 0)
            {
                ForceRelease();
            }
        }

        /// <summary>
        /// 强制释放资源
        /// </summary>
        public void ForceRelease()
        {
            if (IsDisposed) return;
            
            IsDisposed = true;
#if UNITY_ADDRESSABLES
            if (HandleAA.IsValid())
            {
                Addressables.Release(HandleAA);
            }
#endif
            HandleYoo?.Release();
            HandleYoo = null;
            AssetType = null;
        }
    }
} 
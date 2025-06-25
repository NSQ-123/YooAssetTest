using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GF.AstSystem
{
    /// <summary>
    /// AstSystem扩展方法
    /// </summary>
    public static class AstSystemExtensions
    {
        /// <summary>
        /// 异步加载资源（简化版本）
        /// </summary>
        public static async Task<T> LoadAsset<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.None, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（场景卸载模式）
        /// </summary>
        public static async Task<T> LoadAssetForScene<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.SceneUnload, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（自动释放模式）
        /// </summary>
        public static async Task<T> LoadAssetAutoRelease<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.AutoRelease, cancellationToken);
        }

        /// <summary>
        /// 异步加载YooAsset资源（指定包名）
        /// </summary>
        public static async Task<T> LoadYooAsset<T>(this AstSystem astSystem, string address, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.None, packageName, cancellationToken);
        }

        /// <summary>
        /// 异步加载YooAsset资源（指定包名，场景卸载模式）
        /// </summary>
        public static async Task<T> LoadYooAssetForScene<T>(this AstSystem astSystem, string address, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.SceneUnload, packageName, cancellationToken);
        }

        /// <summary>
        /// 异步加载Addressables资源
        /// </summary>
        public static async Task<T> LoadAddressable<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.None, ResourceBackend.Addressables, cancellationToken);
        }

        /// <summary>
        /// 异步加载Addressables资源（场景卸载模式）
        /// </summary>
        public static async Task<T> LoadAddressableForScene<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.SceneUnload, ResourceBackend.Addressables, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载资源（简化版本）
        /// </summary>
        public static async Task<List<T>> LoadAssets<T>(this AstSystem astSystem, List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.None, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载资源（场景卸载模式）
        /// </summary>
        public static async Task<List<T>> LoadAssetsForScene<T>(this AstSystem astSystem, List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.SceneUnload, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载YooAsset资源（指定包名）
        /// </summary>
        public static async Task<List<T>> LoadYooAssets<T>(this AstSystem astSystem, List<string> addresses, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.None, packageName, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载YooAsset资源（指定包名，场景卸载模式）
        /// </summary>
        public static async Task<List<T>> LoadYooAssetsForScene<T>(this AstSystem astSystem, List<string> addresses, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.SceneUnload, packageName, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载Addressables资源
        /// </summary>
        public static async Task<List<T>> LoadAddressables<T>(this AstSystem astSystem, List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.None, ResourceBackend.Addressables, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载Addressables资源（场景卸载模式）
        /// </summary>
        public static async Task<List<T>> LoadAddressablesForScene<T>(this AstSystem astSystem, List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.SceneUnload, ResourceBackend.Addressables, cancellationToken);
        }

        /// <summary>
        /// 释放资源（简化版本）
        /// </summary>
        public static void ReleaseAsset<T>(this AstSystem astSystem, string address) where T : UnityEngine.Object
        {
            astSystem.ReleaseAsset<T>(address);
        }

        /// <summary>
        /// 释放YooAsset资源（指定包名）
        /// </summary>
        public static void ReleaseYooAsset<T>(this AstSystem astSystem, string address, string packageName) where T : UnityEngine.Object
        {
            astSystem.ReleaseAsset<T>(address, ResourceBackend.YooAsset, packageName);
        }

        /// <summary>
        /// 释放Addressables资源
        /// </summary>
        public static void ReleaseAddressable<T>(this AstSystem astSystem, string address) where T : UnityEngine.Object
        {
            astSystem.ReleaseAsset<T>(address, ResourceBackend.Addressables);
        }
    }
} 
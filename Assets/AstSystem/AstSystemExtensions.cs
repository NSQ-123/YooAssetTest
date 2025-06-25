using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        public static async UniTask<T> LoadAsset<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.None, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（场景卸载模式）
        /// </summary>
        public static async UniTask<T> LoadAssetForScene<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.SceneUnload, cancellationToken);
        }

        /// <summary>
        /// 异步加载资源（自动释放模式）
        /// </summary>
        public static async UniTask<T> LoadAssetAutoRelease<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.AutoRelease, cancellationToken);
        }

        /// <summary>
        /// 异步加载YooAsset资源（指定包名）
        /// </summary>
        public static async UniTask<T> LoadYooAsset<T>(this AstSystem astSystem, string address, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.None, packageName, cancellationToken);
        }

        /// <summary>
        /// 异步加载YooAsset资源（指定包名，场景卸载模式）
        /// </summary>
        public static async UniTask<T> LoadYooAssetForScene<T>(this AstSystem astSystem, string address, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.SceneUnload, packageName, cancellationToken);
        }

        /// <summary>
        /// 异步加载Addressables资源
        /// </summary>
        public static async UniTask<T> LoadAddressable<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.None, ResourceBackend.Addressables, cancellationToken);
        }

        /// <summary>
        /// 异步加载Addressables资源（场景卸载模式）
        /// </summary>
        public static async UniTask<T> LoadAddressableForScene<T>(this AstSystem astSystem, string address, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetAsync<T>(address, UnloadMode.SceneUnload, ResourceBackend.Addressables, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载资源（简化版本）
        /// </summary>
        public static async UniTask<List<T>> LoadAssets<T>(this AstSystem astSystem, List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.None, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载资源（场景卸载模式）
        /// </summary>
        public static async UniTask<List<T>> LoadAssetsForScene<T>(this AstSystem astSystem, List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.SceneUnload, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载YooAsset资源（指定包名）
        /// </summary>
        public static async UniTask<List<T>> LoadYooAssets<T>(this AstSystem astSystem, List<string> addresses, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.None, packageName, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载YooAsset资源（指定包名，场景卸载模式）
        /// </summary>
        public static async UniTask<List<T>> LoadYooAssetsForScene<T>(this AstSystem astSystem, List<string> addresses, string packageName, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.SceneUnload, packageName, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载Addressables资源
        /// </summary>
        public static async UniTask<List<T>> LoadAddressables<T>(this AstSystem astSystem, List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            return await astSystem.LoadAssetsAsync<T>(addresses, UnloadMode.None, ResourceBackend.Addressables, cancellationToken);
        }

        /// <summary>
        /// 批量异步加载Addressables资源（场景卸载模式）
        /// </summary>
        public static async UniTask<List<T>> LoadAddressablesForScene<T>(this AstSystem astSystem, List<string> addresses, CancellationToken cancellationToken = default) where T : UnityEngine.Object
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
        
        #region 智能Sprite加载扩展
        
        /// <summary>
        /// 智能加载Sprite（简化版本）
        /// </summary>
        public static async UniTask<Sprite> LoadSprite(this AstSystem astSystem, string spriteName, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpriteAsync(spriteName, UnloadMode.None, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载Sprite（场景卸载模式）
        /// </summary>
        public static async UniTask<Sprite> LoadSpriteForScene(this AstSystem astSystem, string spriteName, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpriteAsync(spriteName, UnloadMode.SceneUnload, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载Sprite（自动释放模式）
        /// </summary>
        public static async UniTask<Sprite> LoadSpriteAutoRelease(this AstSystem astSystem, string spriteName, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpriteAsync(spriteName, UnloadMode.AutoRelease, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载YooAsset Sprite（指定包名）
        /// </summary>
        public static async UniTask<Sprite> LoadYooSprite(this AstSystem astSystem, string spriteName, string packageName, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpriteAsync(spriteName, UnloadMode.None, packageName, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载YooAsset Sprite（指定包名，场景卸载模式）
        /// </summary>
        public static async UniTask<Sprite> LoadYooSpriteForScene(this AstSystem astSystem, string spriteName, string packageName, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpriteAsync(spriteName, UnloadMode.SceneUnload, packageName, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载Addressables Sprite
        /// </summary>
        public static async UniTask<Sprite> LoadAddressableSprite(this AstSystem astSystem, string spriteName, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpriteAsync(spriteName, UnloadMode.None, ResourceBackend.Addressables, cancellationToken);
        }
        
        /// <summary>
        /// 智能加载Addressables Sprite（场景卸载模式）
        /// </summary>
        public static async UniTask<Sprite> LoadAddressableSpriteForScene(this AstSystem astSystem, string spriteName, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpriteAsync(spriteName, UnloadMode.SceneUnload, ResourceBackend.Addressables, cancellationToken);
        }
        
        /// <summary>
        /// 批量智能加载Sprite（简化版本）
        /// </summary>
        public static async UniTask<List<Sprite>> LoadSprites(this AstSystem astSystem, List<string> spriteNames, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpritesAsync(spriteNames, UnloadMode.None, cancellationToken);
        }
        
        /// <summary>
        /// 批量智能加载Sprite（场景卸载模式）
        /// </summary>
        public static async UniTask<List<Sprite>> LoadSpritesForScene(this AstSystem astSystem, List<string> spriteNames, CancellationToken cancellationToken = default)
        {
            return await astSystem.LoadSpritesAsync(spriteNames, UnloadMode.SceneUnload, cancellationToken);
        }
        
        /// <summary>
        /// 释放Sprite（简化版本）
        /// </summary>
        public static void ReleaseSprite(this AstSystem astSystem, string spriteName)
        {
            astSystem.ReleaseSprite(spriteName);
        }
        
        /// <summary>
        /// 释放YooAsset Sprite（指定包名）
        /// </summary>
        public static void ReleaseYooSprite(this AstSystem astSystem, string spriteName, string packageName)
        {
            astSystem.ReleaseSprite(spriteName, ResourceBackend.YooAsset, packageName);
        }
        
        /// <summary>
        /// 释放Addressables Sprite
        /// </summary>
        public static void ReleaseAddressableSprite(this AstSystem astSystem, string spriteName)
        {
            astSystem.ReleaseSprite(spriteName, ResourceBackend.Addressables);
        }
        
        #endregion
    }
} 
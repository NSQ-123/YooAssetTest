using System.Threading.Tasks;
using UnityEngine;

namespace GF.AstSystem
{
    /// <summary>
    /// 便捷扩展方法
    /// </summary>
    public static class AstSystemExtensions
    {
        /// <summary>
        /// 加载Sprite
        /// </summary>
        public static async Task<IResourceHandle> LoadSpriteAsync(this AstSystem ast, string address, UnloadMode unloadMode = UnloadMode.None)
        {
            return await ast.LoadAssetAsync<Sprite>(address, unloadMode);
        }

        /// <summary>
        /// 直接加载Sprite对象
        /// </summary>
        public static async Task<Sprite> LoadSpriteDirectAsync(this AstSystem ast, string address, UnloadMode unloadMode = UnloadMode.None)
        {
            return await ast.LoadAssetDirectAsync<Sprite>(address, unloadMode);
        }

        /// <summary>
        /// 加载Texture2D
        /// </summary>
        public static async Task<IResourceHandle> LoadTextureAsync(this AstSystem ast, string address, UnloadMode unloadMode = UnloadMode.None)
        {
            return await ast.LoadAssetAsync<Texture2D>(address, unloadMode);
        }

        /// <summary>
        /// 直接加载Texture2D对象
        /// </summary>
        public static async Task<Texture2D> LoadTextureDirectAsync(this AstSystem ast, string address, UnloadMode unloadMode = UnloadMode.None)
        {
            return await ast.LoadAssetDirectAsync<Texture2D>(address, unloadMode);
        }

        /// <summary>
        /// 加载GameObject预制体
        /// </summary>
        public static async Task<IResourceHandle> LoadPrefabAsync(this AstSystem ast, string address, UnloadMode unloadMode = UnloadMode.None)
        {
            return await ast.LoadAssetAsync<GameObject>(address, unloadMode);
        }

        /// <summary>
        /// 直接加载GameObject预制体对象
        /// </summary>
        public static async Task<GameObject> LoadPrefabDirectAsync(this AstSystem ast, string address, UnloadMode unloadMode = UnloadMode.None)
        {
            return await ast.LoadAssetDirectAsync<GameObject>(address, unloadMode);
        }

        /// <summary>
        /// 加载音频片段
        /// </summary>
        public static async Task<IResourceHandle> LoadAudioClipAsync(this AstSystem ast, string address, UnloadMode unloadMode = UnloadMode.None)
        {
            return await ast.LoadAssetAsync<AudioClip>(address, unloadMode);
        }

        /// <summary>
        /// 直接加载音频片段对象
        /// </summary>
        public static async Task<AudioClip> LoadAudioClipDirectAsync(this AstSystem ast, string address, UnloadMode unloadMode = UnloadMode.None)
        {
            return await ast.LoadAssetDirectAsync<AudioClip>(address, unloadMode);
        }

        /// <summary>
        /// 加载并实例化预制体
        /// </summary>
        public static async Task<GameObject> LoadAndInstantiateAsync(this AstSystem ast, string address, Transform parent = null, UnloadMode unloadMode = UnloadMode.None)
        {
            var prefab = await ast.LoadAssetDirectAsync<GameObject>(address, unloadMode);
            if (prefab != null)
            {
                return Object.Instantiate(prefab, parent);
            }
            return null;
        }
    }
} 
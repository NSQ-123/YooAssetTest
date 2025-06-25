using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using GF.AstSystem;
using UnityEditor.U2D;

namespace GF.Editor
{
    /// <summary>
    /// 精灵图集映射生成器（仅生成图集精灵映射，散图直接加载）
    /// </summary>
    public static class SpriteAtlasMappingGenerator
    {
        private const string MAPPING_ASSET_PATH = "Assets/Resources/SpriteAtlasMapping.asset";
        private const string SPRITE_ATLAS_ROOT = "Assets/Res/BundleRes/SpriteAtlas";
        
        [MenuItem("资源管理/Generate Sprite Atlas Mapping", false, 1100)]
        public static void GenerateSpriteAtlasMapping()
        {
            var mapping = GetOrCreateMapping();
            mapping.Clear();
            
            // 仅扫描图集精灵
            ScanSpriteAtlases(mapping);
            
            // 保存映射文件
            EditorUtility.SetDirty(mapping);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"精灵图集映射生成完成！共找到 {mapping.Count} 个图集精灵");
        }
        
        /// <summary>
        /// 获取或创建映射资源
        /// </summary>
        private static SpriteAtlasMapping GetOrCreateMapping()
        {
            var mapping = AssetDatabase.LoadAssetAtPath<SpriteAtlasMapping>(MAPPING_ASSET_PATH);
            if (mapping == null)
            {
                // 确保Resources目录存在
                var resourcesDir = Path.GetDirectoryName(MAPPING_ASSET_PATH);
                if (!Directory.Exists(resourcesDir))
                {
                    Directory.CreateDirectory(resourcesDir);
                }
                
                mapping = ScriptableObject.CreateInstance<SpriteAtlasMapping>();
                AssetDatabase.CreateAsset(mapping, MAPPING_ASSET_PATH);
                Debug.Log($"创建新的精灵图集映射文件: {MAPPING_ASSET_PATH}");
            }
            return mapping;
        }
        
        /// <summary>
        /// 扫描所有图集
        /// </summary>
        private static void ScanSpriteAtlases(SpriteAtlasMapping mapping)
        {
            if (!Directory.Exists(SPRITE_ATLAS_ROOT))
            {
                Debug.LogWarning($"图集目录不存在: {SPRITE_ATLAS_ROOT}");
                return;
            }
            
            var atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { SPRITE_ATLAS_ROOT });
            Debug.Log($"找到 {atlasGuids.Length} 个图集");
            
            foreach (var guid in atlasGuids)
            {
                var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                if (atlas == null) continue;
                
                // 获取图集地址（去掉.spriteatlas扩展名）
                var atlasAddress = Path.GetFileNameWithoutExtension(atlasPath);
                
                // 获取图集中的所有精灵
                var sprites = GetSpritesFromAtlas(atlas);
                Debug.Log($"图集 {atlasAddress} 包含 {sprites.Count} 个精灵");
                
                foreach (var sprite in sprites)
                {
                    var spriteInfo = new SpriteAtlasMapping.AtlasSpriteInfo
                    {
                        spriteName = sprite.name,
                        atlasAddress = atlasAddress
                    };
                    mapping.AddAtlasSpriteInfo(spriteInfo);
                }
            }
        }
        

        
        /// <summary>
        /// 从图集中获取所有精灵
        /// </summary>
        private static List<Sprite> GetSpritesFromAtlas(SpriteAtlas atlas)
        {
            var sprites = new List<Sprite>();
            
            // 使用反射或者其他方式获取图集中的精灵
            // 这里使用一个简化的方法：通过图集的packables来获取
            var packables = atlas.GetPackables();
            foreach (var packable in packables)
            {
                if (packable is DefaultAsset folder)
                {
                    // 如果是文件夹，获取文件夹中的所有精灵
                    var folderPath = AssetDatabase.GetAssetPath(folder);
                    var spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
                    
                    foreach (var spriteGuid in spriteGuids)
                    {
                        var spritePath = AssetDatabase.GUIDToAssetPath(spriteGuid);
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        if (sprite != null)
                        {
                            sprites.Add(sprite);
                        }
                    }
                }
                else if (packable is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
            }
            
            return sprites;
        }
        
        /// <summary>
        /// 获取相对于Resources的路径
        /// </summary>
        private static string GetResourcesRelativePath(string fullPath)
        {
            // 移除Assets/前缀和文件扩展名
            var pathWithoutAssets = fullPath.Replace("Assets/", "");
            return Path.ChangeExtension(pathWithoutAssets, null);
        }
        
        [MenuItem("资源管理/Show Sprite Atlas Mapping Info", false, 1101)]
        public static void ShowSpriteAtlasMappingInfo()
        {
            var mapping = AssetDatabase.LoadAssetAtPath<SpriteAtlasMapping>(MAPPING_ASSET_PATH);
            if (mapping == null)
            {
                Debug.LogError("精灵图集映射文件不存在，请先生成映射！");
                return;
            }
            
            var allInfos = mapping.GetAllAtlasSpriteInfos();
            
            Debug.Log($"=== 精灵图集映射信息 ===");
            Debug.Log($"图集精灵总数: {mapping.Count}");
            Debug.Log($"说明: 散图无需映射，直接按名字加载");
            
            // 显示前10个精灵作为示例
            Debug.Log($"=== 前10个图集精灵示例 ===");
            for (int i = 0; i < Mathf.Min(10, allInfos.Count); i++)
            {
                var info = allInfos[i];
                Debug.Log($"{info.spriteName} -> 图集: {info.atlasAddress}");
            }
            
            // 按图集分组显示统计
            var atlasSpriteCount = allInfos.GroupBy(info => info.atlasAddress)
                                          .ToDictionary(g => g.Key, g => g.Count());
            
            Debug.Log($"=== 按图集分组统计 ===");
            foreach (var kvp in atlasSpriteCount)
            {
                Debug.Log($"图集 {kvp.Key}: {kvp.Value} 个精灵");
            }
        }
    }
} 
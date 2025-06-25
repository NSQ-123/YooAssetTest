using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.U2D;


namespace GF.Editor
{
    public static class SpriteAtlasEditorUtility
    {
        //SpriteAtlas生成的目录
        private const string SPRITE_ATLAS_OUT_PUT = "Assets/Res/BundleRes/SpriteAtlas";
        //SpriteAtlas的源文件夹
        private const string SPRITE_ATLAS_SOURCE = "Assets/Res/RawRes/UI/Altas";

        [MenuItem("资源管理/Generate All SpriteAtlases",false, 1000)]
        public static void GenerateAllSpriteAtlases()
        {
            GenerateSpriteAtlasImp(SPRITE_ATLAS_OUT_PUT, SPRITE_ATLAS_SOURCE,true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("资源管理/Update All SpriteAtlases",false, 1001)]
        public static void UpdateSpriteAtlas()
        {
            //更新图集
            UpdateSpriteAtlasImp(SPRITE_ATLAS_OUT_PUT);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        [UnityEditor.MenuItem("资源管理/Delete All SpriteAtlases", false, 1002)]
        public static void DeleteAllSpriteAtlases()
        {
            bool ConfirmDeleteAllSpriteAtlases()
            {
                return EditorUtility.DisplayDialog(
                    "Confirm Delete",
                    "Are you sure you want to delete all SpriteAtlases?",
                    "Yes",
                    "No"
                );
            }
            
            if (ConfirmDeleteAllSpriteAtlases())
            {
                //删除SPRITE_ATLAS_OUT_PUT下的所有SpriteAtlas
                DeleteAllSpriteAtlasImp(SPRITE_ATLAS_OUT_PUT);
                AssetDatabase.Refresh();
            }
        }
        
        public static void DeleteAllSpriteAtlasImp(string atlasPath)
        {
            //判断文件夹存在
            if (!Directory.Exists(atlasPath))
            {
                Debug.LogError($"SpriteAtlas output path does not exist: {atlasPath}");
                return;
            }
            var atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { atlasPath });
            foreach (var guid in atlasGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                if (atlas != null)
                {
                    AssetDatabase.DeleteAsset(path);
                    Debug.Log($"删除图集: {path}");
                }
            }
        }
        
        
        public static void UpdateSpriteAtlasImp(string atlasRoot,BuildTarget buildTarget= BuildTarget.NoTarget)
        {
            //判断文件夹存在
            if (!Directory.Exists(atlasRoot))
            {
                Debug.LogError($"SpriteAtlas output path does not exist: {atlasRoot}");
                return;
            }
            
            //获取atlasRoot下的所有SpriteAtlas
            var atlasGuids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { atlasRoot });
            using var _ = ListPool<SpriteAtlas>.Get(out var atlasList);
            foreach (var guid in atlasGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
                atlasList.Add(atlas);
            }
            //更新图集
            SpriteAtlasUtility.PackAtlases(atlasList.ToArray() , buildTarget);
        }
        
        
        public static void GenerateSpriteAtlasImp(string atlasRoot,string spriteRoot ,bool forceUpdate,List<SpriteAtlas> atlases=null,BuildTarget buildTarget= BuildTarget.NoTarget)
        {
            //判断文件夹存在
            if (!Directory.Exists(atlasRoot))
            {
                Debug.LogError($"SpriteAtlas output path does not exist: {atlasRoot}");
                return;
            }
            
            //判断文件夹存在
            if (!Directory.Exists(spriteRoot))
            {
                Debug.LogError($"SpriteAtlas source path does not exist: {spriteRoot}");
                return;
            }
            
            DirectoryInfo sourceDir = new DirectoryInfo(spriteRoot);
            DirectoryInfo[] subDirs = sourceDir.GetDirectories();

            List<SpriteAtlas> atlasList = null;
            if (atlases == null)
            {
                using var _ = ListPool<SpriteAtlas>.Get(out atlasList);
            }
            else
            {
                atlasList = atlases;
            }
            
            foreach (var subDir in subDirs)
            {
                var spritePath = GetRelativePath(subDir.FullName);
                var atlasPath = Path.Combine(atlasRoot, subDir.Name + ".spriteatlas");
                //如果不存在图集文件，就创建
                if (!File.Exists(atlasPath))
                {
                    //创建图集
                    SpriteAtlas atlas = CreateAtlas(atlasPath, spritePath);
                    atlasList.Add(atlas);
                }
                else
                {
                    //如果图集文件存在，就更新内容
                    var result = UpdateAtlasWithMissing(atlasPath, spritePath);
                    if (result.IsNeedUpdate||forceUpdate)
                    {
                        atlasList.Add(result.Atlas);
                        Debug.Log($"              更新图集: {atlasPath}              ");
                    }
                }
            }
            //图集面板上的PackPreview执行的是这个函数
            SpriteAtlasUtility.PackAtlases(atlasList.ToArray() , buildTarget);
        }
        
        
        private static (bool IsNeedUpdate,SpriteAtlas Atlas) UpdateAtlasWithMissing(string atlasPath, string spritePath)
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            var packables = atlas.GetPackables();
            var hasMissing = packables == null || packables.Length == 0 || packables.Any(a => a == null);
            if (hasMissing)
            {
                atlas.Remove(packables);
                var obj = AssetDatabase.LoadAssetAtPath(spritePath, typeof(UnityEngine.Object));
                atlas.Add(new[] { obj });
                EditorUtility.SetDirty(atlas);
            }

            return (hasMissing,atlas);
        }

        private static SpriteAtlas CreateAtlas(string atlasPath, string spritePath)
        {
            Debug.Log($"              开始创建图集: {atlasPath}              ");
            SpriteAtlas atlas = new SpriteAtlas();
            //设置打包参数
            SpriteAtlasPackingSettings packSetting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = false,//UI图集不勾选旋转
                enableTightPacking = false,//高密度填充，不希望镂空的图片中心有别的图片塞进去
                padding = 2,//?
            };
            atlas.SetPackingSettings(packSetting);

            //设置打包后Texture图集信息
            SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings()
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear,
            };
            atlas.SetTextureSettings(textureSettings);

            //设置平台图集大小压缩等信息
            TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings()
            {
                maxTextureSize = 4096,
                format = TextureImporterFormat.Automatic,
                crunchedCompression = true,
                textureCompression = TextureImporterCompression.Compressed,
                compressionQuality = 100,
            };
            atlas.SetPlatformSettings(platformSettings);
            AssetDatabase.CreateAsset(atlas, atlasPath);

            //加载文件夹对象
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(spritePath);
            //SpriteAtlasExtensions.Add(atlas, new[] { obj });
            atlas.Add(new[] { obj });
            EditorUtility.SetDirty(atlas);

            //创建spriteAtlas资源
            return atlas;
        }
        
        private static string GetRelativePath(string fullPath)
        {
            return "Assets" + fullPath.Substring(Application.dataPath.Length);
        }
    }
}
        
        
    

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace GF.AstSystem.Examples
{
    /// <summary>
    /// 智能Sprite加载系统使用示例
    /// </summary>
    public class SpriteLoaderExample : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private List<string> testSpriteNames = new List<string> { "icon_01", "icon_02", "icon_03" };
        [SerializeField] private Image testImage;
        [SerializeField] private Transform spriteContainer;
        [SerializeField] private GameObject spriteItemPrefab;
        
        private CancellationTokenSource _cancellationTokenSource;
        private List<GameObject> _createdItems = new List<GameObject>();

        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            if (testImage == null)
            {
                testImage = GetComponent<Image>();
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// 测试单个Sprite加载
        /// </summary>
        [ContextMenu("测试单个Sprite加载")]
        public async void TestLoadSingleSprite()
        {
            if (testSpriteNames.Count == 0) return;

            var astSystem = AstSystem.Instance;
            var token = _cancellationTokenSource.Token;

            try
            {
                Debug.Log($"开始加载精灵: {testSpriteNames[0]}");
                
                // 使用智能加载（场景卸载模式）
                var sprite = await astSystem.LoadSpriteForScene(testSpriteNames[0], token);
                
                if (sprite != null)
                {
                    Debug.Log($"加载成功: {sprite.name}");
                    
                    // 获取精灵信息
                    var spriteInfo = astSystem.GetSpriteInfo(testSpriteNames[0]);
                    if (spriteInfo != null)
                    {
                        Debug.Log($"精灵来源: 图集 {spriteInfo.atlasAddress}");
                    }
                    else
                    {
                        Debug.Log($"精灵来源: 散图（直接加载）");
                    }
                    
                    // 显示在UI上
                    if (testImage != null)
                    {
                        testImage.sprite = sprite;
                    }
                }
                else
                {
                    Debug.LogError("加载失败");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"加载异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试批量Sprite加载
        /// </summary>
        [ContextMenu("测试批量Sprite加载")]
        public async void TestLoadMultipleSprites()
        {
            if (testSpriteNames.Count == 0) return;

            var astSystem = AstSystem.Instance;
            var token = _cancellationTokenSource.Token;

            try
            {
                Debug.Log($"开始批量加载 {testSpriteNames.Count} 个精灵");
                
                // 批量加载（场景卸载模式）
                var sprites = await astSystem.LoadSpritesForScene(testSpriteNames, token);
                
                Debug.Log($"批量加载完成，成功加载 {sprites.Count} 个精灵");
                
                // 创建UI显示
                ClearCreatedItems();
                
                for (int i = 0; i < sprites.Count; i++)
                {
                    var sprite = sprites[i];
                    if (sprite != null)
                    {
                        CreateSpriteItem(sprite, testSpriteNames[i]);
                        
                        // 显示精灵信息
                        var spriteInfo = astSystem.GetSpriteInfo(testSpriteNames[i]);
                        if (spriteInfo != null)
                        {
                            Debug.Log($"{testSpriteNames[i]} -> 图集: {spriteInfo.atlasAddress}");
                        }
                        else
                        {
                            Debug.Log($"{testSpriteNames[i]} -> 散图（直接加载）");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"批量加载异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试不同后端加载
        /// </summary>
        [ContextMenu("测试不同后端加载")]
        public async void TestDifferentBackends()
        {
            if (testSpriteNames.Count == 0) return;

            var astSystem = AstSystem.Instance;
            var token = _cancellationTokenSource.Token;
            var spriteName = testSpriteNames[0];

            try
            {
                Debug.Log("=== 测试不同后端加载 ===");

                // 默认后端
                Debug.Log("1. 默认后端加载");
                var sprite1 = await astSystem.LoadSprite(spriteName, token);
                Debug.Log($"默认后端结果: {(sprite1 != null ? "成功" : "失败")}");

                // YooAsset后端
                Debug.Log("2. YooAsset后端加载");
                var sprite2 = await astSystem.LoadSpriteAsync(spriteName, UnloadMode.None, ResourceBackend.YooAsset, token);
                Debug.Log($"YooAsset后端结果: {(sprite2 != null ? "成功" : "失败")}");

                // Addressables后端
                Debug.Log("3. Addressables后端加载");
                var sprite3 = await astSystem.LoadAddressableSprite(spriteName, token);
                Debug.Log($"Addressables后端结果: {(sprite3 != null ? "成功" : "失败")}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"后端测试异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试释放功能
        /// </summary>
        [ContextMenu("测试释放功能")]
        public void TestReleaseSprites()
        {
            var astSystem = AstSystem.Instance;

            Debug.Log("=== 测试释放功能 ===");

            // 获取释放前的统计信息
            astSystem.GetResourceStats(out int totalBefore, out int activeBefore);
            Debug.Log($"释放前 - 总引用: {totalBefore}, 活跃引用: {activeBefore}");

            // 释放所有测试精灵
            foreach (var spriteName in testSpriteNames)
            {
                astSystem.ReleaseSprite(spriteName);
                Debug.Log($"释放精灵: {spriteName}");
            }

            // 获取释放后的统计信息
            astSystem.GetResourceStats(out int totalAfter, out int activeAfter);
            Debug.Log($"释放后 - 总引用: {totalAfter}, 活跃引用: {activeAfter}");
        }

        /// <summary>
        /// 显示精灵映射信息
        /// </summary>
        [ContextMenu("显示精灵映射信息")]
        public void ShowSpriteMappingInfo()
        {
            var astSystem = AstSystem.Instance;
            
            Debug.Log("=== 精灵映射信息 ===");
            
            foreach (var spriteName in testSpriteNames)
            {
                var spriteInfo = astSystem.GetSpriteInfo(spriteName);
                if (spriteInfo != null)
                {
                    Debug.Log($"{spriteName} -> 图集: {spriteInfo.atlasAddress}");
                }
                else
                {
                    Debug.Log($"{spriteName} -> 散图（直接加载）");
                }
            }
        }

        /// <summary>
        /// 创建精灵显示项
        /// </summary>
        private void CreateSpriteItem(Sprite sprite, string spriteName)
        {
            if (spriteContainer == null || spriteItemPrefab == null) return;

            var item = Instantiate(spriteItemPrefab, spriteContainer);
            var image = item.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = sprite;
            }

            // 设置名字
            item.name = $"Sprite_{spriteName}";
            _createdItems.Add(item);
        }

        /// <summary>
        /// 清理创建的显示项
        /// </summary>
        private void ClearCreatedItems()
        {
            foreach (var item in _createdItems)
            {
                if (item != null)
                {
                    DestroyImmediate(item);
                }
            }
            _createdItems.Clear();
        }

        /// <summary>
        /// 清理显示项
        /// </summary>
        [ContextMenu("清理显示项")]
        public void CleanupDisplayItems()
        {
            ClearCreatedItems();
        }
    }
} 
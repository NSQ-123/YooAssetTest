using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using GF.AstSystem;

namespace GF.AstSystem.Examples
{
    /// <summary>
    /// AstSystem使用示例
    /// </summary>
    public class AstSystemExample : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private string testAddress = "Sphere";
        [SerializeField] private string testPackageName = "DefaultPackage";
        [SerializeField] private List<string> testAddresses = new List<string> { "Sphere", "Cube" };

        private CancellationTokenSource _cancellationTokenSource;

        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            RunExamples();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        private async void RunExamples()
        {
            var astSystem = AstSystem.Instance;
            var token = _cancellationTokenSource.Token;

            try
            {
                // 示例1：基本加载（使用默认后端）
                Debug.Log("=== 示例1：基本加载 ===");
                var asset1 = await astSystem.LoadAsset<GameObject>(testAddress, token);
                Debug.Log($"加载成功: {asset1?.name}");

                // 示例2：场景卸载模式
                Debug.Log("=== 示例2：场景卸载模式 ===");
                var asset2 = await astSystem.LoadAssetForScene<GameObject>(testAddress, token);
                Debug.Log($"场景资源加载成功: {asset2?.name}");

                // 示例3：YooAsset包名加载
                Debug.Log("=== 示例3：YooAsset包名加载 ===");
                var asset3 = await astSystem.LoadYooAsset<GameObject>(testAddress, testPackageName, token);
                Debug.Log($"YooAsset资源加载成功: {asset3?.name}");

                // 示例4：Addressables加载
                Debug.Log("=== 示例4：Addressables加载 ===");
                var asset4 = await astSystem.LoadAddressable<GameObject>(testAddress, token);
                Debug.Log($"Addressables资源加载成功: {asset4?.name}");

                // 示例5：批量加载
                Debug.Log("=== 示例5：批量加载 ===");
                var assets = await astSystem.LoadAssets<GameObject>(testAddresses, token);
                Debug.Log($"批量加载成功，共 {assets.Count} 个资源");

                // 示例6：批量YooAsset加载
                Debug.Log("=== 示例6：批量YooAsset加载 ===");
                var yooAssets = await astSystem.LoadYooAssets<GameObject>(testAddresses, testPackageName, token);
                Debug.Log($"批量YooAsset加载成功，共 {yooAssets.Count} 个资源");

                // 示例7：使用扩展方法
                Debug.Log("=== 示例7：使用扩展方法 ===");
                var asset7 = await astSystem.LoadAsset<GameObject>(testAddress, token);
                Debug.Log($"扩展方法加载成功: {asset7?.name}");

                // 示例8：获取统计信息
                Debug.Log("=== 示例8：获取统计信息 ===");
                astSystem.GetResourceStats(out int totalRefData, out int activeRefData);
                Debug.Log($"总引用数据: {totalRefData}, 活跃引用: {activeRefData}");

                // 示例9：释放资源
                Debug.Log("=== 示例9：释放资源 ===");
                astSystem.ReleaseAsset<GameObject>(testAddress);
                Debug.Log("资源释放完成");

                // 示例10：释放包内所有资源
                Debug.Log("=== 示例10：释放包内所有资源 ===");
                astSystem.ReleasePackage(testPackageName);
                Debug.Log("包内资源释放完成");

            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("操作被取消");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 手动测试方法
        /// </summary>
        [ContextMenu("测试加载")]
        public async void TestLoad()
        {
            var astSystem = AstSystem.Instance;
            var token = _cancellationTokenSource.Token;

            try
            {
                var asset = await astSystem.LoadAsset<GameObject>(testAddress, token);
                if (asset != null)
                {
                    Instantiate(asset);
                    Debug.Log($"测试加载成功: {asset.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"测试加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试批量加载
        /// </summary>
        [ContextMenu("测试批量加载")]
        public async void TestBatchLoad()
        {
            var astSystem = AstSystem.Instance;
            var token = _cancellationTokenSource.Token;

            try
            {
                var assets = await astSystem.LoadAssets<GameObject>(testAddresses, token);
                foreach (var asset in assets)
                {
                    if (asset != null)
                    {
                        Instantiate(asset);
                        Debug.Log($"批量加载成功: {asset.name}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"批量加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 测试YooAsset加载
        /// </summary>
        [ContextMenu("测试YooAsset加载")]
        public async void TestYooAssetLoad()
        {
            var astSystem = AstSystem.Instance;
            var token = _cancellationTokenSource.Token;

            try
            {
                var asset = await astSystem.LoadYooAsset<GameObject>(testAddress, testPackageName, token);
                if (asset != null)
                {
                    Instantiate(asset);
                    Debug.Log($"YooAsset加载成功: {asset.name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"YooAsset加载失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清空所有资源
        /// </summary>
        [ContextMenu("清空所有资源")]
        public void ClearAllAssets()
        {
            var astSystem = AstSystem.Instance;
            astSystem.ClearAll();
            Debug.Log("所有资源已清空");
        }

        /// <summary>
        /// 显示统计信息
        /// </summary>
        [ContextMenu("显示统计信息")]
        public void ShowStats()
        {
            var astSystem = AstSystem.Instance;
            astSystem.GetResourceStats(out int totalRefData, out int activeRefData);
            Debug.Log($"资源统计 - 总引用数据: {totalRefData}, 活跃引用: {activeRefData}");
        }
    }
} 
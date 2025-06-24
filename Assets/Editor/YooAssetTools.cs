using System.IO;
using UnityEngine;
using UnityEditor;
using YooAsset;

namespace GF.Editor
{
    /// <summary>
    /// YooAsset沙盒目录工具
    /// </summary>
    public static class YooAssetTools
    {
        /// <summary>
        /// 打开YooAsset缓存根目录
        /// </summary>
        [MenuItem("YooAsset/Open Cache Root Directory")]
        public static void OpenCacheRootDirectory()
        {
            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
            OpenDirectory(cacheRoot, "YooAsset Cache Root");
        }

        /// <summary>
        /// 打开指定包的缓存目录
        /// </summary>
        /// <param name="packageName">包名称</param>
        [MenuItem("YooAsset/Open Package Cache Directory")]
        public static void OpenPackageCacheDirectory()
        {
            string packageName = GetPackageNameFromUser();
            if (string.IsNullOrEmpty(packageName))
                return;

            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
            string packageCachePath = PathUtility.Combine(cacheRoot, packageName);
            OpenDirectory(packageCachePath, $"Package Cache: {packageName}");
        }

        /// <summary>
        /// 打开Bundle文件目录
        /// </summary>
        /// <param name="packageName">包名称</param>
        [MenuItem("YooAsset/Open Bundle Files Directory")]
        public static void OpenBundleFilesDirectory()
        {
            string packageName = GetPackageNameFromUser();
            if (string.IsNullOrEmpty(packageName))
                return;

            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
            string bundleFilesPath = PathUtility.Combine(cacheRoot, packageName, DefaultCacheFileSystemDefine.BundleFilesFolderName);
            OpenDirectory(bundleFilesPath, $"Bundle Files: {packageName}");
        }

        /// <summary>
        /// 打开清单文件目录
        /// </summary>
        /// <param name="packageName">包名称</param>
        [MenuItem("YooAsset/Open Manifest Files Directory")]
        public static void OpenManifestFilesDirectory()
        {
            string packageName = GetPackageNameFromUser();
            if (string.IsNullOrEmpty(packageName))
                return;

            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
            string manifestFilesPath = PathUtility.Combine(cacheRoot, packageName, DefaultCacheFileSystemDefine.ManifestFilesFolderName);
            OpenDirectory(manifestFilesPath, $"Manifest Files: {packageName}");
        }

        /// <summary>
        /// 打开临时文件目录
        /// </summary>
        /// <param name="packageName">包名称</param>
        [MenuItem("YooAsset/Open Temp Files Directory")]
        public static void OpenTempFilesDirectory()
        {
            string packageName = GetPackageNameFromUser();
            if (string.IsNullOrEmpty(packageName))
                return;

            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
            string tempFilesPath = PathUtility.Combine(cacheRoot, packageName, DefaultCacheFileSystemDefine.TempFilesFolderName);
            OpenDirectory(tempFilesPath, $"Temp Files: {packageName}");
        }

        /// <summary>
        /// 打开内置资源目录
        /// </summary>
        [MenuItem("YooAsset/Open Buildin Directory")]
        public static void OpenBuildinDirectory()
        {
            string buildinRoot = YooAssetSettingsData.GetYooDefaultBuildinRoot();
            OpenDirectory(buildinRoot, "YooAsset Buildin Root");
        }

        /// <summary>
        /// 清理指定包的缓存
        /// </summary>
        [MenuItem("YooAsset/Clear Package Cache")]
        public static void ClearPackageCache()
        {
            string packageName = GetPackageNameFromUser();
            if (string.IsNullOrEmpty(packageName))
                return;

            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
            string packageCachePath = PathUtility.Combine(cacheRoot, packageName);

            if (Directory.Exists(packageCachePath))
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Clear Package Cache", 
                    $"Are you sure you want to clear the cache for package '{packageName}'?\n\nPath: {packageCachePath}", 
                    "Clear", 
                    "Cancel"
                );

                if (confirmed)
                {
                    try
                    {
                        Directory.Delete(packageCachePath, true);
                        Debug.Log($"Successfully cleared cache for package: {packageName}");
                        EditorUtility.DisplayDialog("Success", $"Cache cleared for package: {packageName}", "OK");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to clear cache for package {packageName}: {e.Message}");
                        EditorUtility.DisplayDialog("Error", $"Failed to clear cache: {e.Message}", "OK");
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Info", $"No cache found for package: {packageName}", "OK");
            }
        }

        /// <summary>
        /// 清理所有缓存
        /// </summary>
        [MenuItem("YooAsset/Clear All Cache")]
        public static void ClearAllCache()
        {
            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();

            if (Directory.Exists(cacheRoot))
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Clear All Cache", 
                    $"Are you sure you want to clear ALL YooAsset cache?\n\nPath: {cacheRoot}", 
                    "Clear All", 
                    "Cancel"
                );

                if (confirmed)
                {
                    try
                    {
                        Directory.Delete(cacheRoot, true);
                        Debug.Log("Successfully cleared all YooAsset cache");
                        EditorUtility.DisplayDialog("Success", "All cache cleared successfully", "OK");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to clear all cache: {e.Message}");
                        EditorUtility.DisplayDialog("Error", $"Failed to clear all cache: {e.Message}", "OK");
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "No cache found", "OK");
            }
        }

        /// <summary>
        /// 显示缓存信息
        /// </summary>
        [MenuItem("YooAsset/Show Cache Info")]
        public static void ShowCacheInfo()
        {
            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
            string buildinRoot = YooAssetSettingsData.GetYooDefaultBuildinRoot();

            string info = $"YooAsset Cache Information:\n\n" +
                         $"Cache Root: {cacheRoot}\n" +
                         $"Buildin Root: {buildinRoot}\n\n";

            if (Directory.Exists(cacheRoot))
            {
                var packages = Directory.GetDirectories(cacheRoot);
                info += $"Found {packages.Length} package(s):\n";
                foreach (var package in packages)
                {
                    string packageName = Path.GetFileName(package);
                    long size = GetDirectorySize(package);
                    info += $"- {packageName}: {FormatFileSize(size)}\n";
                }
            }
            else
            {
                info += "No cache directory found.";
            }

            EditorUtility.DisplayDialog("Cache Info", info, "OK");
        }

        #region 私有方法

        /// <summary>
        /// 打开目录
        /// </summary>
        private static void OpenDirectory(string path, string title)
        {
            if (Directory.Exists(path))
            {
                EditorUtility.RevealInFinder(path);
                Debug.Log($"Opened directory: {path}");
            }
            else
            {
                bool create = EditorUtility.DisplayDialog(
                    title, 
                    $"Directory does not exist:\n{path}\n\nWould you like to create it?", 
                    "Create", 
                    "Cancel"
                );

                if (create)
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                        EditorUtility.RevealInFinder(path);
                        Debug.Log($"Created and opened directory: {path}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to create directory: {e.Message}");
                        EditorUtility.DisplayDialog("Error", $"Failed to create directory: {e.Message}", "OK");
                    }
                }
            }
        }

        /// <summary>
        /// 从用户获取包名称
        /// </summary>
        private static string GetPackageNameFromUser()
        {
            string cacheRoot = YooAssetSettingsData.GetYooDefaultCacheRoot();
            if (!Directory.Exists(cacheRoot))
            {
                EditorUtility.DisplayDialog("Info", "No cache directory found", "OK");
                return null;
            }

            var packages = Directory.GetDirectories(cacheRoot);
            if (packages.Length == 0)
            {
                EditorUtility.DisplayDialog("Info", "No packages found in cache", "OK");
                return null;
            }

            if (packages.Length == 1)
            {
                return Path.GetFileName(packages[0]);
            }

            // 如果有多个包，让用户选择
            string[] packageNames = new string[packages.Length];
            for (int i = 0; i < packages.Length; i++)
            {
                packageNames[i] = Path.GetFileName(packages[i]);
            }

            int selectedIndex = EditorUtility.DisplayDialogComplex(
                "Select Package",
                "Multiple packages found. Please select one:",
                packageNames[0],
                packages.Length > 2 ? packageNames[1] : "Cancel",
                packages.Length > 2 ? packageNames[2] : ""
            ) == 0 ? 0 : 1;

            if (selectedIndex < packageNames.Length)
            {
                return packageNames[selectedIndex];
            }

            return null;
        }

        /// <summary>
        /// 获取目录大小
        /// </summary>
        private static long GetDirectorySize(string path)
        {
            long size = 0;
            try
            {
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    size += fileInfo.Length;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to calculate directory size for {path}: {e.Message}");
            }
            return size;
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        #endregion
    }
}
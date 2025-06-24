using System.IO;
using UnityEditor;
using UnityEngine;

namespace GF.Editor
{
    public static class FileEditorUtility
    {
        /// <summary>
        /// 打开目录
        /// </summary>
        public static void OpenDirectory(string path, string title)
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
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GF.Editor
{
    public static class Tools
    {
        /// <summary>
        /// 打开持久化数据目录
        /// </summary>
        [MenuItem("Tools/Open Persistent Data Directory")]
        public static void OpenPersistentDataDirectory()
        {
            string persistentDataPath = Application.persistentDataPath;
            FileEditorUtility.OpenDirectory(persistentDataPath, "Persistent Data Directory");
        }

        /// <summary>
        /// 打开临时缓存目录
        /// </summary>
        [MenuItem("Tools/Open Temporary Cache Directory")]
        public static void OpenTemporaryCacheDirectory()
        {
            string tempCachePath = Application.temporaryCachePath;
            FileEditorUtility.OpenDirectory(tempCachePath, "Temporary Cache Directory");
        }

        /// <summary>
        /// 打开数据目录
        /// </summary>
        [MenuItem("Tools/Open Data Directory")]
        public static void OpenDataDirectory()
        {
            string dataPath = Application.dataPath;
            FileEditorUtility.OpenDirectory(dataPath, "Data Directory");
        }

        /// <summary>
        /// 打开流资源目录
        /// </summary>
        [MenuItem("Tools/Open Streaming Assets Directory")]
        public static void OpenStreamingAssetsDirectory()
        {
            string streamingAssetsPath = Application.streamingAssetsPath;
            FileEditorUtility.OpenDirectory(streamingAssetsPath, "Streaming Assets Directory");
        }

    }
}
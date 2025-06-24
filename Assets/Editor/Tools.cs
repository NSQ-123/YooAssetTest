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
    }
}
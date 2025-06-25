using System;
using System.Collections.Generic;
using UnityEngine;

namespace GF.AstSystem
{
    /// <summary>
    /// 精灵图集映射数据（仅存储图集精灵，散图直接加载）
    /// </summary>
    [CreateAssetMenu(fileName = "SpriteAtlasMapping", menuName = "AstSystem/SpriteAtlasMapping")]
    public class SpriteAtlasMapping : ScriptableObject
    {
        [System.Serializable]
        public class AtlasSpriteInfo
        {
            /// <summary>
            /// 精灵名字
            /// </summary>
            public string spriteName;
            
            /// <summary>
            /// 图集地址
            /// </summary>
            public string atlasAddress;
        }
        
        [Header("图集精灵映射数据")]
        [SerializeField] private List<AtlasSpriteInfo> _atlasSpriteInfos = new List<AtlasSpriteInfo>();
        
        private Dictionary<string, AtlasSpriteInfo> _spriteToAtlasDict;
        
        /// <summary>
        /// 初始化字典（运行时调用）
        /// </summary>
        public void Initialize()
        {
            if (_spriteToAtlasDict == null)
            {
                _spriteToAtlasDict = new Dictionary<string, AtlasSpriteInfo>();
                foreach (var info in _atlasSpriteInfos)
                {
                    if (!string.IsNullOrEmpty(info.spriteName))
                    {
                        _spriteToAtlasDict[info.spriteName] = info;
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查精灵是否在图集中
        /// </summary>
        public bool IsInAtlas(string spriteName)
        {
            Initialize();
            return _spriteToAtlasDict.ContainsKey(spriteName);
        }
        
        /// <summary>
        /// 获取精灵对应的图集地址
        /// </summary>
        public string GetAtlasAddress(string spriteName)
        {
            Initialize();
            return _spriteToAtlasDict.TryGetValue(spriteName, out var info) ? info.atlasAddress : null;
        }
        
        /// <summary>
        /// 根据精灵名字获取图集信息
        /// </summary>
        public AtlasSpriteInfo GetAtlasSpriteInfo(string spriteName)
        {
            Initialize();
            return _spriteToAtlasDict.TryGetValue(spriteName, out var info) ? info : null;
        }
        
        /// <summary>
        /// 添加图集精灵信息（编辑器使用）
        /// </summary>
        public void AddAtlasSpriteInfo(AtlasSpriteInfo info)
        {
            if (_atlasSpriteInfos == null)
                _atlasSpriteInfos = new List<AtlasSpriteInfo>();
                
            _atlasSpriteInfos.Add(info);
            _spriteToAtlasDict = null; // 清空缓存，下次使用时重新构建
        }
        
        /// <summary>
        /// 清空所有数据（编辑器使用）
        /// </summary>
        public void Clear()
        {
            _atlasSpriteInfos.Clear();
            _spriteToAtlasDict = null;
        }
        
        /// <summary>
        /// 获取所有图集精灵信息（编辑器使用）
        /// </summary>
        public List<AtlasSpriteInfo> GetAllAtlasSpriteInfos()
        {
            return _atlasSpriteInfos;
        }
        
        /// <summary>
        /// 图集精灵总数
        /// </summary>
        public int Count => _atlasSpriteInfos?.Count ?? 0;
    }
} 

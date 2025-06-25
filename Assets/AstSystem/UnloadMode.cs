namespace GF.AstSystem
{
    /// <summary>
    /// 资源卸载模式
    /// </summary>
    public enum UnloadMode
    {
        /// <summary>
        /// 不自动卸载
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 场景卸载时释放
        /// </summary>
        SceneUnload,
        
        /// <summary>
        /// 自动释放
        /// </summary>
        AutoRelease
    }
} 
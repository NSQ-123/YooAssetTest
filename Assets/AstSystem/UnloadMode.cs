namespace GF.AstSystem
{
    /// <summary>
    /// 资源卸载模式
    /// </summary>
    public enum UnloadMode
    {
        None,           // 手动卸载
        AutoRelease,    // 自动卸载（引用计数为0时）
        SceneUnload     // 过场景卸载
    }
} 
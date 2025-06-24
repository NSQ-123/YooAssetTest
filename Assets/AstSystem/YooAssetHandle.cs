using System;
using System.Threading.Tasks;
using YooAsset;

namespace GF.AstSystem
{
    /// <summary>
    /// YooAsset资源句柄包装
    /// </summary>
    internal class YooAssetHandle : IResourceHandle
    {
        private AssetHandle _handle;
        private int _refCount = 1;
        private UnloadMode _unloadMode;
        private bool _disposed = false;
        private TaskCompletionSource<bool> _taskCompletionSource;

        public UnityEngine.Object Asset => _handle?.AssetObject;
        public bool IsDone => _handle?.IsDone ?? false;
        public float Progress => _handle?.Progress ?? 0f;
        public bool IsValid => _handle?.IsValid ?? false;
        public string Error => _handle?.LastError ?? string.Empty;
        public string Address { get; private set; }
        public UnloadMode UnloadMode => _unloadMode;
        public int RefCount => _refCount;
        public Task Task => _taskCompletionSource?.Task ?? Task.CompletedTask;

        public event Action<IResourceHandle> Completed;

        public YooAssetHandle(AssetHandle handle, string address, UnloadMode unloadMode)
        {
            _handle = handle;
            Address = address;
            _unloadMode = unloadMode;
            _taskCompletionSource = new TaskCompletionSource<bool>();
            
            if (_handle != null)
            {
                _handle.Completed += OnCompleted;
                if (_handle.IsDone)
                {
                    OnCompleted(_handle);
                }
            }
            else
            {
                _taskCompletionSource.SetResult(false);
            }
        }

        private void OnCompleted(AssetHandle handle)
        {
            Completed?.Invoke(this);
            
            if (handle.Status == EOperationStatus.Succeed)
            {
                _taskCompletionSource?.SetResult(true);
            }
            else
            {
                _taskCompletionSource?.SetException(new Exception(handle.LastError));
            }
        }

        public void AddRef()
        {
            if (_disposed) return;
            _refCount++;
        }

        public void Release()
        {
            if (_disposed) return;
            
            _refCount--;
            if (_refCount <= 0)
            {
                ForceRelease();
            }
        }

        private void ForceRelease()
        {
            if (_disposed) return;
            
            _disposed = true;
            _handle?.Release();
            _handle = null;
            Completed = null;
            _taskCompletionSource = null;
        }

        public async Task WaitForCompletionAsync()
        {
            if (_disposed) return;
            await Task;
        }

        public async Task<T> GetAssetAsync<T>() where T : UnityEngine.Object
        {
            await WaitForCompletionAsync();
            return Asset as T;
        }

        public void Dispose()
        {
            ForceRelease();
        }
    }
} 
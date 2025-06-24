using System;
using System.Threading.Tasks;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif

namespace GF.AstSystem
{
#if UNITY_ADDRESSABLES
    /// <summary>
    /// Addressables资源句柄包装
    /// </summary>
    internal class AddressablesHandle : IResourceHandle
    {
        private AsyncOperationHandle<UnityEngine.Object> _handle;
        private int _refCount = 1;
        private UnloadMode _unloadMode;
        private bool _disposed = false;
        private Task _task;

        public UnityEngine.Object Asset => _handle.IsValid() ? _handle.Result : null;
        public bool IsDone => _handle.IsDone;
        public float Progress => _handle.PercentComplete;
        public bool IsValid => _handle.IsValid();
        public string Error => _handle.OperationException?.Message ?? string.Empty;
        public string Address { get; private set; }
        public UnloadMode UnloadMode => _unloadMode;
        public int RefCount => _refCount;
        public Task Task => _task ?? Task.CompletedTask;

        public event Action<IResourceHandle> Completed;

        public AddressablesHandle(AsyncOperationHandle<UnityEngine.Object> handle, string address, UnloadMode unloadMode)
        {
            _handle = handle;
            Address = address;
            _unloadMode = unloadMode;
            _task = ConvertToTask();
            
            _handle.Completed += OnCompleted;
        }

        private async Task ConvertToTask()
        {
            while (!_handle.IsDone)
            {
                await Task.Yield();
            }
            
            if (_handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception(_handle.OperationException?.Message ?? "Addressables load failed");
            }
        }

        private void OnCompleted(AsyncOperationHandle<UnityEngine.Object> handle)
        {
            Completed?.Invoke(this);
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
            if (_handle.IsValid())
            {
                Addressables.Release(_handle);
            }
            Completed = null;
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
#endif
} 
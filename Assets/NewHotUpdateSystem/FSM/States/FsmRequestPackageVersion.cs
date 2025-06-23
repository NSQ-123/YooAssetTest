using System.Collections;
using UnityEngine;
using YooAsset;

namespace GF.HotUpdateSystem.New
{
    /// <summary>
    /// 请求包版本状态
    /// </summary>
    internal class FsmRequestPackageVersion : BaseStateNode
    {
        protected override HotUpdateStage GetStage() => HotUpdateStage.RequestPackageVersion;
        
        protected override void OnEnterState()
        {
            StartCoroutine(UpdatePackageVersion());
        }
        
        private IEnumerator UpdatePackageVersion()
        {
            var packageName = _machine.GetBlackboardValue<string>("PackageName");
            var package = YooAssets.GetPackage(packageName);
            var operation = package.RequestPackageVersionAsync();
            yield return operation;

            if (operation.Status != EOperationStatus.Succeed)
            {
                _operation?.SendError($"请求包版本失败: {operation.Error}", HotUpdateStage.RequestPackageVersion);
                yield break;
            }
            else
            {
                Debug.Log($"请求包版本: {operation.PackageVersion}");
                _machine.SetBlackboardValue("PackageVersion", operation.PackageVersion);
                _machine.ChangeState<FsmUpdatePackageManifest>();
            }
        }
    }
} 
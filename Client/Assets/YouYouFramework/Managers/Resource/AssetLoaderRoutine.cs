using System;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源加载器
    /// </summary>
    public class AssetLoaderRoutine
    {
        /// <summary>
        /// 资源加载请求
        /// </summary>
        private AssetBundleRequest m_CurAssetBundleRequest;

        /// <summary>
        /// 加载中回调
        /// </summary>
        public Action<float> OnLoadAssetUpdate;

        /// <summary>
        /// 加载完毕回调
        /// </summary>
        public Action<UnityEngine.Object> OnLoadAssetComplete;

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetName">资源名称</param>
        /// <param name="assetBundle">所属的资源包</param>
        public void LoadAsset(string assetName, AssetBundle assetBundle) {
            m_CurAssetBundleRequest = assetBundle.LoadAssetAsync(assetName);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset() {
            m_CurAssetBundleRequest = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void OnUpdate() {
            UpdateAssetBundleRequest();
        }

        /// <summary>
        /// 更新加载资源请求
        /// </summary>
        private void UpdateAssetBundleRequest() {
            if(m_CurAssetBundleRequest != null) {
                if (m_CurAssetBundleRequest.isDone) {
                    UnityEngine.Object obj = m_CurAssetBundleRequest.asset;
                    if(obj != null) {
                        GameEntry.Log("资源=>{0} 加载完毕", LogCategory.Resource, obj.name);
                        Reset();

                        OnLoadAssetComplete?.Invoke(obj);
                    } else {
                        GameEntry.LogError("资源=>{0} 加载失败", obj.name);
                        Reset();
                        OnLoadAssetComplete?.Invoke(obj);
                    }
                } else {
                    OnLoadAssetUpdate?.Invoke(m_CurAssetBundleRequest.progress);
                }
            }
        }

    }
}

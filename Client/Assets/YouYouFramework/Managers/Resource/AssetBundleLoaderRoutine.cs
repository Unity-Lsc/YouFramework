using System;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源加载器
    /// </summary>
    public class AssetBundleLoaderRoutine
    {
        /// <summary>
        /// 当前的资源包信息
        /// </summary>
        private AssetBundleInfoEntity m_CurAssetBundleInfoEntity;

        /// <summary>
        /// 资源包的创建请求
        /// </summary>
        private AssetBundleCreateRequest m_CurAssetBundleCreateRequest;

        /// <summary>
        /// 资源包加载中回调
        /// </summary>
        public Action<float> OnAssetBundleCreateUpdate;

        /// <summary>
        /// 加载资源包完毕回调
        /// </summary>
        public Action<AssetBundle> OnLoadAssetBundleComplete;

        /// <summary>
        /// 加载资源包
        /// </summary>
        /// <param name="abPath">资源包路径</param>
        public void LoadAssetBundle(string abPath) {
            m_CurAssetBundleInfoEntity = GameEntry.Resource.ResourceManager.GetAssetBundleInfoEntity(abPath);

            byte[] buffer = GameEntry.Resource.ResourceManager.LocalAssetsManager.GetFileBuffer(abPath);
            if(buffer == null) {
                //可写区没有 就从只读区里获取
                GameEntry.Resource.ResourceManager.StreamingAssetsManager.ReadAssetBundle(abPath, (byte[] buff) => {
                    if (buff == null) {
                        //TODO: 只读区也没,就只能从CDN上下载
                    } else {
                        LoadAssetBundleAsync(buff);
                    }
                });
            } else {
                LoadAssetBundleAsync(buffer);
            }

        }

        /// <summary>
        /// 异步加载资源包
        /// </summary>
        private void LoadAssetBundleAsync(byte[] buffer) {
            if (m_CurAssetBundleInfoEntity.IsEncrypt) {
                buffer = SecurityUtil.Xor(buffer);
            }

            m_CurAssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(buffer);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset() {
            m_CurAssetBundleCreateRequest = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void OnUpdate() {
            UpdateAssetBundleCreateRequest();
        }

        /// <summary>
        /// 更新资源包请求
        /// </summary>
        private void UpdateAssetBundleCreateRequest() {
            if(m_CurAssetBundleCreateRequest != null) {
                if (m_CurAssetBundleCreateRequest.isDone) {
                    var assetBundle = m_CurAssetBundleCreateRequest.assetBundle;
                    if(assetBundle != null) {
                        GameEntry.Log("资源包=>{0}加载完毕", LogCategory.Resource, m_CurAssetBundleInfoEntity.AssetBundleName);
                        Reset();
                        OnLoadAssetBundleComplete?.Invoke(assetBundle);
                    } else {
                        GameEntry.LogError("资源包=>{0}加载失败", m_CurAssetBundleInfoEntity.AssetBundleName);
                        Reset();
                        OnLoadAssetBundleComplete?.Invoke(null);
                    }
                } else {
                    //加载进度
                    OnAssetBundleCreateUpdate?.Invoke(m_CurAssetBundleCreateRequest.progress);
                }
            }
        }

    }
}

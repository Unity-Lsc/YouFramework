using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源组件
    /// </summary>
    public class ResourceComponent : YouYouBaseComponent
    {
        /// <summary>
        /// 本地文件路径
        /// </summary>
        public string LocalFilePath;

        /// <summary>
        /// 资源管理器
        /// </summary>
        private ResourceManager m_ResourceManager;

        protected override void OnAwake() {
            base.OnAwake();
            m_ResourceManager = new ResourceManager();

#if DISABLE_ASSETBUNDLE
            LocalFilePath = Application.dataPath;
#else   
            LocalFilePath = Application.persistentDataPath;
#endif
        }

        /// <summary>
        /// 初始化只读区资源包信息
        /// </summary>
        public void InitStreamingAssetsBundleInfo() {
            m_ResourceManager.InitStreamingAssetsBundleInfo();
        }

        public override void Shutdown() {
            m_ResourceManager.Dispose();
        }
    }
}

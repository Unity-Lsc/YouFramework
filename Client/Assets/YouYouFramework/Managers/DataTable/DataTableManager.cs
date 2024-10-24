using System;
using System.Threading.Tasks;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 数据表 管理器
    /// </summary>
    public class DataTableManager : ManagerBase
    {
        #region 表格属性

        public DTSysAudioDBModel DTSysAudioDBModel { get; private set; }
        public DTSysCodeDBModel DTSysCodeDBModel { get; private set; }
        public DTSysCommonEventIdDBModel DTSysCommonEventIdDBModel { get; private set; }
        public DTSysConfigDBModel DTSysConfigDBModel { get; private set; }
        public DTSysEffectDBModel DTSysEffectDBModel { get; private set; }
        public LocalizationDBModel LocalizationDBModel { get; private set; }
        public DTSysPrefabDBModel DTSysPrefabDBModel { get; private set; }
        public DTSysSceneDBModel DTSysSceneDBModel { get; private set; }
        public DTSysSceneDetailDBModel DTSysSceneDetailDBModel { get; private set; }
        public DTSysStorySoundDBModel DTSysStorySoundDBModel { get; private set; }
        public DTSysUIFormDBModel DTSysUIFormDBModel { get; private set; }

        public DTEquipDBModel DTEquipDBModel { get; private set; }
        public DTShopDBModel DTShopDBModel { get; private set; }
        public DTTaskDBModel DTTaskDBModel { get; private set; }

        #endregion

        /// <summary>
        /// 总共要加载的表格数量
        /// </summary>
        public int TotalLoadCount = 0;

        /// <summary>
        /// 当前已经加载完毕的表格数量
        /// </summary>
        public int CurLoadCount = 0;

        public DataTableManager() {
            InitDBModel();
        }

        /// <summary>
        /// 初始化DBModel(数据管理)
        /// </summary>
        private void InitDBModel() {
            //每个表都要实例化一下
            DTSysAudioDBModel = new DTSysAudioDBModel();
            DTSysCodeDBModel = new DTSysCodeDBModel();
            DTSysCommonEventIdDBModel = new DTSysCommonEventIdDBModel();
            DTSysConfigDBModel = new DTSysConfigDBModel();
            DTSysEffectDBModel = new DTSysEffectDBModel();
            LocalizationDBModel = new LocalizationDBModel();
            DTSysPrefabDBModel = new DTSysPrefabDBModel();
            DTSysSceneDBModel = new DTSysSceneDBModel();
            DTSysSceneDetailDBModel = new DTSysSceneDetailDBModel();
            DTSysStorySoundDBModel = new DTSysStorySoundDBModel();
            DTSysUIFormDBModel = new DTSysUIFormDBModel();

            DTEquipDBModel = new DTEquipDBModel();
            DTShopDBModel = new DTShopDBModel();
            DTTaskDBModel = new DTTaskDBModel();
        }

        private AssetBundle m_DataTableBundle;

        /// <summary>
        /// 异步加载表格
        /// </summary>
        public void LoadDataTableAsync() {
#if DISABLE_ASSETBUNDLE
            Task.Factory.StartNew(LoadDataTable);
#else
            GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle("download/datatable.assetbundle",
                onUpdate : (float progress) => {
                    GameEntry.Log("加载进度:" + progress);
                },
                onComplete: (AssetBundle bundle) => {
                    m_DataTableBundle = bundle;
                    LoadDataTable();
                    GameEntry.Log("LoadDataTableAsync拿到了Bundle");
                }
                );
#endif
        }

        /// <summary>
        /// 获取表格的字节数组
        /// </summary>
        /// <param name="tableName">表格名字</param>
        /// <param name="onComplete">完成回调</param>
        public void GetDataTableBuffer(string tableName, Action<byte[]> onComplete) {
#if DISABLE_ASSETBUNDLE
            byte[] buffer = IOUtil.GetFileBuffer(string.Format("{0}/Download/DataTable/{1}.bytes", GameEntry.Resource.LocalFilePath, tableName));
            onComplete?.Invoke(buffer);
#else
            GameEntry.Resource.ResourceLoaderManager.LoadAsset(GameEntry.Resource.GetLasrPathName(tableName), m_DataTableBundle,
                onComplete: (UnityEngine.Object obj) => {
                    TextAsset asset = obj as TextAsset;
                    onComplete?.Invoke(asset.bytes);
                });
#endif
        }

        /// <summary>
        /// 加载表格
        /// </summary>
        private void LoadDataTable() {
            //每个表格都要LoadData
            DTSysAudioDBModel.LoadData();
            DTSysCodeDBModel.LoadData();
            DTSysCommonEventIdDBModel.LoadData();
            DTSysConfigDBModel.LoadData();
            DTSysEffectDBModel.LoadData();
            LocalizationDBModel.LoadData();
            DTSysPrefabDBModel.LoadData();
            DTSysSceneDBModel.LoadData();
            DTSysSceneDetailDBModel.LoadData();
            DTSysStorySoundDBModel.LoadData();
            DTSysUIFormDBModel.LoadData();

            DTEquipDBModel.LoadData();
            DTShopDBModel.LoadData();
            DTTaskDBModel.LoadData();
        }

        public void Clear() {
            //每个表都要Clear一下
            DTSysAudioDBModel.Clear();
            DTSysCodeDBModel.Clear();
            DTSysCommonEventIdDBModel.Clear();
            DTSysConfigDBModel.Clear();
            DTSysEffectDBModel.Clear();
            LocalizationDBModel.Clear();
            DTSysPrefabDBModel.Clear();
            DTSysSceneDBModel.Clear();
            DTSysSceneDetailDBModel.Clear();
            DTSysStorySoundDBModel.Clear();
            DTSysUIFormDBModel.Clear();

            DTEquipDBModel.Clear();
            DTShopDBModel.Clear();
            DTTaskDBModel.Clear();
        }

    }
}

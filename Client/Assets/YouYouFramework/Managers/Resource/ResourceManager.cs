
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源 管理器
    /// </summary>
    public class ResourceManager : ManagerBase, IDisposable
    {
        /// <summary>
        /// 只读区(StreamingAssets)资源管理器
        /// </summary>
        public StreamingAssetsManager StreamingAssetsManager { get; private set; }

        /// <summary>
        /// 可写区(persisdentDataPath)资源管理器
        /// </summary>
        public LocalAssetsManager LocalAssetsManager { get; private set; }

        public ResourceManager() {
            StreamingAssetsManager = new StreamingAssetsManager();
            LocalAssetsManager = new LocalAssetsManager();
        }

        /// <summary>
        /// 根据字节数组返回资源包版本信息
        /// </summary>
        /// <param name="buffer">字节数组</param>
        /// <param name="version">版本号</param>
        /// <returns>返回资源包版本信息</returns>
        public static Dictionary<string, AssetBundleInfoEntity> GetAssetBundleVersionList(byte[] buffer, ref string version) {
            buffer = ZlibHelper.DeCompressBytes(buffer);
            Dictionary<string, AssetBundleInfoEntity> dict = new Dictionary<string, AssetBundleInfoEntity>();
            MMO_MemoryStream ms = new MMO_MemoryStream(buffer);
            int len = ms.ReadInt();
            for (int i = 0; i < len; i++) {
                if(i == 0) {
                    version = ms.ReadUTF8String().Trim();
                } else {
                    var entity = new AssetBundleInfoEntity();
                    entity.AssetBundleName = ms.ReadUTF8String();
                    entity.MD5 = ms.ReadUTF8String();
                    entity.Size = ms.ReadInt();
                    entity.IsFirstData = ms.ReadByte() == 1;
                    entity.IsEncrypt = ms.ReadByte() == 1;
                    dict[entity.AssetBundleName] = entity;
                }
            }
            return dict;
        }

        #region 只读区StreamingAssets

        /// <summary>
        /// 只读区资源版本号
        /// </summary>
        private string m_StreamingAssetsVersion;

        /// <summary>
        /// 只读区资源包信息
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_StreamingAssetsVersionDict;

        /// <summary>
        /// 只读区是否存在资源包信息
        /// </summary>
        private bool m_IsExistStreamingAssetsVersionFile = false;

        /// <summary>
        /// 初始化只读区资源包信息
        /// </summary>
        public void InitStreamingAssetsBundleInfo() {
            GameEntry.Log("初始化只读区资源包信息", LogCategory.Resource);

            ReadStreamingAssetsBundle(ConstDefine.VersionFileName, (byte[] buffer) => {
                if(buffer != null) {
                    m_IsExistStreamingAssetsVersionFile = true;
                    m_StreamingAssetsVersionDict = GetAssetBundleVersionList(buffer, ref m_StreamingAssetsVersion);

                    //GameEntry.Log("只读区资源版本号=>" + m_StreamingAssetsVersion, LogCategory.Resource);
                    //foreach (var item in m_StreamingAssetsVersionDict) {
                    //    GameEntry.Log(item.Value.AssetBundleName, LogCategory.Resource);
                    //}
                }
                InitCDNAssetBundleInfo();

            });
        }

        /// <summary>
        /// 读取只读区的资源包
        /// </summary>
        internal void ReadStreamingAssetsBundle(string fileUrl, Action<byte[]> onComplete) {
            StreamingAssetsManager.ReadAssetBundle(fileUrl, onComplete);
        }

        #endregion

        #region CDN

        /// <summary>
        /// CDN资源版本号
        /// </summary>
        private string m_CDNVersion;
        /// <summary>
        /// CDN资源包信息
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_CDNVersionDict;

        /// <summary>
        /// 初始化CDN资源包信息
        /// </summary>
        private void InitCDNAssetBundleInfo() {
#if TEST_MODEL
            GameEntry.Data.SystemDataManager.CurChannelConfig.SourceUrl = "http://192.168.1.104:8081";
            GameEntry.Data.SystemDataManager.CurChannelConfig.SourceVersion = "1.0.1";
#endif

            string url = string.Format("{0}VersionFile.bytes", GameEntry.Data.SystemDataManager.CurChannelConfig.RealSourceUrl);
            GameEntry.Log(url, LogCategory.Resource);
            GameEntry.Http.SendData(url, OnInitCDNAssetBundleInfo, isGetData: true);
        }

        /// <summary>
        /// 初始化CDN资源包信息回调
        /// </summary>
        /// <param name="args"></param>
        private void OnInitCDNAssetBundleInfo(HttpCallBackArgs args) {
            if (!args.HasError) {
                m_CDNVersionDict = GetAssetBundleVersionList(args.Data, ref m_CDNVersion);
                GameEntry.Log("初始化CDN资源包信息完成", LogCategory.Resource);

                CheckVersionFileExistInLocal();

            } else {
                GameEntry.Log(args.Value, LogCategory.Resource);
            }
        }

        #endregion

        #region 可写区

        /// <summary>
        /// 可写区资源版本号
        /// </summary>
        private string m_LocalAssetsVersion;

        /// <summary>
        /// 可写区资源包信息
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_LocalAssetsVersionDict;

        /// <summary>
        /// 检查可写区版本文件是否存在
        /// </summary>
        private void CheckVersionFileExistInLocal() {
            GameEntry.Log("检查可写区版本文件是否存在", LogCategory.Resource);

            if (LocalAssetsManager.GetVersionFileIsExist()) {
                //可写区存在资源版本文件,加载可写区的资源包信息(一般是第二次登陆)
                InitLocalAssetsBundleInfo();
            } else {
                //可写区不存在资源版本文件,先判断只读区版本文件是否存在(一般是第一次登陆)
                if (m_IsExistStreamingAssetsVersionFile) {
                    //只读区存在版本文件,将其初始化到可写区
                    InitVersionFileFromStreamingAssetsToLocal();
                }
                CheckVersionChange();
            }

        }

        /// <summary>
        /// 将只读区的版本文件初始化到可写区
        /// </summary>
        private void InitVersionFileFromStreamingAssetsToLocal() {
            GameEntry.Log("将只读区的版本文件初始化到可写区", LogCategory.Resource);

            m_LocalAssetsVersionDict = new Dictionary<string, AssetBundleInfoEntity>();

            var enumerator = m_StreamingAssetsVersionDict.GetEnumerator();
            while (enumerator.MoveNext()) {
                var entity = enumerator.Current.Value;
                m_LocalAssetsVersionDict[enumerator.Current.Key] = new AssetBundleInfoEntity() {
                    AssetBundleName = entity.AssetBundleName,
                    MD5 = entity.MD5,
                    Size = entity.Size,
                    IsFirstData = entity.IsFirstData,
                    IsEncrypt = entity.IsEncrypt
                };
            }

            //保存版本文件
            LocalAssetsManager.SaveVersionFile(m_LocalAssetsVersionDict);
            //保存版本号
            m_LocalAssetsVersion = m_StreamingAssetsVersion;
            LocalAssetsManager.SetResourceVersion(m_LocalAssetsVersion);

        }

        /// <summary>
        /// 初始化可写区资源信息
        /// </summary>
        private void InitLocalAssetsBundleInfo() {
            GameEntry.Log("初始化可写区资源信息", LogCategory.Resource);
            m_LocalAssetsVersionDict = LocalAssetsManager.GetAssetBundleVersionList(ref m_LocalAssetsVersion);
            CheckVersionChange();
        }

        /// <summary>
        /// 获取资源包的信息(这个方法要一定能返回资源信息)
        /// </summary>
        /// <param name="abPath">资源包路径</param>
        public AssetBundleInfoEntity GetAssetBundleInfoEntity(string abPath) {
            m_CDNVersionDict.TryGetValue(abPath, out var entity);
            return entity;
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        private void CheckVersionChange() {
            GameEntry.Log("开始进行检查更新...", LogCategory.Resource);

            if (LocalAssetsManager.GetVersionFileIsExist()) {
                //判断只读区资源版本号和CDN资源版本号是否一致
                if (m_StreamingAssetsVersion.Equals(m_CDNVersion)) {
                    GameEntry.Log("只读区资源版本号和CDN资源版本号一致", LogCategory.Resource);
                    //进入预加载流程
                    GameEntry.Procedure.ChangeState(ProcedureState.Preload);
                } else {
                    GameEntry.Log("只读区资源版本号和CDN资源版本号不一致", LogCategory.Resource);

                    //TODO: 不一致,开始检查更新

                    //然后再进入预加载流程

                }
            } else {
                //TODO: 开始下载初始资源
            }

        }

        #endregion


        public void Dispose() {
            m_StreamingAssetsVersionDict?.Clear();
            m_CDNVersionDict?.Clear();
            m_LocalAssetsVersionDict?.Clear();
        }
    }
}

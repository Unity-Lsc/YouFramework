
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
        /// StreamingAssets资源管理器
        /// </summary>
        public StreamingAssetsManager StreamingAssetsManager { get; private set; }

        public ResourceManager() {
            StreamingAssetsManager = new StreamingAssetsManager();
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
        /// 初始化只读区资源包信息
        /// </summary>
        public void InitStreamingAssetsBundleInfo() {
            ReadStreamingAssetsBundle("VersionFile.bytes", (byte[] buffer) => {
                m_StreamingAssetsVersionDict = GetAssetBundleVersionList(buffer, ref m_StreamingAssetsVersion);

                //Debug.Log("只读区资源版本号=>" + m_StreamingAssetsVersion);

                //foreach (var item in m_StreamingAssetsVersionDict) {
                //    Debug.Log(item.Value.AssetBundleName);
                //}

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
                GameEntry.Log("OnInitCDNAssetBundleInfo", LogCategory.Resource);
            } else {
                GameEntry.Log(args.Value, LogCategory.Resource);
            }
        }

        #endregion


        public void Dispose() {
            m_StreamingAssetsVersionDict.Clear();
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 下载 管理器
    /// </summary>
    public class DownloadManager : ManagerBase
    {
        /// <summary>
        /// 超时时间
        /// </summary>
        public const int DownloadTimeout = 5;

        /// <summary>
        /// 资源下载地址（以后要改成从服务器上读取）
        /// </summary>
        public const string DownloadBaseURL = "http://192.168.0.114:8081";

        /// <summary>
        /// 下载器的数量
        /// </summary>
        public const int DownloadRoutineNum = 5;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        public const string DownloadURL = DownloadBaseURL + "Windows/";
#elif UNITY_ANDROID
        public const string DownloadURL = DownloadBaseURL + "Android/";
#elif UNITY_IOS || UNITY_IPHONE
        public const string DownloadURL = DownloadBaseURL + "iOS/";
#endif

        /// <summary>
        /// 本地资源路径
        /// </summary>
        public string LocalFilePath = Application.persistentDataPath + "/";

        /// <summary>
        /// 需要下载的数据列表
        /// </summary>
        private List<DownloadDataEntity> mNeedDownloadDataList;
        /// <summary>
        /// 本地的数据列表
        /// </summary>
        private List<DownloadDataEntity> mLocalDataList;

        public DownloadManager() {
            mNeedDownloadDataList = new List<DownloadDataEntity>();
            mLocalDataList = new List<DownloadDataEntity>();
        }

        /// <summary>
        /// 检查版本文件
        /// </summary>
        public void CheckVersion() {
            //string verPath = DownloadURL + "VersionFile.txt";
        }

    }
}

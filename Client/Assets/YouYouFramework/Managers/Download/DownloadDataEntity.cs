using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 下载数据实体
    /// </summary>
    public class DownloadDataEntity
    {
        /// <summary>
        /// 资源的名称
        /// </summary>
        public string FullName;
        /// <summary>
        /// MD5
        /// </summary>
        public string MD5;
        /// <summary>
        /// 资源的大小
        /// </summary>
        public int Size;
        /// <summary>
        /// 是否是初始数据
        /// </summary>
        public bool IsFirstData;

    }
}

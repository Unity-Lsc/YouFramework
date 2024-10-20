using System.Collections.Generic;

namespace YouYou
{
    /// <summary>
    /// AssetBundle实体
    /// </summary>
    public class AssetBundleEntity
    {

        /// <summary>
        /// 用于打包时候选定 唯一的Key
        /// </summary>
        public string Key;

        /// <summary>
        /// 名字
        /// </summary>
        public string Name;

        /// <summary>
        /// 标记
        /// </summary>
        public string Tag;

        /// <summary>
        /// 是否打包成一个资源包
        /// </summary>
        public bool Overall;

        /// <summary>
        /// 是否是初始资源
        /// </summary>
        public bool IsFirstData;

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool IsEncrypt;

        private List<string> mPathList = new List<string>();
        /// <summary>
        /// 路径集合
        /// </summary>
        public List<string> PathList { get { return mPathList; } }

    }
}

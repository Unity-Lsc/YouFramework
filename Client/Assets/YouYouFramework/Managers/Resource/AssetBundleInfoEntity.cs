
namespace YouYou
{
    /// <summary>
    /// AssetBundle信息实体
    /// </summary>
    public class AssetBundleInfoEntity
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string AssetBundleName;

        /// <summary>
        /// MD5
        /// </summary>
        public string MD5;

        /// <summary>
        /// 文件大小(单位:K)
        /// </summary>
        public int Size;

        /// <summary>
        /// 是否是初始资源
        /// </summary>
        public bool IsFirstData;

        /// <summary>
        /// 文件是否加密
        /// </summary>
        public bool IsEncrypt;
    }
}

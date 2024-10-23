using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 可写区(persisdentDataPath)管理器
    /// </summary>
    public class LocalAssetsManager
    {
        /// <summary>
        /// 本地 版本文件路径
        /// </summary>
        public string LocalVersionFilePath {
            get {
                return string.Format("{0}/{1}", Application.persistentDataPath, ConstDefine.VersionFileName);
            }
        }

        /// <summary>
        /// 获取 可写区的版本文件 是否存在
        /// </summary>
        public bool GetVersionFileIsExist() {
            return File.Exists(LocalVersionFilePath);
        }

        /// <summary>
        /// 保存版本文件
        /// </summary>
        public void SaveVersionFile(Dictionary<string, AssetBundleInfoEntity> dict) {
            string json = JsonMapper.ToJson(dict);
            IOUtil.CreateTextFile(LocalVersionFilePath, json);
        }

        /// <summary>
        /// 保存资源版本号
        /// </summary>
        public void SetResourceVersion(string version) {
            PlayerPrefs.SetString(ConstDefine.ResourceVersion, version);
        }

        /// <summary>
        /// 加载可写区资源包信息
        /// </summary>
        public Dictionary<string, AssetBundleInfoEntity> GetAssetBundleVersionList(ref string version) {
            version = PlayerPrefs.GetString(ConstDefine.ResourceVersion);
            string json = IOUtil.GetFileText(LocalVersionFilePath);
            return JsonMapper.ToObject<Dictionary<string, AssetBundleInfoEntity>>(json);
        }

        /// <summary>
        /// 获取本地文件的字节数组
        /// </summary>
        public byte[] GetFileBuffer(string path) {
            return IOUtil.GetFileBuffer(string.Format("{0}/{1}", Application.persistentDataPath, path));
        }

    }
}

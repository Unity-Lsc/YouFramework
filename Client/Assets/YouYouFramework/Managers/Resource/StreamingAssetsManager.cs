using System;
using System.Collections;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// StreamingAssets管理器
    /// </summary>
    public class StreamingAssetsManager
    {
        /// <summary>
        /// StreamingAssets资源路径
        /// </summary>
        private string m_StreamingAssetsPath;

        public StreamingAssetsManager() {
            m_StreamingAssetsPath = "file:///" + Application.streamingAssetsPath;
#if UNITY_ANDROID && !UNITY_EDITOR
            m_StreamingAssetsPath = Application.streamingAssetsPath;
#endif
        }

        /// <summary>
        /// 读取StreamingAssets下的资源
        /// </summary>
        /// <param name="url">资源路径</param>
        /// <param name="onComplete">读取成功的回调</param>
        private IEnumerator ReadStreamingAsset(string url, Action<byte[]> onComplete) {
            using (WWW www = new WWW(url)) {
                yield return www;
                if(www.error == null) {
                    onComplete?.Invoke(www.bytes);
                } else {
                    Debug.LogError(www.error);
                }
            }
        }

        /// <summary>
        /// 读取只读区的资源包
        /// </summary>
        /// <param name="fileUrl">资源路径</param>
        /// <param name="onComplete">读取成功的回调</param>
        public void ReadAssetBundle(string fileUrl, Action<byte[]> onComplete) {
            GameEntry.Resource.StartCoroutine(ReadStreamingAsset(string.Format("{0}/AssetBundles/{1}", m_StreamingAssetsPath, fileUrl), onComplete));
        }

    }
}

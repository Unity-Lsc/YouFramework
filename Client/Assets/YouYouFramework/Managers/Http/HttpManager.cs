using System.Collections.Generic;
using UnityEngine;

namespace YouYou 
{
    /// <summary>
    /// Http 管理器
    /// </summary>
    public class HttpManager : ManagerBase
    {

        /// <summary>
        /// 发送Http数据
        /// </summary>
        public void SendData(string url, HttpSendDataCallBack callback, bool isPost = false, bool isGetData = false, Dictionary<string, object> dict = null) {
            GameEntry.Log("从池中获取Http访问器", LogCategory.Proto);
            HttpRoutine http = GameEntry.Pool.DequeueClassObject<HttpRoutine>();
            http.SendData(url, callback, isPost, isGetData, dict);
        }

    }

}

using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace YouYou
{

    /// <summary>
    /// Http发送数据的回调
    /// </summary>
    public delegate void HttpSendDataCallBack(HttpCallBackArgs args);

    /// <summary>
    /// Http访问器
    /// </summary>
    public class HttpRoutine
    {
        /// <summary>
        /// Http请求的回调
        /// </summary>
        private HttpSendDataCallBack mCallBack;
        /// <summary>
        /// Http请求的回调数据
        /// </summary>
        private HttpCallBackArgs mCallBackArgs;
        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy { get; private set; }

        /// <summary>
        /// 获取的是否为Data数据
        /// </summary>
        private bool m_IsGetData = false;

        public HttpRoutine() {
            mCallBackArgs = new HttpCallBackArgs();
        }

        /// <summary>
        /// 发送Http数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <param name="isPost"></param>
        /// <param name="isGetData">是否获取字节数据</param>
        /// <param name="dict"></param>
        public void SendData(string url, HttpSendDataCallBack callback, bool isPost = false, bool isGetData = false, Dictionary<string, object> dict = null) {
            if (IsBusy) return;

            IsBusy = true;
            mCallBack = callback;
            m_IsGetData = isGetData;

            if(!isPost) {
                GetUrl(url);
            } else {
                //Http加密
                if(dict != null) {
                    //客户端标识符
                    dict["deviceIdentifier"] = DeviceUtil.DeviceIdentifier;
                    //设备型号
                    dict["deviceModel"] = DeviceUtil.DeviceModel;

                    long t = GameEntry.Data.SystemDataManager.CurServerTime;
                    //签名
                    dict["sign"] = EncryptUtil.Md5(string.Format("{0}:{1}", t, DeviceUtil.DeviceIdentifier));

                    //时间戳
                    dict["t"] = t;
                }
                //PostUrl(url, dict == null ? "" : JsonUtility.ToJson(dict));//JsonUtility无法解析字典
                string json = string.Empty;
                if(dict != null) {
                    json = JsonMapper.ToJson(dict);
                    GameEntry.Pool.EnqueueClassObject(dict);
                }
                PostUrl(url, dict == null ? "" : json);
            }

        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        private void GetUrl(string url) {
            UnityWebRequest data = UnityWebRequest.Get(url);
            GameEntry.Http.StartCoroutine(Request(data));
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        private void PostUrl(string url, string json) {
            //定义一个表单
            WWWForm form = new WWWForm();

            //给表单添加值
            form.AddField("", json);

            UnityWebRequest data = UnityWebRequest.Post(url, form);
            GameEntry.Http.StartCoroutine(Request(data));
        }

        /// <summary>
        /// 请求服务器
        /// </summary>
        private IEnumerator Request(UnityWebRequest data) {
            //yield return data;//这是WWW的写法

            yield return data.SendWebRequest();

            IsBusy = false;
            if (data.isNetworkError || data.isHttpError) {
                if (mCallBack != null) {
                    mCallBackArgs.HasError = true;
                    mCallBackArgs.Value = data.error;
                    mCallBack(mCallBackArgs);
                }
            } else {
                //没有错误
                if (mCallBack != null) {
                    mCallBackArgs.HasError = false;
                    mCallBackArgs.Value = data.downloadHandler.text;
                    if (!m_IsGetData) {
                        GameEntry.Log("<color=#00eaff>接收消息:</color><color=#00ff9c>" + data.url + "</color>", LogCategory.Proto);
                        GameEntry.Log("<color=#c5e1dc>==>>" + JsonUtility.ToJson(mCallBackArgs) + "</color>", LogCategory.Proto);
                    }
                    mCallBackArgs.Data = data.downloadHandler.data;
                    mCallBack(mCallBackArgs);
                }
            }
            data.Dispose();
            data = null;

            GameEntry.Log("把Http访问器回池", LogCategory.Proto);
            GameEntry.Pool.EnqueueClassObject(this);

        }


    }
}

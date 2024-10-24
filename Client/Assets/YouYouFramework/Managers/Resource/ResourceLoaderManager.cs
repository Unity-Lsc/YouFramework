using System;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源加载管理器
    /// </summary>
    public class ResourceLoaderManager : ManagerBase, IDisposable
    {
        /// <summary>
        /// 资源信息字典
        /// </summary>
        private Dictionary<AssetCategory, Dictionary<string, AssetEntity>> m_AssetInfoDict;

        /// <summary>
        /// 资源包加载器列表
        /// </summary>
        private LinkedList<AssetBundleLoaderRoutine> m_AssetBundleLoaderList;

        /// <summary>
        /// 资源加载器列表
        /// </summary>
        private LinkedList<AssetLoaderRoutine> m_AssetLoaderList;



        public ResourceLoaderManager() {
            m_AssetInfoDict = new Dictionary<AssetCategory, Dictionary<string, AssetEntity>>();
            //确保游戏刚开始运行的时候,分类字典已经初始化好了
            var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
            while (enumerator.MoveNext()) {
                var category = (AssetCategory)enumerator.Current;
                m_AssetInfoDict[category] = new Dictionary<string, AssetEntity>();
            }

            m_AssetBundleLoaderList = new LinkedList<AssetBundleLoaderRoutine>();
            m_AssetLoaderList = new LinkedList<AssetLoaderRoutine>();

        }

        #region 初始化资源信息

        /// <summary>
        /// 初始化资源信息
        /// </summary>
        public void InitAssetInfo() {
            byte[] buffer = GameEntry.Resource.ResourceManager.LocalAssetsManager.GetFileBuffer(ConstDefine.AssetInfoName);
            if(buffer == null) {
                //如果可写区没有,那么就从只读区获取
                GameEntry.Resource.ResourceManager.StreamingAssetsManager.ReadAssetBundle(ConstDefine.AssetInfoName, (byte[] buff) => {
                    if (buff == null) {
                        //如果可读取也没有,那么就从CDN进行下载
                        string url = string.Format("{0}{1}", GameEntry.Data.SystemDataManager.CurChannelConfig.RealSourceUrl, ConstDefine.AssetInfoName);
                        GameEntry.Http.SendData(url, OnLoadAssetInfoFromCDN, isGetData: true);
                    } else {
                        InitAssetInfo(buff);
                    }
                });
            } else {
                InitAssetInfo(buffer);
            }
        }

        /// <summary>
        /// 从CDN加载资源信息
        /// </summary>
        private void OnLoadAssetInfoFromCDN(HttpCallBackArgs args) {
            if (!args.HasError) {
                InitAssetInfo(args.Data);
            } else {
                GameEntry.LogError(args.Value);
            }
        }

        /// <summary>
        /// 初始化资源信息
        /// </summary>
        private void InitAssetInfo(byte[] buffer) {
            buffer = ZlibHelper.DeCompressBytes(buffer);
            MMO_MemoryStream ms = new MMO_MemoryStream(buffer);
            int len = ms.ReadInt();
            for (int i = 0; i < len; i++) {
                AssetEntity entity = new AssetEntity();
                entity.Category = (AssetCategory)ms.ReadByte();
                entity.AssetFullName = ms.ReadUTF8String();
                entity.AssetBundleName = ms.ReadUTF8String();

                //GameEntry.Log("entity.Category = " + entity.Category, LogCategory.Resource);
                //GameEntry.Log("entity.AssetFullName = " + entity.AssetFullName, LogCategory.Resource);

                int depLen = ms.ReadInt();
                if (depLen > 0) {
                    entity.DependsAssetList = new List<AssetDependsEntity>(depLen);
                    for (int j = 0; j < depLen; j++) {
                        AssetDependsEntity dependsEntity = new AssetDependsEntity();
                        dependsEntity.Category = (AssetCategory)ms.ReadByte();
                        dependsEntity.AssetFullName = ms.ReadUTF8String();
                        entity.DependsAssetList.Add(dependsEntity);
                    }
                }
                m_AssetInfoDict[entity.Category][entity.AssetFullName] = entity;
            }
        }

        #endregion

        /// <summary>
        /// 加载AssetBundle资源包
        /// </summary>
        /// <param name="abPath">ab包路径</param>
        /// <param name="onUpdate">加载中回调(进度)</param>
        /// <param name="onComplete">加载完毕回调</param>
        public void LoadAssetBundle(string abPath, Action<float> onUpdate = null, Action<AssetBundle> onComplete = null) {
            var routine = GameEntry.Pool.DequeueClassObject<AssetBundleLoaderRoutine>();
            if(routine == null) {
                routine = new AssetBundleLoaderRoutine();
            }
            GameEntry.Log("资源加载器取池", LogCategory.Resource);
            //加入链表开始循环
            m_AssetBundleLoaderList.AddLast(routine);

            routine.LoadAssetBundle(abPath);
            routine.OnAssetBundleCreateUpdate = (float progress) => {
                onUpdate?.Invoke(progress);
            };
            routine.OnLoadAssetBundleComplete = (AssetBundle assetBundle) => {
                onComplete?.Invoke(assetBundle);

                //结束循环 回池
                m_AssetBundleLoaderList.Remove(routine);
                GameEntry.Pool.EnqueueClassObject(routine);
                GameEntry.Log("资源加载器回池", LogCategory.Resource);
            };

        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetName">资源名字</param>
        /// <param name="assetBundle">所属的资源包</param>
        /// <param name="onUpdate">加载中回调</param>
        /// <param name="onComplete">加载完毕回调</param>
        public void LoadAsset(string assetName, AssetBundle assetBundle, Action<float> onUpdate = null, Action<UnityEngine.Object> onComplete = null) {
            var routine = GameEntry.Pool.DequeueClassObject<AssetLoaderRoutine>();
            if (routine == null) routine = new AssetLoaderRoutine();

            //加入列表开始循环
            m_AssetLoaderList.AddLast(routine);

            routine.LoadAsset(assetName, assetBundle);
            routine.OnLoadAssetUpdate = (float progress) => {
                onUpdate?.Invoke(progress);
            };
            routine.OnLoadAssetComplete = (UnityEngine.Object obj) => {
                onComplete?.Invoke(obj);

                //结束循环 回池
                m_AssetLoaderList.Remove(routine);
                GameEntry.Pool.EnqueueClassObject(routine);
            };


        }

        /// <summary>
        /// 更新
        /// </summary>
        public void OnUpdate() {
            for (var cur = m_AssetBundleLoaderList.First; cur != null; cur = cur.Next) {
                cur.Value.OnUpdate();
            }

            for (var cur = m_AssetLoaderList.First; cur != null; cur = cur.Next) {
                cur.Value.OnUpdate();
            }

        }

        public void Dispose() {
            m_AssetInfoDict.Clear();

            m_AssetBundleLoaderList.Clear();
            m_AssetLoaderList.Clear();
        }
    }
}

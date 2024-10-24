using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源实体(AssetBundle和Asset实体)
    /// </summary>
    public class ResourceEntity
    {
        /// <summary>
        /// 资源名称
        /// </summary>
        public string ResourceName;

        /// <summary>
        /// 资源分类(用于Asset)
        /// </summary>
        public AssetCategory Category;

        /// <summary>
        /// 是否是AssetBundle
        /// </summary>
        public bool IsAssetBundle;

        /// <summary>
        /// 关联目标
        /// </summary>
        public object Target;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferenceCount { get; private set; }

        /// <summary>
        /// 上次使用时间
        /// </summary>
        private float m_LastUseTime;

        /// <summary>
        /// 对象取池
        /// </summary>
        public void Spawn() {
            m_LastUseTime = Time.time;
            if (!IsAssetBundle) {
                ReferenceCount++;
            }
        }

        /// <summary>
        /// 对象回池
        /// </summary>
        public void Unspawn() {
            m_LastUseTime = Time.time;
            ReferenceCount--;
            if(ReferenceCount < 0) {
                ReferenceCount = 0;
            }
        }

        /// <summary>
        /// 对象是否可以释放
        /// </summary>
        public bool GetCanRelease() {
            if(ReferenceCount == 0 && Time.time - m_LastUseTime > GameEntry.Pool.ReleaseResourceInterval) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release() {
            ResourceName = null;
            if (IsAssetBundle) {
                AssetBundle bundle = Target as AssetBundle;
                bundle.Unload(false);
                GameEntry.Log("卸载了资源包");
            }
            Target = null;
            GameEntry.Pool.EnqueueClassObject(this);//把资源实体回池
        }

    }
}

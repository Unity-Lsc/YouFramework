using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源池
    /// </summary>
    public class ResourcePool
    {

#if UNITY_EDITOR
        /// <summary>
        /// 在检视面板显示的信息
        /// </summary>
        public Dictionary<string, int> InspectorDict = new Dictionary<string, int>();
#endif
        /// <summary>
        /// 资源池名称
        /// </summary>
        public string PoolName { get; private set; }

        /// <summary>
        /// 资源池链表
        /// </summary>
        private LinkedList<ResourceEntity> m_ResourceEntityList;

        public ResourcePool(string poolName) {
            PoolName = poolName;
            m_ResourceEntityList = new LinkedList<ResourceEntity>();
        }

        /// <summary>
        /// 注册到资源池
        /// </summary>
        public void Register(ResourceEntity entity) {
            entity.Spawn();
#if UNITY_EDITOR
            InspectorDict[entity.ResourceName] = entity.ReferenceCount;
#endif
            m_ResourceEntityList.AddLast(entity);
        }

        /// <summary>
        /// 资源取池
        /// </summary>
        /// <param name="resName">资源名</param>
        public ResourceEntity Spawn(string resName) {
            LinkedListNode<ResourceEntity> curNode = m_ResourceEntityList.First;
            while (curNode != null) {
                var entity = curNode.Value;
                if(entity.ResourceName.Equals(resName, StringComparison.CurrentCultureIgnoreCase)) {
                    entity.Spawn();
#if UNITY_EDITOR
                    if (InspectorDict.ContainsKey(entity.ResourceName)) {
                        InspectorDict[entity.ResourceName] = entity.ReferenceCount;
                    }
#endif
                    return entity;
                }
                curNode = curNode.Next;
            }
            return null;
        }

        /// <summary>
        /// 资源回池
        /// </summary>
        /// <param name="resName">资源名称</param>
        public void Unspawn(string resName) {
            var curNode = m_ResourceEntityList.First;
            while (curNode != null) {
                var entity = curNode.Value;
                if(entity.ResourceName.Equals(resName, StringComparison.CurrentCultureIgnoreCase)) {
                    entity.Unspawn();
#if UNITY_EDITOR
                    if (InspectorDict.ContainsKey(entity.ResourceName)) {
                        InspectorDict[entity.ResourceName] = entity.ReferenceCount;
                    }
#endif
                }
                curNode = curNode.Next;
            }
        }

        /// <summary>
        /// 释放资源池中可释放资源
        /// </summary>
        public void Release() {
            var curNode = m_ResourceEntityList.First;
            while (curNode != null) {
                var entity = curNode.Value;
                var nextNode = curNode.Next;
                if (entity.GetCanRelease()) {
#if UNITY_EDITOR
                    if (InspectorDict.ContainsKey(entity.ResourceName)) {
                        InspectorDict.Remove(entity.ResourceName);
                    }
#endif
                    m_ResourceEntityList.Remove(entity);
                    entity.Release();
                }
                curNode = nextNode;
            }
        }

    }
}

using System;

namespace YouYou
{
    /// <summary>
    /// 对象池 管理器
    /// </summary>
    public class PoolManager : ManagerBase, IDisposable
    {
        /// <summary>
        /// 类对象池
        /// </summary>
        public ClassObjectPool ClassObjectPool { private set; get; }

        /// <summary>
        /// 游戏物体对象池
        /// </summary>
        public GameObjectPool GameObjectPool { private set; get; }

        /// <summary>
        /// 资源包池
        /// </summary>
        public ResourcePool AssetBundlePool { private set; get; }

        public PoolManager() {
            ClassObjectPool = new ClassObjectPool();
            GameObjectPool = new GameObjectPool();

            AssetBundlePool = new ResourcePool("AssetBundlePool");
        }

        /// <summary>
        /// 释放类对象池
        /// </summary>
        public void ReleaseClassObjectPool() {
            ClassObjectPool.Release();
        }

        /// <summary>
        /// 释放AssetBundle对象池
        /// </summary>
        public void ReleaseAssetBundlePool() {
            AssetBundlePool.Release();
        }

        public void Dispose() {
            ClassObjectPool.Dispose();
            GameObjectPool.Dispose();
        }
    }
}

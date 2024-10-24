using UnityEditor;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 对象池在监视器的面板
    /// </summary>
    [CustomEditor(typeof(PoolComponent), true)]
    public class PoolComponentInspector : Editor
    {
        /// <summary>
        /// 类对象池 释放间隔 的属性
        /// </summary>
        private SerializedProperty mReleaseClassObjectInterval = null;

        /// <summary>
        /// 游戏物体对象池分组 的属性
        /// </summary>
        private SerializedProperty mGameObjectPoolEntityGroup = null;

        /// <summary>
        /// 资源包对象池释放间隔的属性
        /// </summary>
        private SerializedProperty mReleaseResourceInterval = null;

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();//显示父节点(PoolComponent)中的属性
            //更新序列化对象的表示形式
            serializedObject.Update();

            var component = base.target as PoolComponent;

            #region 类对象池
            //绘制滑动条
            int clearInterval = (int)EditorGUILayout.Slider("清空类对象池的时间间隔", mReleaseClassObjectInterval.intValue, 10, 1800);
            if (clearInterval != mReleaseClassObjectInterval.intValue) {
                component.ReleaseClassObjectInterval = clearInterval;
            } else {
                mReleaseClassObjectInterval.intValue = clearInterval;
            }
            
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("类名");
            GUILayout.Label("池中数量", GUILayout.Width(55));
            GUILayout.Label("常驻数量", GUILayout.Width(55));
            GUILayout.EndHorizontal();

            if (component != null && component.PoolManager != null) {
                foreach (var item in component.PoolManager.ClassObjectPool.InspectorDict) {
                    GUILayout.BeginHorizontal("box");

                    GUILayout.Label(item.Key.Name);
                    GUILayout.Label(item.Value.ToString(), GUILayout.Width(55));

                    //显示类对象池常驻数量
                    int key = item.Key.GetHashCode();
                    byte residentCount = 0;
                    component.PoolManager.ClassObjectPool.ClassObjectResidentDict.TryGetValue(key, out residentCount);
                    GUILayout.Label(residentCount.ToString(), GUILayout.Width(55));

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            #endregion

            #region 变量对象池
            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("变量");
            GUILayout.Label("数量", GUILayout.Width(55));
            GUILayout.EndHorizontal();

            if (component != null) {
                foreach (var item in component.VarObjectInspectorDict) {
                    GUILayout.BeginHorizontal("box");

                    GUILayout.Label(item.Key.Name);
                    GUILayout.Label(item.Value.ToString(), GUILayout.Width(55));

                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            #endregion

            #region 游戏物体对象池
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(mGameObjectPoolEntityGroup, true);
            #endregion

            GUILayout.Space(10);

            #region 资源包对象池

            //绘制滑动条 释放资源包对象池间隔
            int releaseAssetBundleInterval = (int)EditorGUILayout.Slider("释放资源包对象池的时间间隔", mReleaseResourceInterval.intValue, 10, 1800);
            if(releaseAssetBundleInterval != mReleaseResourceInterval.intValue) {
                component.ReleaseResourceInterval = releaseAssetBundleInterval;
            } else {
                mReleaseResourceInterval.intValue = releaseAssetBundleInterval;
            }

            GUILayout.Space(10);
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("资源包");
            GUILayout.Label("数量", GUILayout.Width(50));
            GUILayout.EndHorizontal();

            if(component != null && component.PoolManager != null) {
                foreach (var item in component.PoolManager.AssetBundlePool.InspectorDict) {
                    GUILayout.BeginHorizontal("box");
                    GUILayout.Label(item.Key);
                    GUILayout.Label(item.Value.ToString(), GUILayout.Width(50));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            #endregion

            serializedObject.ApplyModifiedProperties();
            //Unity面板重绘
            Repaint();

        }

        private void OnEnable() {
            //建立属性关系
            mReleaseClassObjectInterval = serializedObject.FindProperty("ReleaseClassObjectInterval");
            mGameObjectPoolEntityGroup = serializedObject.FindProperty("m_GameObjectPoolEntityGroup");

            mReleaseResourceInterval = serializedObject.FindProperty("ReleaseResourceInterval");
            //应用 属性信息的修改
            serializedObject.ApplyModifiedProperties();
        }

    }
}

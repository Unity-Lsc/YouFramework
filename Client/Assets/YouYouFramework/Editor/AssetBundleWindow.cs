using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AssetBundle管理窗口
/// </summary>
public class AssetBundleWindow : EditorWindow
{
    /// <summary>
    /// AsserBundle管理类
    /// </summary>
    private AssetBundleDAL mAssetBundleDAL;
    /// <summary>
    /// 存储的XML数据集合
    /// </summary>
    private List<AssetBundleEntity> mXmlDataList;
    /// <summary>
    /// 存储选中的选项集合
    /// </summary>
    private Dictionary<string, bool> mSelectDict;

    //存储资源类别的集合
    private string[] mTags = { "All", "Scene", "Role", "Effect", "Audio", "UI", "UIFont", "Shader", "DataTable", "Lua", "None" };
    //当前选择标签的索引值
    private int mCurTagIndex = 0;
    //选中的标签索引值
    private int mSelectTagIndex = -1;

    //存储打包平台的集合
    private string[] mBuildTargets = { "Windows", "Android", "iOS" };
    //选中的打包平台索引值
    private int mSelectTargetIndex = -1;
#if UNITY_STANDALONE_WIN
    private BuildTarget mCurBuildTarget = BuildTarget.StandaloneWindows;
    private int mCurTagetIndex = 0;
#elif UNITY_ANDROID
    private BuildTarget mCurBuildTarget = BuildTarget.Android;
    private int mCurTagetIndex = 1;
#elif UNITY_IOS
    private BuildTarget mCurBuildTarget = BuildTarget.iOS;
    private int mCurTagetIndex = 2;
#endif

    //绘制列表的 滚轮位置
    private Vector2 mScrollPos;

    public AssetBundleWindow() {
        string xmlPath = Application.dataPath + "/YouYouFramework/Managers/Resource/AssetBundleConfig.xml";
        mAssetBundleDAL = new AssetBundleDAL(xmlPath);
        mXmlDataList = mAssetBundleDAL.GetList();

        mSelectDict = new Dictionary<string, bool>();
        for (int i = 0; i < mXmlDataList.Count; i++) {
            mSelectDict[mXmlDataList[i].Key] = true;
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void OnGUI() {
        if (mXmlDataList == null) return;

        #region 按钮行
        GUILayout.BeginHorizontal("box");

        mSelectTagIndex = EditorGUILayout.Popup(mCurTagIndex, mTags, GUILayout.Width(100));
        if(mSelectTagIndex != mCurTagIndex) {
            mCurTagIndex = mSelectTagIndex;
            EditorApplication.delayCall = OnSelectTagCallback;
        }

        mSelectTargetIndex = EditorGUILayout.Popup(mCurTagetIndex, mBuildTargets, GUILayout.Width(100));
        if (mSelectTargetIndex != mCurTagetIndex) {
            mCurTagetIndex = mSelectTargetIndex;
            EditorApplication.delayCall = OnSelectTargetCallback;
        }

        if (GUILayout.Button("保存设置", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnSaveAssetBundleCallback;
        }

        if (GUILayout.Button("AssetBundle打包", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnAssetBundleCallback;
        }

        if (GUILayout.Button("清空AssetBundle", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnClearAssetBundleCallback;
        }

        if (GUILayout.Button("拷贝数据表", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnCopyDataTableCallback;
        }

        if (GUILayout.Button("生成版本文件", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnCreateVersionFileCallback;
        }

        //让横向box条可以显示到屏幕最右边
        EditorGUILayout.Space();

        GUILayout.EndHorizontal();
        #endregion

        #region 资源包标题行
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("包名");
        GUILayout.Label("标记", GUILayout.Width(100));
        GUILayout.Label("是否文件夹", GUILayout.Width(200));
        GUILayout.Label("是否初始资源", GUILayout.Width(200));
        GUILayout.EndHorizontal();
        #endregion

        #region 资源包展示
        GUILayout.BeginVertical();
        mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);

        for (int i = 0; i < mXmlDataList.Count; i++) {
            AssetBundleEntity entity = mXmlDataList[i];
            GUILayout.BeginHorizontal("box");
            mSelectDict[entity.Key] = GUILayout.Toggle(mSelectDict[entity.Key], "", GUILayout.Width(20));
            GUILayout.Label(entity.Name);
            GUILayout.Label(entity.Tag, GUILayout.Width(100));
            GUILayout.Label(entity.IsFolder.ToString(), GUILayout.Width(200));
            GUILayout.Label(entity.IsFirstData.ToString(), GUILayout.Width(200));
            GUILayout.EndHorizontal();

            foreach (var path in entity.PathList)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Space(40);
                GUILayout.Label(path);
                GUILayout.EndHorizontal();
            }

        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
        #endregion

    }

    /// <summary>
    /// 选定Tag的回调
    /// </summary>
    private void OnSelectTagCallback() {
        switch (mCurTagIndex) {
            default:
            case 0: SetTagToggle("All"); break;//全选
            case 1: SetTagToggle("Scene"); break;//Scene
            case 2: SetTagToggle("Role"); break;//Role
            case 3: SetTagToggle("Effect"); break;//Effect
            case 4: SetTagToggle("Audio"); break;//Audio
            case 5: SetTagToggle("UI"); break;//UI
            case 6: SetTagToggle("UIFont"); break;//UIFont
            case 7: SetTagToggle("Shader"); break;//Shader
            case 8: SetTagToggle("DataTable"); break;//DataTable
            case 9: SetTagToggle("Lua"); break;//Lua
            case 10: SetTagToggle("None"); break;//None
        }
        Debug.LogFormat("当前选择的Tags:{0}", mTags[mCurTagIndex]);
    }

    private void SetTagToggle(string tag) {
        foreach (var entity in mXmlDataList) {
            if(tag == "All") {
                mSelectDict[entity.Key] = true;
            } else if(tag == "None") {
                mSelectDict[entity.Key] = false;
            } else {
                mSelectDict[entity.Key] = entity.Tag.Equals(tag, StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }

    /// <summary>
    /// 选定Target回调
    /// </summary>
    private void OnSelectTargetCallback() {
        switch (mCurTagetIndex) {
            case 0: mCurBuildTarget = BuildTarget.StandaloneWindows; break;//Windows
            case 1: mCurBuildTarget = BuildTarget.Android; break;//Android
            case 2: mCurBuildTarget = BuildTarget.iOS; break;//iOS
        }
        Debug.LogFormat("当前选择的平台:{0}", mBuildTargets[mCurTagetIndex]);
    }

    private void OnSaveAssetBundleCallback() {

        //需要打包的对象
        List<AssetBundleEntity> buildList = new List<AssetBundleEntity>();
        foreach (var entity in mXmlDataList) {
            entity.IsSelected = mSelectDict[entity.Key];
            if (mSelectDict[entity.Key])
                buildList.Add(entity);
        }

        //循环设置文件夹(包括子文件夹)里边的资源项
        int count = buildList.Count;
        for (int i = 0; i < count; i++) {
            var entity = buildList[i];

            int lstCount = entity.PathList.Count;
            string[] filesOrFolders = new string[lstCount];
            //文件夹下所有资源 一个资源一个包
            if (entity.IsFolder) {
                for (int j = 0; j < lstCount; j++) {
                    filesOrFolders[j] = Application.dataPath + "/" + entity.PathList[j];
                }
                SaveFolderSettings(filesOrFolders, !entity.IsSelected);
            }
            //文件夹打成一个包
            else {
                for (int j = 0; j < lstCount; j++) {
                    filesOrFolders[j] = Application.dataPath + "/" + entity.PathList[j];
                    SaveFileSettings(filesOrFolders[j], !entity.IsSelected);
                }
            }
        }

    }



    /// <summary>
    /// 保存文件夹的标注信息
    /// </summary>
    /// <param name="floders">操作的文件夹集合</param>
    /// <param name="isSetNull">是否要将标注设置为null</param>
    private void SaveFolderSettings(string[] floders, bool isSetNull) {
        foreach (var folderPath in floders)
        {
            //1.先处理文件夹下的文件
            string[] files = Directory.GetFiles(folderPath);

            //2.对文件进行设置
            foreach (var filePath in files)
            {
                SaveFileSettings(filePath, isSetNull);
            }

            //3.处理文件夹下的子文件夹
            string[] folders = Directory.GetDirectories(folderPath);
            SaveFolderSettings(folders, isSetNull);

        }
    }

    /// <summary>
    /// 保存文件的标注信息
    /// </summary>
    /// <param name="filePath">操作的文件</param>
    /// <param name="isSetNull">是否要将标注设置为null</param>
    private void SaveFileSettings(string filePath, bool isSetNull) {
        FileInfo fileInfo = new FileInfo(filePath);
        if(!fileInfo.Extension.Equals(".meta", StringComparison.CurrentCultureIgnoreCase)) {
            int index = filePath.IndexOf("Assets/", StringComparison.CurrentCultureIgnoreCase);

            //路径
            string newPath = filePath.Substring(index);
            
            //文件名
            string fileName = newPath.Replace("Assets/", "").Replace(fileInfo.Extension, "");

            //后缀
            string variant = fileInfo.Extension.Equals(".unity", StringComparison.CurrentCultureIgnoreCase) ? "unity" : "assetbundle";

            AssetImporter importer = AssetImporter.GetAtPath(newPath);

            if (isSetNull) {
                importer.SetAssetBundleNameAndVariant(null, null);
            } else {
                importer.SetAssetBundleNameAndVariant(fileName, variant);
            }
            importer.SaveAndReimport();
        }
    }

    /// <summary>
    /// AssetBundle打包回调
    /// </summary>
    private void OnAssetBundleCallback() {

        string toPath = Application.dataPath + "/../AssetBundles/" + mBuildTargets[mCurTagetIndex];
        if (!Directory.Exists(toPath)) {
            Directory.CreateDirectory(toPath);
        }

        BuildPipeline.BuildAssetBundles(toPath, BuildAssetBundleOptions.None, mCurBuildTarget);
        Debug.Log("打包完毕");
    }

    /// <summary>
    /// 清空AssetBundle回调
    /// </summary>
    private void OnClearAssetBundleCallback() {
        //先清空所有资源的AssetLabel
        ClearAllAssetLabel();

        //再清空AssetBundle资源
        string path = Application.dataPath + "/../AssetBundles/" + mBuildTargets[mCurTagetIndex];
        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }
        Debug.Log("清空AssetBundle完毕");
    }

    /// <summary>
    /// 清空所有资源的AssetLabel
    /// </summary>
    private void ClearAllAssetLabel() {
        //获取所有的AssetBundle名称
        string[] abNames = AssetDatabase.GetAllAssetBundleNames();

        //强制删除所有AssetBundle名称
        for (int i = 0; i < abNames.Length; i++) {
            AssetDatabase.RemoveAssetBundleName(abNames[i], true);
        }
    }

    /// <summary>
    /// 拷贝数据表
    /// </summary>
    private void OnCopyDataTableCallback() {
        string fromPath = Application.dataPath + "/Download/DataTable";
        string toPath = Application.dataPath + "/../AssetBundles/" + mBuildTargets[mCurTagetIndex] + "/Download/DataTable";
        IOUtil.CopyDirectory(fromPath, toPath);
        Debug.Log("拷贝数据表完毕");
    }

    /// <summary>
    /// 生成版本文件
    /// </summary>
    private void OnCreateVersionFileCallback() {
        string path = Application.dataPath + "/../AssetBundles/" + mBuildTargets[mCurTagetIndex];
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        string verPath = path + "/VersionFile.txt";//版本文件路径
        //如果版本文件存在 先进行删除
        IOUtil.DeleteFile(verPath);

        StringBuilder sbr = new StringBuilder();

        DirectoryInfo dir = new DirectoryInfo(path);

        //拿到路径下的所有文件
        FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++) {
            var file = files[i];
            string fullName = file.FullName;//全名 包含扩展名
            //相对路径
            string name = fullName.Substring(fullName.IndexOf(mBuildTargets[mCurTagetIndex]) + mBuildTargets[mCurTagetIndex].Length + 1);

            string md5 = EncryptUtil.GetFileMD5(fullName);
            if (md5 == null) continue;

            //文件大小 单位K
            string size = Math.Ceiling(file.Length / 1024f).ToString();

            bool isFirstData = false;//是否是初始数据
            bool isBreak = false;

            for (int j = 0; j < mXmlDataList.Count; j++) {
                foreach (var xmlPath in mXmlDataList[j].PathList) {
                    string tempPath = xmlPath;
                    if(xmlPath.IndexOf(".") != -1) {
                        tempPath = xmlPath.Substring(0, xmlPath.IndexOf("."));
                    }
                    if(name.IndexOf(tempPath, StringComparison.CurrentCultureIgnoreCase) != -1) {
                        isFirstData = mXmlDataList[j].IsFirstData;
                        isBreak = true;
                        break;
                    }
                }
                if (isBreak) break;
            }

            //一般来说，表格数据都属于初始数据
            if(name.IndexOf("DataTable") != -1) {
                isFirstData = true;
            }

            string strLine = string.Format("{0} {1} {2} {3}", name, md5, size, isFirstData ? 1 : 0);
            sbr.AppendLine(strLine);
            
        }
        IOUtil.CreateTextFile(verPath, sbr.ToString());
        Debug.Log("创建版本文件成功");
    }

}

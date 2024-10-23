using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YouYou;

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
    /// <summary>
    /// 需要打包的资源包
    /// </summary>
    private List<AssetBundleBuild> mNeedBuildList;

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

    private void OnEnable() {
        string xmlPath = Application.dataPath + "/YouYouFramework/Managers/Resource/AssetBundleConfig.xml";
        mAssetBundleDAL = new AssetBundleDAL(xmlPath);
        mXmlDataList = mAssetBundleDAL.GetList();
        mNeedBuildList = new List<AssetBundleBuild>();

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

        if (GUILayout.Button("AssetBundle打包", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnAssetBundleCallback;
        }

        if (GUILayout.Button("清空AssetBundle", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnClearAssetBundleCallback;
        }

        if (GUILayout.Button("生成版本文件", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnCreateVersionFileCallback;
        }

        //让横向box条可以显示到屏幕最右边
        EditorGUILayout.Space();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");

        if (GUILayout.Button("生成依赖文件", GUILayout.Width(200))) {
            EditorApplication.delayCall = CreateDependenciesFile;
        }

        if (GUILayout.Button("升级资源版本号(" + mAssetBundleDAL.GetVersion() + ")", GUILayout.Width(200))) {
            EditorApplication.delayCall = OnUpdateVersionCallback;
        }

        EditorGUILayout.Space();
        GUILayout.EndHorizontal();

        #endregion

        #region 资源包标题行
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("包名");
        GUILayout.Label("标记", GUILayout.Width(100));
        GUILayout.Label("是否打成一个资源包", GUILayout.Width(200));
        GUILayout.Label("是否初始资源", GUILayout.Width(200));
        GUILayout.Label("是否加密", GUILayout.Width(200));
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
            GUILayout.Label(entity.Overall.ToString(), GUILayout.Width(200));
            GUILayout.Label(entity.IsFirstData.ToString(), GUILayout.Width(200));
            GUILayout.Label(entity.IsEncrypt.ToString(), GUILayout.Width(200));
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
        GameEntry.Log("当前选择的Tags:{0}", LogCategory.Resource, mTags[mCurTagIndex]);
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

    /// <summary>
    /// 打包AssetBundle回调
    /// </summary>
    private void OnAssetBundleCallback() {
        mNeedBuildList.Clear();

        //需要打包的对象
        List<AssetBundleEntity> buildList = GetNeedBuildList();

        int len = buildList.Count;

        for (int i = 0; i < len; i++) {
            var entity = buildList[i];
            int lenPath = entity.PathList.Count;
            for (int j = 0; j < lenPath; j++) {
                string path = entity.PathList[j];
                BuildAssetBundleForPath(path, entity.Overall, entity.Tag == "Scene");
            }
        }

        if(mNeedBuildList.Count <= 0) {
            GameEntry.Log("未找到需要打包的内容", LogCategory.Resource);
            return;
        }

        string toPath = GetToPath();
        if (!Directory.Exists(toPath)) {
            Directory.CreateDirectory(toPath);
        }

        BuildPipeline.BuildAssetBundles(toPath, mNeedBuildList.ToArray(), BuildAssetBundleOptions.None, mCurBuildTarget);
        GameEntry.Log("打包完毕", LogCategory.Resource);

        AssetBundleEncrypt();
        GameEntry.Log("资源包加密完毕", LogCategory.Resource);

        CreateDependenciesFile();
        GameEntry.Log("生成资源依赖文件 AssetInfo.bytes 完毕", LogCategory.Resource);

        OnCreateVersionFileCallback();
        GameEntry.Log("创建版本文件成功", LogCategory.Resource);

    }

    /// <summary>
    /// 根据路径打包资源
    /// </summary>
    /// <param name="ovrall">是否打成一个资源包</param>
    private void BuildAssetBundleForPath(string path, bool overall, bool isScene = false) {
        string fullPath = Application.dataPath + "/" + path;
        //1.拿下文件夹下所有文件
        DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
        //2.拿下文件夹下的所有文件
        FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

        if (overall) {
            //打成一个资源包
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = path;
            build.assetBundleVariant = "assetbundle";
            string[] arr = GetValidateFiles(fileInfos);
            build.assetNames = arr;
            mNeedBuildList.Add(build);
        } else {
            //每个文件打成一个包
            string[] arr = GetValidateFiles(fileInfos);
            for (int i = 0; i < arr.Length; i++) {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = arr[i].Substring(0, arr[i].LastIndexOf('.')).Replace("Assets/", "");
                build.assetBundleVariant = isScene ? "unity3d" : "assetbundle";
                build.assetNames = new string[] { arr[i] };
                mNeedBuildList.Add(build);
            }
        }
    }

    private string[] GetValidateFiles(FileInfo[] arrFiles) {
        List<string> lst = new List<string>();
        int len = arrFiles.Length;

        for (int i = 0; i < len; i++) {
            FileInfo fileInfo = arrFiles[i];
            if (!fileInfo.Extension.Equals(".meta", StringComparison.CurrentCultureIgnoreCase)) {
                lst.Add("Assets" + fileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, ""));
            }
        }
        return lst.ToArray();

    }

    /// <summary>
    /// 生成依赖关系文件
    /// </summary>
    private void CreateDependenciesFile() {
        List<AssetEntity> tempList = new List<AssetEntity>();

        //需要打包的对象
        List<AssetBundleEntity> buildList = GetNeedBuildList();

        int len = buildList.Count;
        for (int i = 0; i < len; i++) {
            AssetBundleEntity entity = buildList[i];
            for (int j = 0; j < entity.PathList.Count; j++) {
                string path = Application.dataPath + "/" + entity.PathList[j];
                CollectFileInfo(tempList, path);
            }
        }

        len = tempList.Count;
        // 资源列表
        List<AssetEntity> assetList = new List<AssetEntity>();
        for (int i = 0; i < len; i++) {
            AssetEntity entity = tempList[i];

            AssetEntity newEntity = new AssetEntity();
            newEntity.Category = entity.Category;
            newEntity.AssetName = entity.AssetFullName.Substring(entity.AssetFullName.LastIndexOf("/") + 1);
            newEntity.AssetName = newEntity.AssetName.Substring(0, newEntity.AssetName.LastIndexOf("."));
            newEntity.AssetFullName = entity.AssetFullName;
            newEntity.AssetBundleName = entity.AssetBundleName;
            assetList.Add(newEntity);

            //场景不需要检查依赖项
            if (entity.Category == AssetCategory.Scenes) continue;

            newEntity.DependsAssetList = new List<AssetDependsEntity>();
            string[] arr = AssetDatabase.GetDependencies(entity.AssetFullName);
            foreach (string str in arr) {
                if(!str.Equals(newEntity.AssetFullName, StringComparison.CurrentCultureIgnoreCase) && IsAssetInList(tempList, str)) {
                    AssetDependsEntity dependsEntity = new AssetDependsEntity();
                    dependsEntity.Category = GetAssetCategory(str);
                    dependsEntity.AssetFullName = str;

                    //把依赖资源加到依赖资源列表中
                    newEntity.DependsAssetList.Add(dependsEntity);
                }
            }
        }

        //生成一个Json文件
        string targetPath = GetToPath();
        if (!Directory.Exists(targetPath)) {
            Directory.CreateDirectory(targetPath);
        }
        string strJsonFilePath = targetPath + "/AssetInfo.json"; //Json文件路径
        IOUtil.CreateTextFile(strJsonFilePath, LitJson.JsonMapper.ToJson(assetList));
        GameEntry.Log("生成资源依赖文件 AssetInfo.json 完毕", LogCategory.Resource);

        MMO_MemoryStream ms = new MMO_MemoryStream();
        //生成二进制文件
        len = assetList.Count;
        ms.WriteInt(len);

        for (int i = 0; i < len; i++) {
            AssetEntity entity = assetList[i];
            ms.WriteByte((byte)entity.Category);
            ms.WriteUTF8String(entity.AssetFullName);
            ms.WriteUTF8String(entity.AssetBundleName);

            if (entity.DependsAssetList != null) {
                //添加依赖资源
                int depLen = entity.DependsAssetList.Count;
                ms.WriteInt(depLen);
                for (int j = 0; j < depLen; j++) {
                    AssetDependsEntity assetDepends = entity.DependsAssetList[j];
                    ms.WriteByte((byte)assetDepends.Category);
                    ms.WriteUTF8String(assetDepends.AssetFullName);
                }
            } else {
                ms.WriteInt(0);
            }
        }

        string filePath = targetPath + "/AssetInfo.bytes"; //版本文件路径
        byte[] buffer = ms.ToArray();
        buffer = ZlibHelper.CompressBytes(buffer);
        FileStream fs = new FileStream(filePath, FileMode.Create);
        fs.Write(buffer, 0, buffer.Length);
        fs.Close();
        fs.Dispose();
    }

    /// <summary>
    /// 判断某个资源是否存在于资源列表
    /// </summary>
    private bool IsAssetInList(List<AssetEntity> tempList, string assetFullName) {
        int len = tempList.Count;
        for (int i = 0; i < len; i++) {
            AssetEntity entity = tempList[i];
            if (entity.AssetFullName.Equals(assetFullName, StringComparison.CurrentCultureIgnoreCase)) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 收集文件信息
    /// </summary>
    private void CollectFileInfo(List<AssetEntity> tempList, string folderPath) {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        //拿下文件夹下所有文件
        FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < fileInfos.Length; i++) {
            FileInfo fileInfo = fileInfos[i];
            if (fileInfo.Extension == ".meta") continue;
            string filePath = fileInfo.FullName;//全名 包含路径扩展名
            int index = filePath.IndexOf("Assets\\", StringComparison.CurrentCultureIgnoreCase);
            string newPath = filePath.Substring(index);
            if (newPath.IndexOf(".idea") != -1) continue;

            AssetEntity entity = new AssetEntity();
            entity.AssetFullName = newPath.Replace("\\", "/");
            entity.Category = GetAssetCategory(newPath.Replace(fileInfo.Name, ""));//去掉文件名,只保留路径
            entity.AssetBundleName = (GetAssetBundleName(newPath) + ".assetbundle").ToLower();
            tempList.Add(entity);
        }
    }

    /// <summary>
    /// 获取资源的分类类别
    /// </summary>
    private AssetCategory GetAssetCategory(string filePath) {
        AssetCategory category = AssetCategory.None;
        if (filePath.IndexOf("Reporter", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.Reporter;
        } else if (filePath.IndexOf("Audio", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.Audio;
        } else if (filePath.IndexOf("CusShaders", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.CusShaders;
        } else if (filePath.IndexOf("DataTable", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.DataTable;
        } else if (filePath.IndexOf("EffectSources", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.EffectSources;
        } else if (filePath.IndexOf("RoleEffectPrefab", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.RoleEffectPrefab;
        } else if (filePath.IndexOf("UIEffectPrefab", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.UIEffectPrefab;
        } else if (filePath.IndexOf("RolePrefab", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.RolePrefab;
        } else if (filePath.IndexOf("RoleSources", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.RoleSources;
        } else if (filePath.IndexOf("Scenes", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.Scenes;
        } else if (filePath.IndexOf("UIFont", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.UIFont;
        } else if (filePath.IndexOf("UIPrefab", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.UIPrefab;
        } else if (filePath.IndexOf("UIRes", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.UIRes;
        } else if (filePath.IndexOf("xLuaLogic", StringComparison.CurrentCultureIgnoreCase) != -1) {
            category = AssetCategory.xLuaLogic;
        }
        return category;
    }

    /// <summary>
    /// 获取资源包的名称
    /// </summary>
    private string GetAssetBundleName(string newPath) {
        string path = newPath.Replace("\\", "/");
        int len = mXmlDataList.Count;
        for (int i = 0; i < len; i++) {
            var entity = mXmlDataList[i];
            for (int j = 0; j < entity.PathList.Count; j++) {
                if(path.IndexOf(entity.PathList[j], StringComparison.CurrentCultureIgnoreCase) > -1) {
                    if (entity.Overall) {
                        //文件夹是个整包 则返回这个文件夹名字
                        return entity.PathList[j].ToLower();
                    }else {
                        //零散资源
                        return path.Substring(0, path.LastIndexOf('.')).ToLower().Replace("assets/", "");
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 资源包加密
    /// </summary>
    private void AssetBundleEncrypt() {
        //需要打包的对象
        List<AssetBundleEntity> buildList = GetNeedBuildList();
        int len = buildList.Count;
        for (int i = 0; i < len; i++) {
            var entity = buildList[i];
            if (entity.IsEncrypt) {
                string[] folders = new string[entity.PathList.Count];
                for (int j = 0; j < entity.PathList.Count; j++) {
                    string path = GetToPath() + "/" + entity.PathList[j];
                    if (entity.Overall) {
                        string str = ".assetbundle";
                        path = path + str;
                        EncryptFile(path);
                    } else {
                        EncryptFolder(path);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 加密文件夹
    /// </summary>
    /// <param name="folderPath">要加密的文件夹</param>
    private void EncryptFolder(string folderPath) {
        DirectoryInfo info = new DirectoryInfo(folderPath);
        FileInfo[] fileInfos = info.GetFiles("*", SearchOption.AllDirectories);
        foreach (var file in fileInfos) {
            EncryptFile(file.FullName);
        }
    }

    /// <summary>
    /// 加密文件
    /// </summary>
    /// <param name="filePath">要加密文件的路径</param>
    private void EncryptFile(string filePath) {
        FileInfo fileInfo = new FileInfo(filePath);
        if(!fileInfo.Extension.Equals(".assetbundle", StringComparison.CurrentCultureIgnoreCase)) {
            return;
        }

        byte[] buffer = null;
        using (FileStream fs = new FileStream(filePath, FileMode.Open)) {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
        }

        buffer = SecurityUtil.Xor(buffer);

        using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
            fs.Write(buffer, 0, buffer.Length);
            fs.Flush();
        }

    }

    /// <summary>
    /// 清空AssetBundle回调
    /// </summary>
    private void OnClearAssetBundleCallback() {
        //先清空所有资源的AssetLabel
        ClearAllAssetLabel();

        //再清空AssetBundle资源
        string path = GetToPath();
        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
        }
        GameEntry.Log("清空AssetBundle完毕", LogCategory.Resource);
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
    /// 生成版本文件
    /// </summary>
    private void OnCreateVersionFileCallback() {
        string path = GetToPath();
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
        string verPath = path + "/VersionFile.txt";//版本文件路径(txt)
        //如果版本文件存在 先进行删除
        IOUtil.DeleteFile(verPath);

        StringBuilder sbr = new StringBuilder();

        DirectoryInfo dir = new DirectoryInfo(path);

        //拿到路径下的所有文件
        FileInfo[] files = dir.GetFiles("*", SearchOption.AllDirectories);
        sbr.AppendLine(mAssetBundleDAL.GetVersion());
        for (int i = 0; i < files.Length; i++) {
            var file = files[i];
            if(file.Extension == ".manifest") {
                continue;
            }
            string fullName = file.FullName;//全名 包含扩展名
            //相对路径
            string name = fullName.Substring(fullName.IndexOf(mBuildTargets[mCurTagetIndex]) + mBuildTargets[mCurTagetIndex].Length + 1);

            string md5 = EncryptUtil.GetFileMD5(fullName);
            if (md5 == null) continue;

            //文件大小 单位K
            string size = Math.Ceiling(file.Length / 1024f).ToString();

            bool isFirstData = false;//是否是初始数据
            bool isEncrypt = false;//文件是否加密
            bool isBreak = false;

            for (int j = 0; j < mXmlDataList.Count; j++) {
                foreach (var xmlPath in mXmlDataList[j].PathList) {
                    string tempPath = xmlPath;
                    if(xmlPath.IndexOf(".") != -1) {
                        tempPath = xmlPath.Substring(0, xmlPath.IndexOf("."));
                    }
                    name = name.Replace("\\", "/");
                    if(name.IndexOf(tempPath, StringComparison.CurrentCultureIgnoreCase) != -1) {
                        isFirstData = mXmlDataList[j].IsFirstData;
                        isEncrypt = mXmlDataList[j].IsEncrypt;
                        isBreak = true;
                        break;
                    }
                }
                if (isBreak) break;
            }

            string strLine = string.Format("{0}|{1}|{2}|{3}|{4}", name, md5, size, isFirstData ? 1 : 0, isEncrypt ? 1 : 0);
            sbr.AppendLine(strLine);
            
        }
        IOUtil.CreateTextFile(verPath, sbr.ToString());

        MMO_MemoryStream ms = new MMO_MemoryStream();
        string str = sbr.ToString().Trim();
        string[] arr = str.Split('\n');
        int len = arr.Length;
        ms.WriteInt(len);
        for (int i = 0; i < len; i++) {
            if(i == 0) {
                ms.WriteUTF8String(arr[i]);
            } else {
                string[] arrInner = arr[i].Split('|');
                ms.WriteUTF8String(arrInner[0]);
                ms.WriteUTF8String(arrInner[1]);
                ms.WriteInt(int.Parse(arrInner[2]));
                ms.WriteByte(byte.Parse(arrInner[3]));
                ms.WriteByte(byte.Parse(arrInner[4]));
            }
        }

        string filePath = path + "/" + ConstDefine.VersionFileName;//版本文件路径(.bytes)
        byte[] buffer = ms.ToArray();
        ms.Dispose();
        ms.Close();

        buffer = ZlibHelper.CompressBytes(buffer);
        using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
            fs.Dispose();
        }
    }

    /// <summary>
    /// 升级资源版本号
    /// </summary>
    private void OnUpdateVersionCallback() {
        mAssetBundleDAL.UpdateVersion();
    }

    /// <summary>
    /// 根据标签获取需要打包的列表
    /// </summary>
    private List<AssetBundleEntity> GetNeedBuildList() {
        List<AssetBundleEntity> buildList = new List<AssetBundleEntity>();
        foreach (var entity in mXmlDataList) {
            if (mSelectDict[entity.Key])
                buildList.Add(entity);
        }
        return buildList;
    }

    /// <summary>
    /// 获取AssetBundle的存储路径
    /// </summary>
    /// <returns></returns>
    private string GetToPath() {
        return Application.dataPath + "/../AssetBundles/" + mAssetBundleDAL.GetVersion() + "/" + mBuildTargets[mCurTagetIndex];
    }

}

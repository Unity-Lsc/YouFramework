using UnityEngine;
using UnityEditor;
using YouYou;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System;

public class Menu
{
    [MenuItem("工具/设置")]
    public static void Settings() {

        SettingsWindow window = EditorWindow.GetWindow<SettingsWindow>();
        window.titleContent = new GUIContent("全局设置");
        window.Show();

    }

    [MenuItem("工具/资源管理/资源打包管理")]
    public static void AssetBundleCreate() {

        AssetBundleWindow window = EditorWindow.GetWindow<AssetBundleWindow>();
        window.titleContent = new GUIContent("资源打包打包");
        window.Show();
        
    }

    [MenuItem("工具/资源管理/打开persisdentDataPath")]
    public static void AssetBundleOpenPersisdentDataPath() {
        string outPath = Application.persistentDataPath;
        if (!Directory.Exists(outPath)) {
            Directory.CreateDirectory(outPath);
        }
        outPath = outPath.Replace("/", "\\");
        System.Diagnostics.Process.Start("explorer.exe", outPath);
    }

    [MenuItem("工具/资源管理/初始资源拷贝到StreamAssets")]
    public static void AssetBundleCopyToStreamAssets() {
        string toPath = Application.streamingAssetsPath + "/AssetBundles/";
        if (Directory.Exists(toPath)) {
            Directory.Delete(toPath, true);
        }
        Directory.CreateDirectory(toPath);

        IOUtil.CopyDirectory(Application.persistentDataPath, toPath);

        //重新生成版本文件
        //先读取persisdentDataPath里边的版本文件,这个版本文件中存放了所有的资源包信息
        byte[] buffer = IOUtil.GetFileBuffer(Application.persistentDataPath + "/VersionFile.bytes");
        string version = "";
        Dictionary<string, AssetBundleInfoEntity> dict = ResourceManager.GetAssetBundleVersionList(buffer, ref version);
        Dictionary<string, AssetBundleInfoEntity> newDict = new Dictionary<string, AssetBundleInfoEntity>();

        DirectoryInfo directory = new DirectoryInfo(toPath);

        //拿到文件夹下所有文件
        FileInfo[] fileInfos = directory.GetFiles("*", SearchOption.AllDirectories);

        for (int i = 0; i < fileInfos.Length; i++) {
            FileInfo fileInfo = fileInfos[i];
            string fullName = fileInfo.FullName.Replace("\\", "/");//全名 包含路径扩展名
            string name = fullName.Replace(toPath, "").Replace(".assetbundle", "").Replace(".unity3d", "");

            if (name.Equals("AssetInfo.json", StringComparison.CurrentCultureIgnoreCase) 
                || name.Equals("Windows", StringComparison.CurrentCultureIgnoreCase)
                || name.Equals("Windows.manifest", StringComparison.CurrentCultureIgnoreCase)

                || name.Equals("Android", StringComparison.CurrentCultureIgnoreCase)
                || name.Equals("Android.manifest", StringComparison.CurrentCultureIgnoreCase)

                || name.Equals("iOS", StringComparison.CurrentCultureIgnoreCase)
                || name.Equals("iOS.manifest", StringComparison.CurrentCultureIgnoreCase)) {
                File.Delete(fileInfo.FullName);
                continue;
            }

            dict.TryGetValue(name, out AssetBundleInfoEntity entity);
            if (entity != null) {
                newDict[name] = entity;
            }
        }

        StringBuilder sbr = new StringBuilder();
        sbr.AppendLine(version);

        foreach (var item in newDict) {
            AssetBundleInfoEntity entity = item.Value;
            string strLine = string.Format("{0}|{1}|{2}|{3}|{4}", entity.AssetBundleName, entity.MD5, entity.Size, entity.IsFirstData ? 1 : 0, entity.IsEncrypt ? 1 : 0);
            sbr.AppendLine(strLine);
        }

        IOUtil.CreateTextFile(toPath + "/VersionFile.txt", sbr.ToString());

        MMO_MemoryStream ms = new MMO_MemoryStream();
        string str = sbr.ToString().Trim();
        string[] arr = str.Split('\n');
        int len = arr.Length;
        ms.WriteInt(len);
        for (int i = 0; i < len; i++) {
            if(i== 0) {
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

        string filePath = toPath + "/VersionFile.bytes";//版本文件路径
        buffer = ms.ToArray();
        buffer = ZlibHelper.CompressBytes(buffer);
        FileStream fs = new FileStream(filePath, FileMode.Create);
        fs.Write(buffer, 0, buffer.Length);
        fs.Close();

        AssetDatabase.Refresh();
        GameEntry.Log("初始资源拷贝到StreamAssets完毕", LogCategory.Resource);

    }

    [MenuItem("工具/生成LuaView脚本")]
    public static void CreateLuaView() {
        if (Selection.transforms.Length == 0) return;
        Transform tran = Selection.transforms[0];

        LuaForm luaForm = tran.GetComponent<LuaForm>();
        string viewName = tran.gameObject.name;
        if (luaForm == null) {
            Debug.LogError(string.Format("该UI组件:{0}上没有挂载LuaForm脚本", viewName));
            return;
        }

        LuaComp[] luaComps = luaForm.LuaComps;
        int len = luaComps.Length;

        StringBuilder sbr = new StringBuilder();
        sbr.AppendFormat("");
        sbr.AppendFormat("{0}View = {{}};\n", viewName);
        sbr.AppendFormat("local this = {0}View;\n", viewName);
        sbr.AppendFormat("\n");
        for (int i = 0; i < len; i++) {
            LuaComp comp = luaComps[i];
            sbr.AppendFormat("local {0}Index = {1};\n", comp.Name, i);
        }
        sbr.AppendFormat("\n");
        sbr.AppendFormat("function {0}View.OnInit(transform, userData)\n", viewName);
        sbr.AppendFormat("    this.InitView(transform);\n");
        sbr.AppendFormat("    {0}Ctrl.OnInit(userData);\n", viewName);
        sbr.AppendFormat("end\n");
        sbr.AppendFormat("\n");
        sbr.AppendFormat("function {0}View.InitView(transform)\n", viewName);
        sbr.AppendFormat("    this.LuaForm = transform:GetComponent(typeof(CS.YouYou.LuaForm));\n");
        for (int i = 0; i < len; i++) {
            LuaComp comp = luaComps[i];
            sbr.AppendFormat("    this.{0} = this.LuaForm:GetLuaComp({0}Index);\n", comp.Name);
        }
        sbr.AppendFormat("end\n");
        sbr.AppendFormat("\n");
        sbr.AppendFormat("function {0}View.OnOpen(userData)\n", viewName);
        sbr.AppendFormat("    {0}Ctrl.OnOpen(userData);\n", viewName);
        sbr.AppendFormat("end\n");
        sbr.AppendFormat("\n");
        sbr.AppendFormat("function {0}View.OnClose()\n", viewName);
        sbr.AppendFormat("    {0}Ctrl.OnClose();\n", viewName);
        sbr.AppendFormat("end\n");
        sbr.AppendFormat("\n");
        sbr.AppendFormat("function {0}View.OnBeforeDestroy()\n", viewName);
        sbr.AppendFormat("    {0}Ctrl.OnBeforeDestroy();\n", viewName);
        sbr.AppendFormat("end\n");

        string path = Application.dataPath + "/Download/xLuaLogic/Modules/Temp/" + viewName + "View.bytes";

        using (FileStream fs = new FileStream(path, FileMode.Create)) {
            using (StreamWriter sw = new StreamWriter(fs)) {
                sw.Write(sbr.ToString());
            }
        }
    }

}

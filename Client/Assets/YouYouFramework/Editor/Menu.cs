using UnityEngine;
using UnityEditor;
using YouYou;
using System.Text;
using System.IO;
using System.Collections.Generic;

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

    [MenuItem("工具/资源管理/初始资源拷贝到StreamAssets")]
    public static void AssetBundleCopyToStreamAssets() {
        string toPath = Application.streamingAssetsPath + "/AssetBundles/";
        if (Directory.Exists(toPath)) {
            Directory.Delete(toPath, true);
        }
        Directory.CreateDirectory(toPath);

        IOUtil.CopyDirectory(Application.persistentDataPath, toPath);
        AssetDatabase.Refresh();
        Debug.Log("初始资源拷贝到StreamAssets完毕");

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

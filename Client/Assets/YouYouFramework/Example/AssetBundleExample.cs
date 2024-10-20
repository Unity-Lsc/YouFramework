using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetBundleExample : MonoBehaviour
{

    private string mLocalFilePath;

    private AssetBundle mBundleUIRes;

    private AssetBundle mbundleUIPrefab;

    private List<Object> mObjList = new List<Object>();


    void Start()
    {
        mLocalFilePath = @"G:\UnityProject\YouFramework\Client\AssetBundles\Windows\";

        AssetBundleLoadUIRes("download/ui/uires/uicommon.assetbundle");
        //AssetBundleLoadUIRes("download/ui/uires/uiframe.assetbundle");

        AssetBundleLoadUIPrefab("download/ui/uiprefab.assetbundle");

    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.W)) {
            Resources.UnloadUnusedAssets();
        }

        if (Input.GetKeyUp(KeyCode.M)) {
            Instantiate(mbundleUIPrefab.LoadAsset("UITask"));
        }

        if (Input.GetKeyUp(KeyCode.B)) {
            Destroy(GameObject.Find("UITask(Clone)"));
        }

    }

    private byte[] GetBuffer(string path) {
        byte[] buffer = null;

        using (FileStream fs = new FileStream(path, FileMode.Open)) {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
        }
        return buffer;

    }

    private void AssetBundleLoadUIRes(string abPath) {
        string fullPath = mLocalFilePath + abPath;
        byte[] buffer = GetBuffer(fullPath);
        mBundleUIRes = AssetBundle.LoadFromMemory(buffer);

        //var arr = mBundleUIRes.LoadAllAssets();
        //for (int i = 0; i < arr.Length; i++) {
        //    Debug.Log("==>" + arr[i].name);
        //    if (arr[i].name == "Btn_2") {
        //        mObjList.Add(arr[i]);
        //    }
        //}

    }

    private void AssetBundleLoadUIPrefab(string abPath) {
        string fullPath = mLocalFilePath + abPath;
        byte[] buffer = GetBuffer(fullPath);
        mbundleUIPrefab = AssetBundle.LoadFromMemory(buffer);
    }



}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAssetBundle : MonoBehaviour
{
    private List<AssetBundle> assetBundles;

    void Start()
    {
        assetBundles = new List<AssetBundle>();

        string materialPaths = File.ReadAllText(Application.dataPath + "\\materialPaths.txt");
        string[] resPath = materialPaths.Split('\n');

        AssetBundle assetbundle = null;

        for (int i = 0; i < resPath.Length; i++)
        {
            string materialPath = resPath[i];
            if (!File.Exists(materialPath))
                continue;

            assetbundle = AssetBundle.LoadFromFile(materialPath);
            if (assetbundle == null)
            {
                Debug.LogError("assetbundle == null, path = " + materialPath);
                return;
            }

            assetBundles.Add(assetbundle);
            UnityEngine.Object[] assets = assetbundle.LoadAllAssets();
        }


        string path = @"C:\Users\lihehui\Desktop\com.longtugame.yjfb\files\luaframework\AssetBundles\scene_420_mgtx_final.unity.bundle";
        assetbundle = AssetBundle.LoadFromFile(path);
        if (assetbundle == null)
        {
            Debug.LogError("assetbundle == null, path = " + path);
            return;
        }

        assetBundles.Add(assetbundle);
        string[] paths = assetbundle.GetAllScenePaths();
        StartCoroutine(AsyncLoading(paths[0]));
    }

    IEnumerator AsyncLoading(string MapName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(MapName);
        asyncLoad.allowSceneActivation = true;
        yield return asyncLoad;
    }

    private void OnDestroy()
    {
        foreach (AssetBundle assetbundle in assetBundles)
        {
            assetbundle.Unload(true);
        }

        assetBundles.Clear();
    }
}

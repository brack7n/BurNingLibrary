using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;
using System.Text;

/// <summary>
/// radarmap -- 小地图
/// android -- 主角的外观 斗笠 武器
/// sprite -- icon和内观
/// </summary>

[Serializable]
public class QKAnimFrameInfo
{
    public string[] atlas;
    public bool flipX;
    public string[] frames;
    public bool isGetDo;
    public string layer;
    public string layerValue;
    public string sp;
}

[Serializable]
public class QKMapData
{
    public object arg1;
    public object atlas;
}

internal sealed class VersionConfigToNamespaceAssemblyObjectBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type typeToDeserialize = null;
        try
        {
            string ToAssemblyName = assemblyName.Split(',')[0];
            Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly ass in Assemblies)
            {
                if (ass.FullName.Split(',')[0] == ToAssemblyName)
                {
                    typeToDeserialize = ass.GetType(typeName);
                    break;
                }
            }
        }
        catch (System.Exception exception)
        {
            throw exception;
        }
        return typeToDeserialize;
    }
}

public class UnzipAssetbundle
{
    private static string rootPath = Application.dataPath + @"\..\resourcesab\";
    private static string rootOutputResPath = Application.dataPath + @"\..\resources\";
    private static string playerResPath = rootPath + @"android\";

    private static Dictionary<string, object> nameToObjs = new Dictionary<string, object>();
    private static Dictionary<string, Dictionary<string, QKAnimFrameInfo>> animFrames = new Dictionary<string, Dictionary<string, QKAnimFrameInfo>>();
    private static UnityEngine.Object[] spriteObjs;
    private static UnityEngine.Object[] configObjs;
    private static MemoryStream memoryStream;
    private static BinaryFormatter binaryFormatter;
    private static AssetBundle configAssetbundle;
    private static List<AssetBundle> resAssetbundle = new List<AssetBundle>();
    private static bool reset = false;
    private static int maxWidth = 512;
    private static int maxHeight = 512;
    private static List<Color> colors = new List<Color>();

    private static Dictionary<string, int> dirs = new Dictionary<string, int>()
    {
        ["Top"] = 10000,
        ["RightTop"] = 20000,
        ["Right"] = 30000,
        ["RightBttm"] = 40000,
        ["Bttm"] = 50000,
        ["LeftBttm"] = 60000,
        ["Left"] = 70000,
        ["LeftTop"] = 80000,
    };

    private static Dictionary<string, string> eToc = new Dictionary<string, string>()
    {
        ["attack"] = "攻击",
        ["beaten"] = "受击",
        ["die"] = "死亡",
        ["hold"] = "备战",
        ["leftrun"] = "leftrun",
        ["rightrun"] = "rightrun",
        ["run"] = "跑步",
        ["skill"] = "施法",
        ["stand"] = "待机",
        ["walk"] = "走路",
    };

    public static void InitTransparentColor()
    {
        if (colors.Count > 0)
            return;

        for (int i = 0; i < maxWidth; i++)
        {
            for (int j = 0; j < maxHeight; j++)
            {
                Color color = new Color(0, 0, 0, 0);
                colors.Add(color);
            }
        }
    }

    [MenuItem("Tools/冰雪传奇-Body/导出player所有资源")]
    public static void UnzipAllRes()
    {
        Debug.Log(DateTime.Now.ToString());
        UnzipMaleBody();
        UnzipFemaleBody();
        UnzipMaleWeapon();
        UnzipFemalWeapon();
        UnzipMaleDouLi();
        UnzipFemalDouLi();
        Debug.Log(DateTime.Now.ToString());
    }

    [MenuItem("Tools/冰雪传奇-Body/导出男模")]
    public static void UnzipMaleBody()
    {
        string abResPath = playerResPath + @"player\body\male";
        string abJsonPath = playerResPath + @"player\featurejson\bodymale_1096a5181a2e9d9eef38740b809adb02.bytes";
        string outputSpritePath = rootOutputResPath + @"body\sprite\男模";
        string outputTexturePath = rootOutputResPath + @"body\texture\男模";

        Unzip(abJsonPath, abResPath, outputTexturePath, outputSpritePath, "男模");
    }

    [MenuItem("Tools/冰雪传奇-Body/导出女模")]
    public static void UnzipFemaleBody()
    {
        string abResPath = playerResPath + @"player\body\female";
        string abJsonPath = playerResPath + @"player\featurejson\bodyfemale_ff05c355b660ceb3e6d45220768a5c5e.bytes";
        string outputSpritePath = rootOutputResPath + @"body\sprite\女模";
        string outputTexturePath = rootOutputResPath + @"body\texture\女模";

        Unzip(abJsonPath, abResPath, outputTexturePath, outputSpritePath, "女模");
    }

    [MenuItem("Tools/冰雪传奇-Body/导出男模武器")]
    public static void UnzipMaleWeapon()
    {
        string abResPath = playerResPath + @"player\weapon\male";
        string abJsonPath = playerResPath + @"player\featurejson\weaponmale_78c7536fd68bd1e134d305b1e136e032.bytes";
        string outputSpritePath = rootOutputResPath + @"weapon\sprite\男模武器";
        string outputTexturePath = rootOutputResPath + @"weapon\texture\男模武器";

        if (!Directory.Exists(outputSpritePath))
            Directory.CreateDirectory(outputSpritePath);

        if (!Directory.Exists(outputTexturePath))
            Directory.CreateDirectory(outputTexturePath);

        Unzip(abJsonPath, abResPath, outputTexturePath, outputSpritePath, "男模武器");
    }

    [MenuItem("Tools/冰雪传奇-Body/导出女模武器")]
    public static void UnzipFemalWeapon()
    {
        string abResPath = playerResPath + @"player\weapon\female";
        string abJsonPath = playerResPath + @"player\featurejson\weaponfemale_4c4b838ca6a9e99633134b8c2d4e6976.bytes";
        string outputSpritePath = rootOutputResPath + @"weapon\sprite\女模武器";
        string outputTexturePath = rootOutputResPath + @"weapon\texture\女模武器";

        if (!Directory.Exists(outputSpritePath))
            Directory.CreateDirectory(outputSpritePath);

        if (!Directory.Exists(outputTexturePath))
            Directory.CreateDirectory(outputTexturePath);

        Unzip(abJsonPath, abResPath, outputTexturePath, outputSpritePath, "女模武器");
    }

    [MenuItem("Tools/冰雪传奇-Body/导出男模斗笠")]
    public static void UnzipMaleDouLi()
    {
        string abResPath = playerResPath + @"player\douli\male";
        string abJsonPath = playerResPath + @"player\featurejson\doulimale_f25a1a2733e0cfb043a7728a1621c92a.bytes";
        string outputSpritePath = rootOutputResPath + @"douli\sprite\男模斗笠";
        string outputTexturePath = rootOutputResPath + @"douli\texture\男模斗笠";

        if (!Directory.Exists(outputSpritePath))
            Directory.CreateDirectory(outputSpritePath);

        if (!Directory.Exists(outputTexturePath))
            Directory.CreateDirectory(outputTexturePath);

        Unzip(abJsonPath, abResPath, outputTexturePath, outputSpritePath, "男模斗笠", true);
    }

    [MenuItem("Tools/冰雪传奇-Body/导出女模斗笠")]
    public static void UnzipFemalDouLi()
    {
        string abResPath = playerResPath + @"player\douli\female";
        string abJsonPath = playerResPath + @"player\featurejson\doulifemale_7df034bdadc28eda4ca70aebbf52cd48.bytes";
        string outputSpritePath = rootOutputResPath + @"douli\sprite\女模斗笠";
        string outputTexturePath = rootOutputResPath + @"douli\texture\女模斗笠";

        if (!Directory.Exists(outputSpritePath))
            Directory.CreateDirectory(outputSpritePath);

        if (!Directory.Exists(outputTexturePath))
            Directory.CreateDirectory(outputTexturePath);

        Unzip(abJsonPath, abResPath, outputTexturePath, outputSpritePath, "女模斗笠", true);
    }

    [MenuItem("Tools/冰雪传奇-Body/图标资源")]
    public static void UnzipIcon()
    {
        string path = rootPath + @"sprite";
        string outputSpritePath = rootOutputResPath + @"ui\sprite\";
        string outputTexturePath = rootOutputResPath + @"ui\texture\";

        List<string> filePaths = FindFiles(path);
        for (int i = 0; i < filePaths.Count; i++)
        {
            string filePath = filePaths[i];
            UnzipUI(filePath, outputSpritePath, outputTexturePath, "图标");
        }
    }

    [MenuItem("Tools/冰雪传奇-Body/特效资源")]
    public static void UnzipEffect()
    {
        string path = playerResPath + @"player\effect";
        string outputSpritePath = rootOutputResPath + @"effect\sprite\";
        string outputTexturePath = rootOutputResPath + @"effect\texture\";

        List<string> filePaths = FindFiles(path);
        for (int i = 0; i < filePaths.Count; i++)
        {
            string filePath = filePaths[i];
            UnzipUI(filePath, outputSpritePath, outputTexturePath, "特效");
        }
    }

    static Dictionary<string, Texture2D> testSprtes = new Dictionary<string, Texture2D>();

    [MenuItem("Tools/冰雪传奇-资源校验/删除空文件夹")]
    public static void DeleteNullDirectorie()
    {
        string detectPath = rootOutputResPath + @"buff\texture";
        string[] directPaths = Directory.GetDirectories(detectPath);
        for (int i = 0; i < directPaths.Length; i++)
        {
            string directPath = directPaths[i];
            string[] childDirectPaths = Directory.GetDirectories(directPath);
            if (childDirectPaths.Length == 0)
            {
                Debug.LogError("空文件夹: " + directPath);
                Directory.Delete(directPath);
            }
        }
    }

    [MenuItem("Tools/冰雪传奇-资源校验/资源监测")]
    public static void ResDetect()
    {
        List<string> errorPaths = new List<string>();
        List<string> missResAbName = new List<string>();

        string misstext = "";
        string path = rootPath + @"\android\monster\prefabs";
        string detectPath = rootOutputResPath + @"monster\sprite";
        string[] directPaths = Directory.GetDirectories(detectPath);
        List<string> abPaths = FindFiles(path);

        foreach (string abPath in abPaths)
        {
            bool exist = false;
            string[] ids = Path.GetFileNameWithoutExtension(abPath).Split('_');
            foreach (string filePath in directPaths)
            {
                string[] childPaths = filePath.Split('\\');
                string id = childPaths[childPaths.Length - 1];

                if (ids[0].Equals(id))
                {
                    exist = true;
                    break;
                }
            }

            if (!exist)
            {
                misstext += abPath + "\n";
                Debug.LogError("缺失资源: " + abPath);
            }
        }

        File.WriteAllText(Application.dataPath + "\\missAbPath.txt", misstext);
        misstext = "";

        for (int i = 0; i < directPaths.Length; i++)
        {
            string directPath = directPaths[i];
            string[] childDirectPaths = Directory.GetDirectories(directPath);
            foreach (string childDirectPath in childDirectPaths)
            {
                string[] filePaths = Directory.GetFiles(childDirectPath);
                int frame = 0;
                float index = 0;
                foreach (string filePath in filePaths)
                {
                    int dir = int.Parse(Path.GetFileNameWithoutExtension(filePath));
                    if (dir < 20000)
                    {
                        frame++;
                    }

                    EditorUtility.DisplayProgressBar("资源监测", filePath, index / (float)filePaths.Length);
                    index++;
                }

                int allFrame = frame * 8;
                if (allFrame != filePaths.Length)
                {
                    errorPaths.Add(childDirectPath);
                    Debug.LogError("资源文件不匹配: " + childDirectPath);

                    string[] childPaths = childDirectPath.Split('\\');
                    string id = childPaths[childPaths.Length - 2];
                    foreach (string abPath in abPaths)
                    {
                        string[] ids = Path.GetFileNameWithoutExtension(abPath).Split('_');
                        if (ids[0].Equals(id))
                        {
                            misstext += abPath + "\n";
                        }
                    }
                }
            }
        }

        File.WriteAllText(Application.dataPath + "\\miss.txt", misstext);
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("资源修复", "资源问题文件数量: " + errorPaths.Count, "ok");
    }

    [MenuItem("Tools/ABTest")]
    public static void ABTest()
    {
        string dirPath = @"C:\Users\lihehui\Desktop\com.longtugame.yjfb\files\luaframework\lua\x86\lua_config_table.unity3d";
        AssetBundle assetbundle = AssetBundle.LoadFromFile(dirPath);
        if (assetbundle == null)
        {
            Debug.LogError("assetbundle == null, path = " + dirPath);
            return; ;
        }

        UnityEngine.Object[] assets = assetbundle.LoadAllAssets();

        //Debug.LogError(assets.Length);

        //List<string> filePaths = FindFiles(dirPath);
        //AssetBundle assetbundle = null;
        //StringBuilder stringBuilder = new StringBuilder();

        //List<string> materialPaths = new List<string>();

        //float index = 0;
        //foreach (string filePath in filePaths)
        //{
        //    assetbundle = AssetBundle.LoadFromFile(filePath);
        //    if (assetbundle == null)
        //    {
        //        Debug.LogError("assetbundle == null, path = " + filePath);
        //        continue;
        //    }

        //    UnityEngine.Object[] assets = null;

        //    try
        //    {
        //        assets = assetbundle.LoadAllAssets();
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError(e.Message);
        //    }
        //    finally
        //    {
        //        if (assets != null)
        //        {
        //            for (int i = 0; i < assets.Length; i++)
        //            {
        //                Shader material = assets[i] as Shader;
        //                if (material != null)
        //                {
        //                    Debug.Log(filePath);
        //                    stringBuilder.Append(filePath);
        //                    stringBuilder.AppendLine();
        //                }
        //            }
        //        }
        //    }

        //    index++;
        //    assetbundle.Unload(true);
        //    EditorUtility.DisplayProgressBar("资源检测", filePath, index / (float)filePaths.Count);
        //}

        //EditorUtility.ClearProgressBar();
        //File.WriteAllText(Application.dataPath + "\\materialPaths.txt", stringBuilder.ToString());

        //List<byte> bufferTest = new List<byte>();
        //for (int j = 100; j < 1000; j++)
        //{
        //    for (int i = j; i < buffer.Length; i++)
        //    {
        //        bufferTest.Add(buffer[i]);
        //    }

        //    Texture2D t2D = new Texture2D(1024, 1024);
        //    t2D.LoadImage(bufferTest.ToArray());

        //    GameObject test = new GameObject("monsterBody");
        //    SpriteRenderer spriteRendererTest = test.AddComponent<SpriteRenderer>();
        //    Sprite sptest = Sprite.Create(t2D, new Rect(0, 0, t2D.width, t2D.height), new Vector2(0.5f, 0.5f));
        //    spriteRendererTest.sprite = sptest;
        //}

        //GameObject monster = UnityEngine.Object.Instantiate(assets[26]) as GameObject;

        for (int i = 0; i < assets.Length; i++)
        {
            //UnityEngine.Object obj = assets[i];
            //Texture2D texture2D = obj as Texture2D;
            //if (texture2D != null)
            //{
            //    RenderTexture rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height);
            //    rt.filterMode = texture2D.filterMode;
            //    RenderTexture.active = rt;
            //    Graphics.Blit(texture2D, rt);
            //    Texture2D img2 = new Texture2D(texture2D.width, texture2D.height);
            //    img2.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
            //    img2.Apply();
            //    RenderTexture.active = null;
            //    texture2D = img2;

            //    GameObject go = new GameObject("monsterBody");
            //    SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
            //    Sprite sp = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            //    spriteRenderer.sprite = sp;
            //}

            //GameObject monster = UnityEngine.Object.Instantiate(obj) as GameObject;
            //if (monster != null)
            //{

            //}

            TextAsset textAsset = assets[i] as TextAsset;
            //UnityEngine.Object obj = BinaryDeSerialize(textAsset.bytes);
            string info = textAsset.text;

            File.WriteAllBytes(@"G:\UnityProject\UnzipAssetbundle\resources\lua\yjfb\" + assets[i].name, textAsset.bytes);
            break;
        }

        assetbundle.Unload(false);
        //File.WriteAllBytes(@"E:\UserFiles\mapdata.bin", textAsset.bytes);
    }

    public static void UnzipMap(string path, string outputSpritePath, string outputTexturePath, string title)
    {
        if (!Directory.Exists(outputSpritePath))
            Directory.CreateDirectory(outputSpritePath);

        if (!Directory.Exists(outputTexturePath))
            Directory.CreateDirectory(outputTexturePath);

        AssetBundle assetbundle = AssetBundle.LoadFromFile(path);
        if (assetbundle == null)
        {
            Debug.LogError("assetbundle == null, path = " + path);
            return;
        }

        UnityEngine.Object[] assets = assetbundle.LoadAllAssets();
        Dictionary<string, Texture2D> texture2Ds = new Dictionary<string, Texture2D>();

        for (int i = 0; i < assets.Length; i++)
        {
            UnityEngine.Object obj = assets[i];
            Texture2D texture2D = obj as Texture2D;
            if (texture2D)
            {
                string savePath = outputTexturePath + @"\" + obj.name + ".png";
                if (!File.Exists(savePath))
                {
                    RenderTexture rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height);
                    rt.filterMode = texture2D.filterMode;
                    RenderTexture.active = rt;
                    Graphics.Blit(texture2D, rt);
                    Texture2D img2 = new Texture2D(texture2D.width, texture2D.height);
                    img2.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
                    img2.Apply();
                    RenderTexture.active = null;
                    texture2D = img2;

                    SavePng(savePath, texture2D);
                    UnityEngine.Object.DestroyImmediate(texture2D, true);
                    UnityEngine.Object.DestroyImmediate(img2, true);
                }
            }
            else
            {
                Sprite sprite = obj as Sprite;
                if (sprite == null)
                {
                    Debug.LogError("sprite == null");
                    continue;
                }

                //string savePath = outputSpritePath + @"\" + sprite.texture.name;
                //if (!Directory.Exists(savePath))
                //    Directory.CreateDirectory(savePath);

                string savePath = outputSpritePath + @"\" + sprite.name + ".png";
                if (!File.Exists(savePath) || reset)
                {
                    string loadTexturePath = outputTexturePath + @"\" + sprite.texture.name + ".png";
                    Byte[] bytes = File.ReadAllBytes(loadTexturePath);

                    Texture2D t2D;
                    if (!texture2Ds.TryGetValue(sprite.texture.name, out t2D))
                    {
                        t2D = new Texture2D(sprite.texture.width, sprite.texture.height);
                        t2D.LoadImage(bytes);

                        texture2Ds[sprite.texture.name] = t2D;
                    }

                    int x = (int)sprite.rect.x;
                    int y = (int)sprite.rect.y;
                    int maxWidth = (int)sprite.rect.width;
                    int maxHeight = (int)sprite.rect.height;

                    Texture2D image = new Texture2D(maxWidth, maxHeight);
                    image.filterMode = sprite.texture.filterMode;
                    image.wrapMode = sprite.texture.wrapMode;
                    image.SetPixels(t2D.GetPixels(x, y, maxWidth, maxHeight));
                    image.Apply();

                    SavePng(savePath, image);

                    testSprtes.Add(sprite.name, image);
                }

                EditorUtility.DisplayProgressBar("导出UI资源 - " + title, sprite.name, (float)i / (float)assets.Length);
            }
        }

        foreach (var value in texture2Ds)
        {
            UnityEngine.Object.DestroyImmediate(value.Value);
        }

        assetbundle.Unload(false);
        EditorUtility.ClearProgressBar();
    }

    public static void UnzipUI(string path, string outputSpritePath, string outputTexturePath, string title)
    {
        if (!Directory.Exists(outputSpritePath))
            Directory.CreateDirectory(outputSpritePath);

        if (!Directory.Exists(outputTexturePath))
            Directory.CreateDirectory(outputTexturePath);

        AssetBundle assetbundle = AssetBundle.LoadFromFile(path);
        if (assetbundle == null)
        {
            Debug.LogError("assetbundle == null, path = " + path);
            return;
        }

        UnityEngine.Object[] assets = assetbundle.LoadAllAssets();
        Dictionary<string, Texture2D> texture2Ds = new Dictionary<string, Texture2D>();

        for (int i = 0; i < assets.Length; i++)
        {
            UnityEngine.Object obj = assets[i];
            Texture2D texture2D = obj as Texture2D;
            if (texture2D)
            {
                string savePath = outputTexturePath + @"\" + obj.name + ".png";
                if (!File.Exists(savePath))
                {
                    RenderTexture rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height);
                    rt.filterMode = texture2D.filterMode;
                    RenderTexture.active = rt;
                    Graphics.Blit(texture2D, rt);
                    Texture2D img2 = new Texture2D(texture2D.width, texture2D.height);
                    img2.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
                    img2.Apply();
                    RenderTexture.active = null;
                    texture2D = img2;

                    SavePng(savePath, texture2D);
                    UnityEngine.Object.DestroyImmediate(texture2D, true);
                    UnityEngine.Object.DestroyImmediate(img2, true);
                }
            }
            else
            {
                Sprite sprite = obj as Sprite;
                if (sprite == null)
                    continue;

                string savePath = outputSpritePath + @"\" + sprite.texture.name;
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                savePath = savePath + @"\" + sprite.name + ".png";
                if (!File.Exists(savePath) || reset)
                {
                    string loadTexturePath = outputTexturePath + @"\" + sprite.texture.name + ".png";

                    Texture2D t2D;
                    if (!texture2Ds.TryGetValue(sprite.texture.name, out t2D))
                    {
                        Byte[] bytes = File.ReadAllBytes(loadTexturePath);
                        t2D = new Texture2D(sprite.texture.width, sprite.texture.height);
                        t2D.LoadImage(bytes);
                        t2D.Apply();
                    
                        texture2Ds[sprite.texture.name] = t2D;
                    }

                    int x = (int)sprite.rect.x;
                    int y = (int)sprite.rect.y;
                    int maxWidth = (int)sprite.rect.width;
                    int maxHeight = (int)sprite.rect.height;

                    Texture2D image = new Texture2D(maxWidth, maxHeight);
                    image.filterMode = sprite.texture.filterMode;
                    image.wrapMode = sprite.texture.wrapMode;

                    try
                    {
                        image.SetPixels(t2D.GetPixels(x, y, maxWidth, maxHeight));
                    }
                    catch
                    {
                        Debug.LogError(savePath);
                    }
                    
                    image.Apply();

                    SavePng(savePath, image);
                }
               
                EditorUtility.DisplayProgressBar("导出UI资源 - " + title, sprite.name, (float)i / (float)assets.Length);
            }
        }

        foreach (var value in texture2Ds)
        {
            UnityEngine.Object.DestroyImmediate(value.Value);
        }

        texture2Ds.Clear();
        assetbundle.Unload(false);
        EditorUtility.ClearProgressBar();
    }

    public static void Unzip(string abJsonPath, string abResPath, string outputTexturePath, string outputSpritePath, string title, bool douli = false)
    {
        if (configAssetbundle != null)
        {
            configAssetbundle.Unload(true);
            AssetBundle.DestroyImmediate(configAssetbundle, true);
        }

        configAssetbundle = AssetBundle.LoadFromFile(abJsonPath);
        if (configAssetbundle == null)
        {
            Debug.LogError("configAssetbundle == null, path = " + abJsonPath);
            return;
        }

        //解析资源资源名 id对应资源路径
        List<string> filePaths = FindFiles(abResPath);
        Dictionary<string, List<string>> idToPaths = new Dictionary<string, List<string>>();
        for (int i = 0; i < filePaths.Count; i++)
        {
            string filePath = filePaths[i];
            string id = "";
            if (douli)
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                string[] directoryNames = directoryPath.Split('\\');
                id = directoryNames[directoryNames.Length - 1];
            }
            else
            {
                string[] paths = Path.GetFileNameWithoutExtension(filePath).Split('_');
                if (paths.Length < 1)
                {
                    Debug.LogError("格式不对, 不是id_md5格式的");
                    continue;
                }

                id = paths[0];
            }

            List<string> chiuldPaths;
            if (!idToPaths.TryGetValue(id, out chiuldPaths))
            {
                chiuldPaths = new List<string>();
                idToPaths[id] = chiuldPaths;
            }

            chiuldPaths.Add(filePath);
        }

        //读取配置到内存
        configObjs = configAssetbundle.LoadAllAssets();

        for (int i = 0; i < configObjs.Length; i++)
        {
            animFrames.Clear();
            TextAsset textAsset = configObjs[i] as TextAsset;
            if (textAsset == null)
                continue;

            memoryStream = new MemoryStream(textAsset.bytes);
            binaryFormatter = new BinaryFormatter();
            animFrames = (Dictionary<string, Dictionary<string, QKAnimFrameInfo>>) binaryFormatter.Deserialize(memoryStream);

            if (animFrames == null)
            {
                Debug.LogError("obj == null");
                return;
            }

            foreach (var value in animFrames)
            {
                string id = value.Key;
                List<string> childFilePaths;
                if (!idToPaths.TryGetValue(id, out childFilePaths))
                {
                    Debug.LogError("不存在该资源: " + id);
                    continue;
                }

                nameToObjs.Clear();
                resAssetbundle.Clear();
                foreach (var filePath in childFilePaths)
                {
                    AssetBundle resAb = AssetBundle.LoadFromFile(filePath);
                    resAssetbundle.Add(resAb);

                    if (resAb == null)
                    {
                        Debug.LogError("resAssetbundle == null, path = " + filePath);
                        continue;
                    }

                    spriteObjs = resAb.LoadAllAssets();
                    foreach (var obj in spriteObjs)
                    {
                        nameToObjs[obj.name] = obj;
                        Texture2D texture2D = obj as Texture2D;

                        if (texture2D != null)
                        {
                            string savePath = outputTexturePath + @"\" + value.Key;
                            if (!Directory.Exists(savePath))
                            {
                                Debug.LogError(savePath);
                                Directory.CreateDirectory(savePath);
                            }

                            savePath = savePath + @"\" + obj.name + ".png";

                            if (!File.Exists(savePath))
                            {
                                RenderTexture rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height);
                                rt.filterMode = texture2D.filterMode;
                                RenderTexture.active = rt;
                                Graphics.Blit(texture2D, rt);
                                Texture2D img2 = new Texture2D(texture2D.width, texture2D.height);
                                img2.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
                                img2.Apply();
                                RenderTexture.active = null;
                                texture2D = img2;
                                SavePng(savePath, texture2D);

                                UnityEngine.Object.DestroyImmediate(texture2D, true);
                                UnityEngine.Object.DestroyImmediate(img2, true);
                            }
                        }

                        texture2D = null;
                    }
                }

                Dictionary<string, Texture2D> texture2Ds = new Dictionary<string, Texture2D>();

                float index = 0;
                foreach (var animFrame in value.Value)
                {
                    int dirIndex = 0;

                    foreach (var frameName in animFrame.Value.frames)
                    {
                        string[] dirTexts = animFrame.Key.Split('_');
                        if (dirTexts.Length < 1)
                        {
                            Debug.LogError("动作文件不规范, Name: " + value.Key + " -- " + animFrame.Key);
                            continue;
                        }

                        string eTocName = dirTexts[0];
                        if (eToc.ContainsKey(dirTexts[0]))
                        {
                            eTocName = eToc[dirTexts[0]];
                        }

                        string outputPath = outputSpritePath + @"\" + value.Key + @"\" + eTocName;
                        int dir = dirs[dirTexts[1]];
                        string dirName = (dir + dirIndex).ToString();

                        if (!Directory.Exists(outputPath))
                        {
                            Directory.CreateDirectory(outputPath);
                        }

                        outputPath = outputPath + @"\" + dirName + ".png";
                        if (!File.Exists(outputPath) || reset)
                        {
                            Sprite sprite = nameToObjs[frameName] as Sprite;

                            if (sprite == null)
                            {
                                Debug.LogError("sprite == null, name: " + frameName);
                                continue;
                            }

                            Texture2D texture2D = sprite.texture;
                            if (texture2D == null)
                            {
                                Debug.LogError("texture2D == null, name: " + frameName);
                                continue;
                            }

                            string loadTexturePath = outputTexturePath + @"\" + value.Key + @"\" + texture2D.name + ".png";
                            Byte[] bytes = File.ReadAllBytes(loadTexturePath);

                            Texture2D t2D;
                            string textureKey = value.Key + texture2D.name;

                            if (!texture2Ds.TryGetValue(textureKey, out t2D))
                            {
                                t2D = new Texture2D(texture2D.width, texture2D.height);
                                t2D.LoadImage(bytes);

                                texture2Ds[textureKey] = t2D;
                            }

                            int x = (int)sprite.rect.x;
                            int y = (int)sprite.rect.y;
                            int width = (int)sprite.rect.width;
                            int height = (int)sprite.rect.height;

                            int pivotx = (int)sprite.pivot.x;
                            int pivoty = (int)sprite.pivot.y;

                            int offsetX = 0;
                            int offsetY = -32;
                            int pivotOffsetY = pivoty; //中心在中心点 像素整体向下偏移

                            Vector2 center = new Vector2(maxWidth / 2, maxHeight / 2);

                            int startX = (int)center.x - pivotx - offsetX;
                            int StartY = (int)center.y - pivoty - offsetY;

                            Texture2D image = new Texture2D(maxWidth, maxHeight);
                            image.filterMode = sprite.texture.filterMode;
                            image.wrapMode = sprite.texture.wrapMode;

                            InitTransparentColor();
                            //图片设置成透明像素
                            image.SetPixels(colors.ToArray());

                            //像素根据锚点偏移-并且将像素设置到中心
                            for (int colori = startX; colori < maxWidth; colori++)
                            {
                                for (int colorj = StartY; colorj < maxHeight; colorj++)
                                {
                                    Color color = new Color(0, 0, 0, 0);
                                    if (colori - startX <= width && colorj - StartY <= height)
                                    {
                                        color = t2D.GetPixel(colori - startX + x, colorj - StartY + y);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    image.SetPixel((colori), (colorj), color);
                                }
                            }

                            image.Apply();
                            SavePng(outputPath, image);

                            UnityEngine.Object.DestroyImmediate(image, true);
                        }

                        index++;
                        string info = value.Key + "  " + animFrame.Key + " " + frameName;
                        EditorUtility.DisplayProgressBar("导出模型资源 - " + title, info, index / (float)(value.Value.Count * animFrame.Value.frames.Length));
                        dirIndex++;
                    }
                }

                foreach (var texture in texture2Ds)
                {
                    if (texture.Value != null)
                        UnityEngine.Object.DestroyImmediate(texture.Value, true);
                }

                foreach (var ab in resAssetbundle)
                {
                    if (ab == null)
                        continue;

                    ab.Unload(true);
                    AssetBundle.DestroyImmediate(ab, true);
                }

               
                EditorUtility.ClearProgressBar();
            }

            memoryStream.Close();
            memoryStream.Dispose();
        }

        configAssetbundle.Unload(true);
        configObjs = null;
        Debug.Log("ok");
        EditorUtility.ClearProgressBar();
    }

    public static void SavePng(string path, Texture2D tex)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (tex == null)
            return;

        FileStream fileStream = null;
        BinaryWriter binaryWriter = null;
        byte[] bytes = null;

        try
        {
            bytes = tex.EncodeToPNG();
            fileStream = File.Open(path, FileMode.Create);
            binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(bytes);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream.Dispose();
            }

            if (binaryWriter != null)
            {
                binaryWriter.Close();
                binaryWriter.Dispose();
            }

            bytes = null;
        }
    }

    public static UnityEngine.Object BinaryDeSerialize(byte[] bytes)
    {
        MemoryStream stream = new MemoryStream(bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        UnityEngine.Object obj = (UnityEngine.Object)formatter.Deserialize(stream);
        return obj;
    }

    private static List<string> FindFiles(string path)
    {
        List<string> filePaths = new List<string>();
        if (Directory.Exists(path))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                filePaths.Add(files[i].FullName);
            }
        }

        return filePaths;
    }
}

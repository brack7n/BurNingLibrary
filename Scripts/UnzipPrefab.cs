using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UnzipPrefab : MonoBehaviour
{
    private Dictionary<string, int> dirs = new Dictionary<string, int>()
    {
        ["00000"] = 10000,
        ["10000"] = 20000,
        ["20000"] = 30000,
        ["30000"] = 40000,
        ["40000"] = 50000,
        ["50000"] = 60000,
        ["60000"] = 70000,
        ["70000"] = 80000,
    };

    private Dictionary<string, string> eToc = new Dictionary<string, string>()
    {
        ["attack"] = "攻击",
        ["attack1"] = "攻击1",
        ["attack2"] = "攻击2",
        ["attack3"] = "攻击3",
        ["beaten"] = "受击",
        ["die"] = "死亡",
        ["hold"] = "备战",
        ["leftrun"] = "leftrun",
        ["rightrun"] = "rightrun",
        ["run"] = "跑步",
        ["skill"] = "施法",
        ["skill1"] = "施法1",
        ["skill2"] = "施法2",
        ["skill3"] = "施法3",
        ["stand"] = "待机",
        ["walk"] = "走路",
    };

    public Text text;
    public Material material;
    private bool reset = false;
    private int maxWidth = 1024;
    private int maxHeight = 1024;
    private List<Color> colors;

    private string rootPath;
    private string path;
    private string rootOutputResPath;
    private string outputSpritePath;
    private string outputTexturePath;
    private bool unZip = false;

    public Animator testAnimator;

    private void Awake()
    {
        rootPath = Application.dataPath + @"\..\resourcesab\";
        path = rootPath + @"\android\monster\prefabs";
        rootOutputResPath = Application.dataPath + @"\..\resources\";
        outputSpritePath = rootOutputResPath + @"monster\sprite\";
        outputTexturePath = rootOutputResPath + @"monster\texture\";

        colors = new List<Color>();
        for (int i = 0; i < maxWidth; i++)
        {
            for (int j = 0; j < maxHeight; j++)
            {
                Color color = new Color(0, 0, 0, 0);
                colors.Add(color);
            }
        }
    }

    private void Start()
    {
        Time.timeScale = 1;
    }

    public void Update()
    {
    }

    public void AgainUnzip(bool ison)
    {
        reset = ison;
    }

    public void UnzipMiss()
    {
        if (unZip)
            return;
        
        string missText = File.ReadAllText(Application.dataPath + "\\missAbPath.txt");
        if (missText == null)
            return;

        unZip = true;
        string[] resPath = missText.Split('\n');
        List<string> filePaths = new List<string>(resPath);
        StartCoroutine(LoadFile(filePaths, outputSpritePath, outputTexturePath));
    }

    public void UnzipMissAbPaths()
    {
        if (unZip)
            return;

        string missText = File.ReadAllText(Application.dataPath + "\\missAbPath.txt");
        if (missText == null)
            return;

        unZip = true;
        string[] resPath = missText.Split('\n');
        List<string> filePaths = new List<string>(resPath);
        StartCoroutine(InstanceMonster(filePaths, outputSpritePath, outputTexturePath));
    }

    public void UnzipAllPrefab()
    {
        if (unZip)
            return;

        unZip = true;
        text.gameObject.SetActive(true);
        List<string> filePaths = new List<string> { @"D:\UnityProject\UnzipAssetbundle\resourcesab\android\monster\prefabs\11415_3fc5339ea19b9b0cd78fcd1486239b58.prefab" };
        filePaths = FindFiles(path);
        StartCoroutine(LoadFile(filePaths, outputSpritePath, outputTexturePath));
    }

    public void UnzipMagicPrefab()
    {
        if (unZip)
            return;

        unZip = true;
        text.gameObject.SetActive(true);
        List<string> filePaths = new List<string> { @"G:\UnityProject\UnzipAssetbundle\resourcesab\prefabs\magic\animationprefab\1001_70a3e34627e9fa8ec53c672aca1454fd.prefab" };
        //filePaths = FindFiles(rootPath + @"\prefabs\magic\");

        string outSpritePath = rootOutputResPath + @"magic\sprite\";
        string outTexturePath = rootOutputResPath + @"magic\texture\";
        StartCoroutine(LoadMagicFile(filePaths, outSpritePath, outTexturePath));
    }

    public void UnzipNeiguan()
    {
        if (unZip)
            return;

        unZip = true;
        text.gameObject.SetActive(true);
        List<string> filePaths = new List<string> { @"G:\UnityProject\UnzipAssetbundle\resourcesab\animationclip\equip\weapon_tou_15011_d36692a9a26664325cb9be57a4d035da.controller" };
        filePaths = FindFiles(rootPath + @"\animationclip\equip\");

        string outSpritePath = rootOutputResPath + @"Neiguan\sprite\";
        string outTexturePath = rootOutputResPath + @"Neiguan\texture\";
        StartCoroutine(LoadNeiGuanFile(filePaths, outSpritePath, outTexturePath));
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

    private IEnumerator InstanceMonster(List<string> filePaths, string spitePath, string texturePath)
    {
        int resIndex = 0;
        foreach (string filePath in filePaths)
        {
            resIndex++;
            if (String.IsNullOrEmpty(filePath))
                continue;

            text.text = string.Format("加载Assetbundle: {0}", Path.GetFileName(filePath));
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string[] paths = fileName.Split('_');
            string id = paths[0];

            string outPutSpitePath = spitePath + id + @"\";
            string outPutTexturePath = texturePath + id + @"\";

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(filePath);
            if (request == null)
            {
                Debug.LogError("request == null, path = " + filePath);
                yield break;
            }

            AssetBundle assetBundle = request.assetBundle;
            if (assetBundle == null)
                yield break;

            UnityEngine.Object[] objects = assetBundle.LoadAllAssets();
            GameObject monster = UnityEngine.Object.Instantiate(objects[0]) as GameObject;
            Animator animator = monster.transform.Find("offset/partner/body").GetComponent<Animator>();
            SpriteRenderer spriteRenderer = monster.transform.Find("offset/partner/body").GetComponent<SpriteRenderer>();
            AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
            monster.transform.position = new Vector3(-5 + resIndex * 1.5f, 0, 0);

            if (animator == null)
            {
                Debug.LogError("animator == null, name: " + monster.name);
                assetBundle.Unload(true);
                yield break;
            }

            if (spriteRenderer == null)
            {
                Debug.LogError("spriteRenderer == null, name: " + monster.name);
                assetBundle.Unload(true);
                yield break;
            }

            foreach (var ani in animationClips)
            {
                animator.Play(ani.name);
            }

            spriteRenderer.material = material;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator LoadNeiGuanFile(List<string> filePaths, string spitePath, string texturePath)
    {
        Debug.Log("开始时间: " + DateTime.Now.ToString());
        int resIndex = 0;
        int allSpriteCount = 0;
        foreach (string filePath in filePaths)
        {
            resIndex++;
            if (String.IsNullOrEmpty(filePath))
                continue;

            text.text = string.Format("加载Assetbundle: {0}", Path.GetFileName(filePath));
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string[] paths = fileName.Split('_');
            string id = paths[0] + @"\" + paths[1] + @"\" + paths[2];

            string outPutSpitePath = spitePath + id + @"\";
            string outPutTexturePath = texturePath + id + @"\";

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(filePath);
            if (request == null)
            {
                Debug.LogError("request == null, path = " + filePath);
                yield break;
            }

            AssetBundle assetBundle = request.assetBundle;
            if (assetBundle == null)
                yield break;

            UnityEngine.Object[] objects = assetBundle.LoadAllAssets();
            RuntimeAnimatorController runtimeAnimatorController = objects[0] as RuntimeAnimatorController;
            testAnimator.runtimeAnimatorController = runtimeAnimatorController;
            Dictionary<string, Dictionary<string, Sprite>> spriteDic = new Dictionary<string, Dictionary<string, Sprite>>();
            Image douli = testAnimator.GetComponent<Image>();
            
            int index = 0;

            if (!Directory.Exists(outPutSpitePath))
            {
                Debug.LogError("新资源路径: " + outPutSpitePath);
                Directory.CreateDirectory(outPutSpitePath);
            }

            if (!Directory.Exists(outPutTexturePath))
            {
                Directory.CreateDirectory(outPutTexturePath);
            }

            index = 0;
            if (runtimeAnimatorController == null)
            {
                Debug.LogError("animator = null, path" + filePath);
                continue;
            }

            AnimationClip[] animationClips = runtimeAnimatorController.animationClips;
            while (index < animationClips.Length)
            {
                text.text = string.Format("贴图采样: {0}/{1}, 资源名: {2}, 动画数量: {3}/{4}", resIndex, filePaths.Count, Path.GetFileName(filePath), index, animationClips.Length);

                string animatorName = animationClips[index].name;
                if (animatorName.Equals("null"))
                {
                    index++;
                    continue;
                }

                AnimationClip animationClip = animationClips[index];
                testAnimator.Play(animatorName);

                Dictionary<string, Sprite> sprites;
                if (!spriteDic.TryGetValue(animatorName, out sprites))
                {
                    sprites = new Dictionary<string, Sprite>();
                    spriteDic.Add(animatorName, sprites);
                }

                float sampTime = 2;
                while (sampTime > 0)
                {
                    if (douli.sprite != null)
                    {
                        string spriteName = douli.sprite.name;
                        Sprite sprite;
                        if (!sprites.TryGetValue(spriteName, out sprite))
                        {
                            sprite = douli.sprite;
                            sprites.Add(spriteName, sprite);
                            Debug.Log(spriteName);
                        }
                    }

                    sampTime -= 0.01f;
                    yield return new WaitForSeconds(0.01f);
                }

                index++;
                yield return new WaitForSeconds(0.1f);
            }

            int spriteIndex = 1;
            int spriteCount = 0;
            foreach (var value in spriteDic)
            {
                spriteCount += value.Value.Count;
            }

            allSpriteCount = allSpriteCount + spriteCount;
            Dictionary<string, Texture2D> texture2Ds = new Dictionary<string, Texture2D>();
            foreach (var sprites in spriteDic)
            {
                int dirIndex = 0;
                foreach (var value in sprites.Value)
                {
                    Sprite sprite = value.Value;
                    Texture2D texture2D = sprite.texture;
                    string textureSavePath = outPutTexturePath + texture2D.name + ".png";
                    if (!File.Exists(textureSavePath))
                    {
                        RenderTexture rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height);
                        rt.filterMode = texture2D.filterMode;
                        RenderTexture.active = rt;
                        Graphics.Blit(texture2D, rt);
                        Texture2D img2 = new Texture2D(texture2D.width, texture2D.height);
                        img2.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
                        img2.Apply();
                        RenderTexture.active = null;

                        SavePng(textureSavePath, img2);
                        Destroy(img2);
                    }

                    string[] dirTexts = sprite.name.Split('_');

                    string spriteSavePath = outPutSpitePath;
                    if (!Directory.Exists(spriteSavePath))
                        Directory.CreateDirectory(spriteSavePath);

                    int dir = 10000;
                    if (dirTexts.Length > 3)
                    {
                        dir = int.Parse(dirTexts[3]) + 9999;
                    }

                    string dirName = (dir + dirIndex).ToString();
                    spriteSavePath = spriteSavePath + @"\" + dirName + ".png";

                    if (File.Exists(spriteSavePath) && !reset)
                        continue;

                    Texture2D t2D;
                    if (!texture2Ds.TryGetValue(texture2D.name, out t2D))
                    {
                        try
                        {
                            Byte[] bytes = File.ReadAllBytes(textureSavePath);
                            t2D = new Texture2D(texture2D.width, texture2D.height);
                            t2D.LoadImage(bytes);
                            t2D.Apply();

                            texture2Ds[texture2D.name] = t2D;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }

                    int x = (int)sprite.rect.x;
                    int y = (int)sprite.rect.y;
                    int width = (int)sprite.rect.width;
                    int height = (int)sprite.rect.height;

                    int pivotx = (int)sprite.pivot.x;
                    int pivoty = (int)sprite.pivot.y;

                    Vector2 center = new Vector2(maxWidth / 2, maxHeight / 2);

                    int startX = (int)center.x - pivotx;
                    int StartY = (int)center.y - pivoty;

                    Texture2D image = new Texture2D(maxWidth, maxHeight);
                    image.filterMode = sprite.texture.filterMode;
                    image.wrapMode = sprite.texture.wrapMode;

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

                    string assetbundleInfo = string.Format("Assetbundle: ({0}/{1})", resIndex, filePaths.Count);
                    string frameInfo = string.Format(" 帧数: ({0}/{1})", spriteIndex, spriteCount);
                    string actionInfo = string.Format(" 动作id: {0}/{1}", sprites.Key, dirName);
                    string textureSizeInfo = string.Format(" \n贴图大小: {0}/{1}", maxWidth, maxHeight);
                    string pixelSizeInfo = string.Format(" \n图像大小: {0}/{1}", width, height);

                    text.text = "正在导出资源: " + assetbundleInfo + frameInfo + actionInfo + textureSizeInfo + pixelSizeInfo;

                    Debug.LogError("新资源：" + spriteSavePath);

                    image.Apply();
                    SavePng(spriteSavePath, image);
                    DestroyImmediate(image, true);
                    spriteIndex++;
                    yield return new WaitForSeconds(0.01f);
                }
            }

            foreach (KeyValuePair<string, Texture2D> value in texture2Ds)
            {
                if (value.Value != null)
                    DestroyImmediate(value.Value, true);
            }

            assetBundle.Unload(true);
            texture2Ds.Clear();
            spriteDic.Clear();
            Debug.Log(filePath + " 导出完毕");
            yield return new WaitForSeconds(0.2f);
        }

        unZip = false;
        Debug.Log("全部资源导出完毕: " + string.Format("({0}/{1}), 共导出: {2} 张序列帧, 结束时间: {3}", resIndex, filePaths.Count, allSpriteCount, DateTime.Now.ToString()));
        text.text = "全部资源导出完毕: " + string.Format("({0}/{1}), 共导出: {2} 张序列帧", resIndex, filePaths.Count, allSpriteCount);
    }

    private IEnumerator LoadMagicFile(List<string> filePaths, string spitePath, string texturePath)
    {
        Debug.Log("开始时间: " + DateTime.Now.ToString());
        int resIndex = 0;
        int allSpriteCount = 0;
        foreach (string filePath in filePaths)
        {
            resIndex++;
            if (String.IsNullOrEmpty(filePath))
                continue;

            text.text = string.Format("加载Assetbundle: {0}", Path.GetFileName(filePath));
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string[] paths = fileName.Split('_');
            string id = paths[0];

            string outPutSpitePath = spitePath + id + @"\";
            string outPutTexturePath = texturePath + id + @"\";

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(filePath);
            if (request == null)
            {
                Debug.LogError("request == null, path = " + filePath);
                yield break;
            }

            AssetBundle assetBundle = request.assetBundle;
            if (assetBundle == null)
                yield break;

            UnityEngine.Object[] objects = assetBundle.LoadAllAssets();
            GameObject monster = UnityEngine.Object.Instantiate(objects[0]) as GameObject;
            SpriteRenderer[] spriteRenderers = monster.transform.GetComponentsInChildren<SpriteRenderer>();
            Animator[] animators = monster.transform.GetComponentsInChildren<Animator>();
            Dictionary<string, Dictionary<string, Sprite>> spriteDic = new Dictionary<string, Dictionary<string, Sprite>>();

            int index = 0;

            if (!Directory.Exists(outPutSpitePath))
            {
                Debug.LogError("新资源路径: " + outPutSpitePath);
                Directory.CreateDirectory(outPutSpitePath);
            }

            if (!Directory.Exists(outPutTexturePath))
            {
                Directory.CreateDirectory(outPutTexturePath);
            }

            foreach (Animator animator in animators)
            {
                index = 0;
                if (animator == null || animator.runtimeAnimatorController == null)
                {
                    Debug.LogError("animator = null, path" + filePath);
                    continue;
                }

                AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
                while (index < animationClips.Length)
                {
                    text.text = string.Format("贴图采样: {0}/{1}, 资源名: {2}, 动画数量: {3}/{4}", resIndex, filePaths.Count, Path.GetFileName(filePath), index, animationClips.Length);

                    string animatorName = animationClips[index].name;
                    if (animatorName.Equals("null"))
                    {
                        index++;
                        continue;
                    }                        

                    AnimationClip animationClip = animationClips[index];
                    animator.Play(animatorName);

                    Dictionary<string, Sprite> sprites;
                    if (!spriteDic.TryGetValue(animatorName, out sprites))
                    {
                        sprites = new Dictionary<string, Sprite>();
                        spriteDic.Add(animatorName, sprites);
                    }

                    float sampTime = 8;
                    while (sampTime > 0)
                    {
                        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                        {
                            spriteRenderer.flipX = false;
                            if (spriteRenderer.sprite != null)
                            {
                                string spriteName = spriteRenderer.sprite.name;
                                Sprite sprite;
                                if (!sprites.TryGetValue(spriteName, out sprite))
                                {
                                    sprite = spriteRenderer.sprite;
                                    sprites.Add(spriteName, sprite);
                                    Debug.Log(spriteName);
                                }
                            }

                            sampTime -= 0.01f;
                            yield return new WaitForSeconds(0.01f);
                        }
                    }

                    index++;
                    yield return new WaitForSeconds(0.1f);
                }
            }

            int spriteIndex = 1;
            int spriteCount = 0;
            foreach (var value in spriteDic)
            {
                spriteCount += value.Value.Count;
            }

            allSpriteCount = allSpriteCount + spriteCount;
            Dictionary<string, Texture2D> texture2Ds = new Dictionary<string, Texture2D>();
            foreach (var sprites in spriteDic)
            {
                int dirIndex = 0;
                foreach (var value in sprites.Value)
                {
                    Sprite sprite = value.Value;
                    Texture2D texture2D = sprite.texture;
                    string textureSavePath = outPutTexturePath + texture2D.name + ".png";
                    if (!File.Exists(textureSavePath))
                    {
                        RenderTexture rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height);
                        rt.filterMode = texture2D.filterMode;
                        RenderTexture.active = rt;
                        Graphics.Blit(texture2D, rt);
                        Texture2D img2 = new Texture2D(texture2D.width, texture2D.height);
                        img2.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
                        img2.Apply();
                        RenderTexture.active = null;

                        SavePng(textureSavePath, img2);
                        Destroy(img2);
                    }

                    string[] dirTexts = sprite.name.Split('_');
                    string eTocName = dirTexts[0];
                    if (eToc.ContainsKey(dirTexts[0]))
                        eTocName = eToc[dirTexts[0]];

                    string spriteSavePath = outPutSpitePath + eTocName;
                    if (!Directory.Exists(spriteSavePath))
                        Directory.CreateDirectory(spriteSavePath);

                    int dir = int.Parse(dirTexts[1]) /*+ 9999*/;
                    string dirName = (dir + dirIndex).ToString();
                    spriteSavePath = spriteSavePath + @"\" + dirName + ".png";

                    if (File.Exists(spriteSavePath) && !reset)
                        continue;

                    Texture2D t2D;
                    if (!texture2Ds.TryGetValue(texture2D.name, out t2D))
                    {
                        try
                        {
                            Byte[] bytes = File.ReadAllBytes(textureSavePath);
                            t2D = new Texture2D(texture2D.width, texture2D.height);
                            t2D.LoadImage(bytes);

                            texture2Ds[texture2D.name] = t2D;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                    }

                    int x = (int)sprite.rect.x;
                    int y = (int)sprite.rect.y;
                    int width = (int)sprite.rect.width;
                    int height = (int)sprite.rect.height;

                    int pivotx = (int)sprite.pivot.x;
                    int pivoty = (int)sprite.pivot.y;

                    Vector2 center = new Vector2(maxWidth / 2, maxHeight / 2);

                    int startX = (int)center.x - pivotx;
                    int StartY = (int)center.y - pivoty;

                    Texture2D image = new Texture2D(maxWidth, maxHeight);
                    image.filterMode = sprite.texture.filterMode;
                    image.wrapMode = sprite.texture.wrapMode;

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

                    string assetbundleInfo = string.Format("Assetbundle: ({0}/{1})", resIndex, filePaths.Count);
                    string frameInfo = string.Format(" 帧数: ({0}/{1})", spriteIndex, spriteCount);
                    string directoryInfo = string.Format(" 写入到文件夹: {0}/{1}", id, eTocName);
                    string actionInfo = string.Format(" 动作id: {0}/{1}", sprites.Key, dirName);
                    string textureSizeInfo = string.Format(" \n贴图大小: {0}/{1}", maxWidth, maxHeight);
                    string pixelSizeInfo = string.Format(" \n图像大小: {0}/{1}", width, height);

                    text.text = "正在导出资源: " + assetbundleInfo + frameInfo + directoryInfo + actionInfo + textureSizeInfo + pixelSizeInfo;

                    image.Apply();
                    SavePng(spriteSavePath, image);
                    DestroyImmediate(image, true);
                    spriteIndex++;
                    yield return new WaitForSeconds(0.01f);
                }
            }

            foreach (KeyValuePair<string, Texture2D> value in texture2Ds)
            {
                if (value.Value != null)
                    DestroyImmediate(value.Value, true);
            }

            Destroy(monster);
            assetBundle.Unload(true);
            texture2Ds.Clear();
            spriteDic.Clear();
            Debug.Log(filePath + " 导出完毕");
            yield return new WaitForSeconds(0.2f);
        }

        unZip = false;
        Debug.Log("全部资源导出完毕: " + string.Format("({0}/{1}), 共导出: {2} 张序列帧, 结束时间: {3}", resIndex, filePaths.Count, allSpriteCount, DateTime.Now.ToString()));
        text.text = "全部资源导出完毕: " + string.Format("({0}/{1}), 共导出: {2} 张序列帧", resIndex, filePaths.Count, allSpriteCount);
    }

    private IEnumerator LoadFile(List<string> filePaths, string spitePath, string texturePath)
    {
        Debug.Log("开始时间: " + DateTime.Now.ToString());       
        int resIndex = 0;
        int allSpriteCount = 0;
        foreach (string filePath in filePaths)
        {
            resIndex++;
            if (String.IsNullOrEmpty(filePath))
                continue;

            text.text = string.Format("加载Assetbundle: {0}", Path.GetFileName(filePath));
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string[] paths = fileName.Split('_');
            string id = paths[0];

            string outPutSpitePath = spitePath + id + @"\";
            string outPutTexturePath = texturePath + id + @"\";

            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(filePath);
            if (request == null)
            {
                Debug.LogError("request == null, path = " + filePath);
                yield break;
            }

            AssetBundle assetBundle = request.assetBundle;
            if (assetBundle == null)
                yield break;

            UnityEngine.Object[] objects = assetBundle.LoadAllAssets();
            GameObject monster = UnityEngine.Object.Instantiate(objects[0]) as GameObject;
            Animator animator = monster.transform.Find("offset/partner/body").GetComponent<Animator>();
            SpriteRenderer spriteRenderer = monster.transform.Find("offset/partner/body").GetComponent<SpriteRenderer>();

            if (animator == null)
            {
                Debug.LogError("animator == null, name: " + monster.name);
                assetBundle.Unload(true);
                yield break;
            }

            if (spriteRenderer == null)
            {
                Debug.LogError("spriteRenderer == null, name: " + monster.name);
                assetBundle.Unload(true);
                yield break;
            }

            spriteRenderer.material = material;
            Dictionary<string, Dictionary<string, Sprite>> spriteDic = new Dictionary<string, Dictionary<string, Sprite>>();

            int index = 0;
            string endName = "";
            AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = animatorOverrideController;

            //if (!animationClips[0].name.Equals("stand_Top"))
            //{
            //    Debug.LogError(string.Format("动画序列不符合导出规范, 资源名: {0}, 动画名: {1}", Path.GetFileName(filePath), animationClips[0].name));
            //    Destroy(monster);
            //    assetBundle.Unload(true);       
            //    continue;
            //}

            if (!Directory.Exists(outPutSpitePath))
            {
                Debug.LogError("新资源路径: " + outPutSpitePath);
                Directory.CreateDirectory(outPutSpitePath);
            }

            if (!Directory.Exists(outPutTexturePath))
            {
                Directory.CreateDirectory(outPutTexturePath);
            }

            while (index < animationClips.Length)
            {
                text.text = string.Format("贴图采样: {0}/{1}, 资源名: {2}, 动画数量: {3}/{4}", resIndex, filePaths.Count, Path.GetFileName(filePath), index, animationClips.Length);

                string animatorName = animationClips[index].name;
                AnimationClip animationClip = animationClips[index];
                if (animationClips.Length < 5)
                {
                    //特殊的序列帧 只有三个动画 像触龙神那种
                    animator.Play(animatorName);
                }
                else
                {
                    if (animationClip.isLooping)
                    {
                        for (int j = 0; j < animationClips.Length; j++)
                        {
                            string clipName = animationClips[j].name.Split('_')[0];
                            if (clipName == "stand")
                            {
                                animatorOverrideController[animationClips[j].name] = animationClips[index];
                            }
                        }
                    }
                    else
                    {
                        animator.Play(animatorName);
                    }
                }

                Dictionary<string, Sprite> sprites;
                if (!spriteDic.TryGetValue(animatorName, out sprites))
                {
                    sprites = new Dictionary<string, Sprite>();
                    spriteDic.Add(animatorName, sprites);
                }

                float sampTime = 3;
                while (sampTime > 0)
                {
                    spriteRenderer.flipX = false;
                    if (spriteRenderer.sprite != null)
                    {
                        string spriteName = spriteRenderer.sprite.name;
                        if (animatorName.Split('_')[0].Equals(spriteName.Split('_')[0]) && endName != spriteName)
                        {
                            Sprite sprite;
                            if (!sprites.TryGetValue(spriteName, out sprite))
                            {
                                sprite = spriteRenderer.sprite;
                                sprites.Add(spriteName, sprite);
                                Debug.Log(spriteName);
                            }
                        }
                    }

                    sampTime -= 0.02f;
                    yield return new WaitForSeconds(0.02f);
                }

                //容错判断
                int upDir = 100;
                foreach (var value in sprites)
                {
                    string strDir = value.Key.Substring(value.Key.Length - 2, 2);
                    int dir = int.Parse(strDir);
                    if (dir < upDir)
                        upDir = dir;
                }

                if (upDir != 1)
                {
                    spriteDic.Clear();
                    Debug.LogError(string.Format("动作计算错误: 资源: {0}, 动作: {1}, Dir: {2}", filePath, animatorName, upDir));
                    break;
                }

                if (spriteRenderer.sprite != null)
                {
                    endName = spriteRenderer.sprite.name;
                }
                
                index++;
                yield return new WaitForSeconds(0.1f);
            }

            int spriteIndex = 1;
            int spriteCount = 0;
            foreach (var value in spriteDic)
            {
                spriteCount += value.Value.Count;
            }

            allSpriteCount = allSpriteCount + spriteCount;
            Dictionary<string, Texture2D> texture2Ds = new Dictionary<string, Texture2D>();
            foreach (var sprites in spriteDic)
            {
                int dirIndex = 0;  
                foreach (var value in sprites.Value)
                {
                    Sprite sprite = value.Value;
                    Texture2D texture2D = sprite.texture;
                    string textureSavePath = outPutTexturePath + texture2D.name + ".png";
                    if (!File.Exists(textureSavePath))
                    {
                        RenderTexture rt = RenderTexture.GetTemporary(texture2D.width, texture2D.height);
                        rt.filterMode = texture2D.filterMode;
                        RenderTexture.active = rt;
                        Graphics.Blit(texture2D, rt);
                        Texture2D img2 = new Texture2D(texture2D.width, texture2D.height);
                        img2.ReadPixels(new Rect(0, 0, texture2D.width, texture2D.height), 0, 0);
                        img2.Apply();
                        RenderTexture.active = null;

                        SavePng(textureSavePath, img2);
                        Destroy(img2);
                    }

                    string[] dirTexts = sprite.name.Split('_');
                    string eTocName = dirTexts[0];
                    if (eToc.ContainsKey(dirTexts[0]))
                        eTocName = eToc[dirTexts[0]];

                    string spriteSavePath = outPutSpitePath + eTocName;
                    if (!Directory.Exists(spriteSavePath))
                        Directory.CreateDirectory(spriteSavePath);

                    int dir = int.Parse(dirTexts[1]) + 9999;
                    string dirName = (dir + dirIndex).ToString();

                    spriteSavePath = spriteSavePath + @"\" + dirName + ".png";

                    if (File.Exists(spriteSavePath) && !reset)
                        continue;

                    Texture2D t2D;
                    if (!texture2Ds.TryGetValue(texture2D.name, out t2D))
                    {
                        try
                        {
                            Byte[] bytes = File.ReadAllBytes(textureSavePath);
                            t2D = new Texture2D(texture2D.width, texture2D.height);
                            t2D.LoadImage(bytes);

                            texture2Ds[texture2D.name] = t2D;
                        }
                        catch(Exception e)
                        { 
                            Debug.LogError(e.Message);
                        }
                    }
                   
                    int x = (int)sprite.rect.x;
                    int y = (int)sprite.rect.y;
                    int width = (int)sprite.rect.width;
                    int height = (int)sprite.rect.height;

                    int pivotx = (int)sprite.pivot.x;
                    int pivoty = (int)sprite.pivot.y;

                    Vector2 center = new Vector2(maxWidth / 2, maxHeight / 2);

                    int startX = (int)center.x - pivotx;
                    int StartY = (int)center.y - pivoty;

                    Texture2D image = new Texture2D(maxWidth, maxHeight);
                    image.filterMode = sprite.texture.filterMode;
                    image.wrapMode = sprite.texture.wrapMode;

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

                    string assetbundleInfo = string.Format("Assetbundle: ({0}/{1})", resIndex, filePaths.Count);
                    string frameInfo = string.Format(" 帧数: ({0}/{1})", spriteIndex, spriteCount);
                    string directoryInfo = string.Format(" 写入到文件夹: {0}/{1}", id, eTocName);
                    string actionInfo = string.Format(" 动作id: {0}/{1}", sprites.Key, dirName);
                    string textureSizeInfo = string.Format(" \n贴图大小: {0}/{1}", maxWidth, maxHeight);
                    string pixelSizeInfo = string.Format(" \n图像大小: {0}/{1}", width, height);

                    text.text = "正在导出资源: " + assetbundleInfo + frameInfo + directoryInfo + actionInfo + textureSizeInfo + pixelSizeInfo;

                    image.Apply();
                    SavePng(spriteSavePath, image);
                    DestroyImmediate(image, true);
                    spriteIndex++;
                    yield return new WaitForSeconds(0.01f);
                }
            }

            foreach (KeyValuePair<string, Texture2D> value in texture2Ds)
            {
                if (value.Value != null)
                    DestroyImmediate(value.Value, true);
            }

            Destroy(monster);
            assetBundle.Unload(true);
            texture2Ds.Clear();
            spriteDic.Clear();
            Debug.Log(filePath + " 导出完毕");
            yield return new WaitForSeconds(0.2f);
        }

        unZip = false;
        Debug.Log("全部资源导出完毕: " + string.Format("({0}/{1}), 共导出: {2} 张序列帧, 结束时间: {3}", resIndex, filePaths.Count, allSpriteCount, DateTime.Now.ToString()));
        text.text = "全部资源导出完毕: " + string.Format("({0}/{1}), 共导出: {2} 张序列帧", resIndex, filePaths.Count, allSpriteCount);
    }

    static void PerformOverrideClipListCleanup(AnimatorOverrideController overrideController) 
    {
        var assembly = typeof(AnimatorOverrideController).Assembly; 
        var type = assembly.GetType("UnityEngine.AnimatorOverrideController");
        var method = type.GetMethod("PerformOverrideClipListCleanup", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic);
        method.Invoke(overrideController, new object[] { });
    }
    public void SavePng(string path, Texture2D tex)
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

}

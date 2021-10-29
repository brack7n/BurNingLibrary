using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Threading;
using System.Text;

public enum UnzipType
{
    zipToPkm,
    PkmToTexture,
    TextureToSprite,
}

public class PngAndPList
{
    public string pngPath;
    public string plistPath;
}

public class TPFrameData
{
    public string name;
    public Rect frame;
    public Vector2 offset;
    public bool rotated;
    public Rect sourceColorRect;
    public Vector2 sourceSize;

    public void LoadX(string sname, PList plist)
    {
        name = sname;
        //frame = TPAtlas.StrToRect(plist["frame"] as string);
        //offset = TPAtlas.StrToVec2(plist["offset"] as string);
        //sourceColorRect = TPAtlas.StrToRect(plist["sourceColorRect"] as string);
        //sourceSize = TPAtlas.StrToVec2(plist["sourceSize"] as string);
        object varCheck;
        if (plist.TryGetValue("frame", out varCheck))
        {
            frame = TPAtlas.StrToRect(plist["frame"] as string);
            offset = TPAtlas.StrToVec2(plist["offset"] as string);
            sourceColorRect = TPAtlas.StrToRect(plist["sourceColorRect"] as string);
            sourceSize = TPAtlas.StrToVec2(plist["sourceSize"] as string);
            rotated = (bool)plist["rotated"];
        }
        else
        {
            frame = TPAtlas.StrToRect(plist["textureRect"] as string);
            offset = TPAtlas.StrToVec2(plist["spriteOffset"] as string);
            sourceColorRect = TPAtlas.StrToRect(plist["sourceColorRect"] as string);
            sourceSize = TPAtlas.StrToVec2(plist["spriteSourceSize"] as string);
        }
    }
}

public class TPAtlas
{
    public string realTextureFileName;
    public Vector2 size;
    public List<TPFrameData> sheets = new List<TPFrameData>();

    public void LoadX(PList plist)
    {
        //read metadata
        PList meta = plist["metadata"] as PList;
        object varCheck;
        if (meta.TryGetValue("realTextureFileName", out varCheck))
        {
            realTextureFileName = meta["realTextureFileName"] as string;
        }
        else
        {
            PList ptarget = meta["target"] as PList;
            realTextureFileName = ptarget["name"] as string;
        }

        size = StrToVec2(meta["size"] as string);

        //read frames
        PList frames = plist["frames"] as PList;
        foreach (var kv in frames)
        {
            string name = kv.Key;
            PList framedata = kv.Value as PList;
            TPFrameData frame = new TPFrameData();
            frame.LoadX(name, framedata);
            sheets.Add(frame);
        }
    }

    public static Vector2 StrToVec2(string str)
    {

        str = str.Replace("{", "");
        str = str.Replace("}", "");
        string[] vs = str.Split(',');

        Vector2 v = new Vector2();
        v.x = float.Parse(vs[0]);
        v.y = float.Parse(vs[1]);
        return v;
    }
    public static Rect StrToRect(string str)
    {
        str = str.Replace("{", "");
        str = str.Replace("}", "");
        string[] vs = str.Split(',');

        Rect v = new Rect(float.Parse(vs[0]), float.Parse(vs[1]), float.Parse(vs[2]), float.Parse(vs[3]));
        return v;
    }
}

public class PList : Dictionary<string, object>
{
    public PList()
    {
    }

    public PList(string file)
    {
        Load(file);
    }

    public void Load(string file)
    {
        Clear();

        XDocument doc = XDocument.Load(file);
        XElement plist = doc.Element("plist");
        XElement dict = plist.Element("dict");

        var dictElements = dict.Elements();
        Parse(this, dictElements);
    }

    public void LoadText(string text)
    {
        Clear();
        XDocument doc = XDocument.Parse(text);
        XElement plist = doc.Element("plist");
        XElement dict = plist.Element("dict");

        var dictElements = dict.Elements();
        Parse(this, dictElements);
    }

    private void Parse(PList dict, IEnumerable<XElement> elements)
    {
        for (int i = 0; i < elements.Count(); i += 2)
        {
            XElement key = elements.ElementAt(i);
            XElement val = elements.ElementAt(i + 1);

            dict[key.Value] = ParseValue(val);
        }
    }

    private List<object> ParseArray(IEnumerable<XElement> elements)
    {
        List<dynamic> list = new List<dynamic>();
        foreach (XElement e in elements)
        {
            object one = ParseValue(e);
            list.Add(one);
        }

        return list;
    }

    private object ParseValue(XElement val)
    {
        switch (val.Name.ToString())
        {
            case "string":
                return val.Value;
            case "integer":
                return int.Parse(val.Value);
            case "real":
                return float.Parse(val.Value);
            case "true":
                return true;
            case "false":
                return false;
            case "dict":
                PList plist = new PList();
                Parse(plist, val.Elements());
                return plist;
            case "array":
                List<object> list = ParseArray(val.Elements());
                return list;
            default:
                throw new ArgumentException("Unsupported");
        }
    }
}

[Serializable]
public class sprtieData
{
    public string frame;
    public string offset;
    public bool rotated;
    public string sourceColorRect;
    public string sourceSize;
}

public class UnzipTexture2D
{
    private static List<Color> colors = new List<Color>();
    private static bool reset = false;
    private static int maxWidth = 512;
    private static int maxHeight = 512;
    private static string resOutputRootPath = Path.GetFullPath(Application.dataPath + "/../YSCQResources/");
    private static string zipOutputRootPath = Path.GetFullPath(Application.dataPath + "/../YSCQZip/");
    private static string OriginalResRootPath = Path.GetFullPath(Application.dataPath + "/../yscq/stab/");
    private static List<System.Diagnostics.Process> processList = new List<System.Diagnostics.Process>();

    private static Dictionary<int, string> idToGender = new Dictionary<int, string>()
    {
        [0] = "��",
        [1] = "Ů",
    };

    private static Dictionary<int, string> idToAction = new Dictionary<int, string>()
    {
        [0] = "����",
        [1] = "��·",
        [2] = "����",
        [3] = "ʩ��",
        [4] = "����",
        [5] = "�ܲ�",
        [6] = "6",
        [7] = "�ܻ�",
        [8] = "�ɼ�",
        [9] = "9",
        [10] = "����1",
        [11] = "��ս",
    };

    //ֻ������Щid����Դ
    private static Dictionary<string, int> resIdList = new Dictionary<string, int>()
    {
        //["0314"] = 1,
        //["0034"] = 1,
        //["0237"] = 1,
    };

    public static void ConvertSprite(string plistPath, string texturePath)
    {
        if (string.IsNullOrEmpty(plistPath) || string.IsNullOrEmpty(texturePath))
            return;

        //weapon_0040_0_1_0_0000
        //0.��Դ����
        //1.��Դid
        //2.��Ů 0-�� 1-Ů
        //3.�������� ��·֮���
        //4.����
        //5.�����ϵ�����֡

        //frame: Сͼ�ڴ�ͼ��λ����Ϣ{ { ���ϽǶ�������}��{ Сͼ���} }
        //offset: Сͼ�ü�ǰ�����ĵ��ƫ��ֵ
        //rotated: Сͼ�ڴ�ͼ���Ƿ���ת
        //sourceColorRect: Сͼ�ü�����Ϣ
        //sourceSize: Сͼԭ�ߴ��С

        //ͼƬ����������: ��cocos���Ƶ������ĵ�ê��Ϊ(0, 1) ���Ͻ�
        //��Unity�е�ת�� ����λ�ڴ�ͼ�е�λ��(frame) + �����и���ƫ��ֵ(offset) - ͼƬ��ʵ�ʸ�(��Ϊcocos��ʹ�õ�ʱ��ê��λ������)

        string plistText = File.ReadAllText(plistPath);
        PList plist = new PList();
        plist.LoadText(plistText);

        TPAtlas at = new TPAtlas();
        at.LoadX(plist);

        if(!File.Exists(texturePath))
            return;

        byte[] bytes = File.ReadAllBytes(texturePath);
        Texture2D t2D = new Texture2D((int)at.size.x, (int)at.size.y);
        t2D.LoadImage(bytes);
        t2D.Apply();

        for (int i = 0; i < at.sheets.Count; i++)
        {
            TPFrameData tPFrameData = at.sheets[i];
            string[] names = tPFrameData.name.Split('_');
            string resType = names[0];
            string resid = names[1];
            string resGender = names[2];
            string resActionType = names[3];
            if (idToAction.ContainsKey(int.Parse(resActionType)))
            {
                resActionType = idToAction[int.Parse(resActionType)];
            }

            string outputRootPath = resOutputRootPath + resType + @"\sprite\" + resid + @"\" + resGender + @"\" + resActionType;

            if (!Directory.Exists(outputRootPath))
                Directory.CreateDirectory(outputRootPath);

            int dir = (int.Parse(names[4]) + 1) * 10000;
            string frameIndex = names[5].Split('.')[0];
            int frameid = int.Parse(frameIndex) + dir;
            string outpuSpritePath = outputRootPath + @"\" + frameid + ".png";
            if(File.Exists(outpuSpritePath))
            {
                continue;
            }

            int width = (int)tPFrameData.frame.width;
            int height = (int)tPFrameData.frame.height;

            int pivotx = (int)tPFrameData.offset.x;
            int pivoty = (int)tPFrameData.offset.y;

            int x = (int)tPFrameData.frame.x;
            int y = (int)(at.size.y - tPFrameData.frame.y - tPFrameData.frame.height);

            Vector2 center = new Vector2(maxWidth / 2, maxHeight / 2);
            float startX = center.x + pivotx;
            float StartY = center.y + pivoty - height;

            //���ͼƬ����ת �������
            if (tPFrameData.rotated)
            {
                y = (int)(at.size.y - tPFrameData.frame.y - tPFrameData.frame.width);
                width = height;
                height = (int)tPFrameData.frame.width;
            }

            Texture2D image = new Texture2D(width, height);
            InitTransparentColor();
            //ͼƬ���ó�͸������
            image.SetPixels(colors.ToArray());

            try
            {
                image.SetPixels(t2D.GetPixels(x, y, width, height));
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message + "  " + outpuSpritePath);
            }

            Color[] src = image.GetPixels(0, 0, width, height);
            Color[] des = new Color[src.Length];

            if (tPFrameData.rotated)
            {
                //��ʱ����ת90��
                if (image.width != height || image.height != width)
                {
                    image.Resize(height, width);
                }

                for (int colori = 0; colori < width; colori++)
                {
                    for (int colorj = 0; colorj < height; colorj++)
                    {
                        des[colori * height + colorj] = src[(height - 1 - colorj) * width + colori];
                    }
                }

               image.SetPixels(des);
            }
            image.Apply();

            if (tPFrameData.rotated)
            {
                startX = center.x + pivotx;
                StartY = center.y + pivoty - image.height;
            }

            //����ԭʼ�������Դ���Ĳ��� ƫ��ֵ
            #region ����ƫ��
            int offsetX = -24;
            int offsetY = 16;
            #endregion

            startX += offsetX;
            StartY += offsetY;

            //�ŵ�һ�Ŵ�ͼ��
            Texture2D imageBig = new Texture2D(maxWidth, maxHeight);
            imageBig.SetPixels(colors.ToArray());

            for (int colori = 0; colori < tPFrameData.frame.width; colori++)
            {
                for (int colorj = 0; colorj < tPFrameData.frame.height; colorj++)
                {
                    Color color = image.GetPixel(colori, colorj);
                    imageBig.SetPixel(colori + (int)startX, colorj + (int)StartY, color);
                }
            }

            imageBig.Apply();
            SavePng(outpuSpritePath, imageBig);
            UnityEngine.Object.DestroyImmediate(image, true);
            UnityEngine.Object.DestroyImmediate(imageBig, true);
            //EditorUtility.DisplayCancelableProgressBar(string.Format("������Դ: ({0})", tPFrameData.name), string.Format("����·��: ({0})", outpuSpritePath), (float)i / (float)at.sheets.Count);
        }

        UnityEngine.Object.DestroyImmediate(t2D, true);
        //EditorUtility.ClearProgressBar();
        //EditorUtility.DisplayDialog("��Դ����", "��Դ·��: " + plistPath, "ok");

        //AssetDatabase.Refresh();
    }

    [MenuItem("Tools/ԭʼ����/all")]
    public static void UnzipAllEditor()
    {
        Debug.Log(DateTime.Now.ToString());
        UnzipEditor();
        PkmToPngEditor();
        TextureToSpriteEditor();
        Debug.Log(DateTime.Now.ToString());
    }

    [MenuItem("Tools/ԭʼ����/unzip")]
    public static void UnzipEditor() 
    {
        UnAllzipEditor(UnzipType.zipToPkm);
    }

    [MenuItem("Tools/ԭʼ����/pkm to png")]
    public static void PkmToPngEditor()
    {
        UnAllzipEditor(UnzipType.PkmToTexture);
    }

    [MenuItem("Tools/ԭʼ����/texture to sprite")]
    public static void TextureToSpriteEditor()
    {
        resIdList.Clear();
        UnAllzipEditor(UnzipType.TextureToSprite);
    }

    [MenuItem("Tools/ԭʼ����/pkm to png(player)")]
    public static void PkmToPngPlayerEditor()
    {
        List<string> rootPath = new List<string>()
        {
            OriginalResRootPath + "anim/player",
        };
        UnAllzipEditor(UnzipType.PkmToTexture, rootPath);
    }

    [MenuItem("Tools/ԭʼ����/pkm to png(weapon)")]
    public static void PkmToPngWeaponEditor()
    {
        List<string> rootPath = new List<string>()
        {
            OriginalResRootPath + "anim/weapon",
        };
        UnAllzipEditor(UnzipType.PkmToTexture, rootPath);
    }

    [MenuItem("Tools/ԭʼ����/pkm to png(monster)")]
    public static void PkmToPngMonsterEditor()
    {
        List<string> rootPath = new List<string>()
        {
            OriginalResRootPath + "anim/monster",
        };
        UnAllzipEditor(UnzipType.PkmToTexture, rootPath);
    }

    [MenuItem("Tools/ԭʼ����/pkm to png(hair)")]
    public static void PkmToPngHairEditor()
    {
        List<string> rootPath = new List<string>()
        {
            OriginalResRootPath + "anim/hair",
        };
        UnAllzipEditor(UnzipType.PkmToTexture, rootPath);
    }

    [MenuItem("Tools/ԭʼ����/texture to sprite(player)")]
    public static void TextureToSpriteplayerEditor()
    {
        List<string> rootPath = new List<string>()
        {
            OriginalResRootPath + "anim/player",
        };

        UnAllzipEditor(UnzipType.TextureToSprite, rootPath);
    }

    [MenuItem("Tools/ԭʼ����/texture to sprite(weapon)")]
    public static void TextureToSpriteWeaponEditor()
    {
        List<string> rootPath = new List<string>()
        {
            OriginalResRootPath + "anim/weapon",
        };

        UnAllzipEditor(UnzipType.TextureToSprite, rootPath);
    }

    [MenuItem("Tools/ԭʼ����/texture to sprite(monster)")]
    public static void TextureToSpriteMonsterEditor()
    {
        List<string> rootPath = new List<string>()
        {
            OriginalResRootPath + "anim/monster",
        };

        UnAllzipEditor(UnzipType.TextureToSprite, rootPath);
    }

    [MenuItem("Tools/ԭʼ����/texture to sprite(hair)")]
    public static void TextureToSpriteHairEditor()
    {
        List<string> rootPath = new List<string>()
        {
            OriginalResRootPath + "anim/hair",
        };

        UnAllzipEditor(UnzipType.TextureToSprite, rootPath);
    }

    public static void UnAllzipEditor(UnzipType unzipType, List<string> sigleRootPath = null)
    {
        List<string> resRootPath = new List<string>() 
        {
            OriginalResRootPath + "anim/monster",
            OriginalResRootPath + "anim/player",
            OriginalResRootPath + "anim/weapon",
            OriginalResRootPath + "anim/hair",
        };

        if (sigleRootPath != null)
        {
            resRootPath.Clear();
            resRootPath = sigleRootPath;
        }

        Dictionary<string, Dictionary<string, PngAndPList>> pngAndPlistDicDic = new Dictionary<string, Dictionary<string, PngAndPList>>();

        for (int i = 0; i < resRootPath.Count; i++)
        {
            string rootPath = resRootPath[i];
            string rootPathFileName = Path.GetFileNameWithoutExtension(rootPath);
            Dictionary<string, PngAndPList> pngAndPlistDic;
            if (!pngAndPlistDicDic.TryGetValue(rootPathFileName, out pngAndPlistDic))
            {
                pngAndPlistDic = new Dictionary<string, PngAndPList>();
                pngAndPlistDicDic.Add(rootPathFileName, pngAndPlistDic);
            }

            float index = 0;
            List<string> filePaths = FindFiles(rootPath);
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileName(filePath);
                string[] names = fileName.Split('.');

                string name = names[0];
                string ext = names[1];

                if (resIdList.Count > 0)
                {
                    string resId = name.Split('_')[1];
                    if (!resIdList.ContainsKey(resId))
                    {
                        continue;
                    }
                }

                PngAndPList pngAndPList;
                if (!pngAndPlistDic.TryGetValue(name, out pngAndPList))
                {
                    pngAndPList = new PngAndPList();
                    pngAndPlistDic.Add(name, pngAndPList);
                }

                if (ext.Equals("png"))
                {
                    pngAndPList.pngPath = filePath;
                }
                else if (ext.Equals("plist"))
                {
                    pngAndPList.plistPath = filePath;
                }

                index++;
                EditorUtility.DisplayCancelableProgressBar(string.Format("��Դ·������: ({0})", rootPathFileName),
                    string.Format("path: ({0})", filePath), index / (float)filePaths.Count);
            }
        }
        EditorUtility.ClearProgressBar();

        float lastRealtimeSinceStartup = 0;
        float threshold = 5;
        int resIndex = 0;
        int childIndex = 0;

        var enumerator = pngAndPlistDicDic.GetEnumerator();
        bool next = enumerator.MoveNext();
        Dictionary<string, PngAndPList>.Enumerator childEnumerator;

        while (resIndex < pngAndPlistDicDic.Count)
        {
            while (childIndex < enumerator.Current.Value.Count)
            {
                PngAndPList pngAndPList;
                float timeDiff = (Time.realtimeSinceStartup * 1000) - (lastRealtimeSinceStartup * 1000);
                if (timeDiff >= threshold)
                {
                    timeDiff = 0;
                    lastRealtimeSinceStartup = Time.realtimeSinceStartup;

                    if (childIndex == 0)
                        childEnumerator = enumerator.Current.Value.GetEnumerator();
                    
                    bool childNext = childEnumerator.MoveNext();

                    if (childNext)
                    {
                        pngAndPList = childEnumerator.Current.Value;

                        string info = "";
                        switch (unzipType)
                        {
                            case UnzipType.zipToPkm:
                                Unzip(pngAndPList.pngPath, pngAndPList.plistPath);
                                info = string.Format("zip to pkm: {0}/{1}", childIndex, enumerator.Current.Value.Count);
                                break;
                            case UnzipType.PkmToTexture:
                                PkmToTexture(pngAndPList.pngPath, pngAndPList.plistPath);
                                info = string.Format("pkm to texture: {0}/{1}", childIndex, enumerator.Current.Value.Count);
                                break;
                            case UnzipType.TextureToSprite:
                                TextureToSprite(pngAndPList.pngPath, pngAndPList.plistPath);
                                info = string.Format("texture to sprite: {0}/{1}", childIndex, enumerator.Current.Value.Count);
                                break;
                        }

                        EditorUtility.DisplayCancelableProgressBar(info, 
                            string.Format("path: ({0})", pngAndPList.pngPath), childIndex / (float)enumerator.Current.Value.Count);
                    }

                    childIndex++;
                }
            }

            if (childIndex >= enumerator.Current.Value.Count)
            {
                childIndex = 0;
                resIndex++;
                next = enumerator.MoveNext();
            }
        }

        EditorUtility.ClearProgressBar();
        pngAndPlistDicDic.Clear();
    }

    private static void PkmToTexture(string path, string plistPath)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(plistPath))
            return;

        #region .png��׺������Ϊ.pkm��׺
        string name = Path.GetFileNameWithoutExtension(path);
        string outputPath = zipOutputRootPath + name.Split('_')[0] + @"\";
        outputPath = outputPath + name + @"\";

        string pkmPath = outputPath + name + ".pkm";
        string unzipFilePath = outputPath + name + ".png";

        //�󲿷���Դ�ļ����ͽ�ѹ������������ͬ, ������Щ��ѹ����������ͬ
        string[] filePaths = Directory.GetFiles(outputPath);
        foreach (string filePath in filePaths)
        {
            if (filePath.EndsWith(".png"))
            {
                if (!filePath.Equals(unzipFilePath))
                {
                    Debug.LogError(string.Format("��ѹ�������ļ���zip�ļ���������: ��ѹ�������ļ�: {0} ---- zip�ļ�{1}", filePath, unzipFilePath));
                }

                unzipFilePath = filePath;
            }
        }

        byte[] buffer = File.ReadAllBytes(unzipFilePath);
        
        if (!File.Exists(pkmPath))
        {
            File.WriteAllBytes(pkmPath, File.ReadAllBytes(unzipFilePath));
        }
        #endregion

        #region pkm to png
        string outputPngPath = resOutputRootPath;
        if (!Directory.Exists(outputPngPath))
            Directory.CreateDirectory(outputPngPath);

        string fileName = Path.GetFileNameWithoutExtension(path);
        string[] fileNames = fileName.Split('_');
        string resType = fileNames[0];              //����monster
        string resid = fileNames[1];                //����0010
        string resGender = fileNames[2];            //����0/1
        string resActionType = fileNames[3];        //����0++

        string spriteOutputPath = outputPngPath + resType + @"\sprite\" + resid + @"\" + resGender;
        string textureOutputPath = outputPngPath + resType + @"\texture\" + resid + @"\" + resGender;

        if (!Directory.Exists(spriteOutputPath))
        {
            Directory.CreateDirectory(spriteOutputPath);
            Debug.LogError("����Դ·��: " + spriteOutputPath);
        }

        if (!Directory.Exists(textureOutputPath))
            Directory.CreateDirectory(textureOutputPath);

        string exePath = @"etcpack.exe";
        string pngFilePath = textureOutputPath + @"\" + Path.GetFileNameWithoutExtension(pkmPath) + ".png";
        if (!File.Exists(pngFilePath))
        {
            string suffix = "-ext PNG";
            string param = "/k " + exePath + " " + pkmPath + " " + textureOutputPath + " " + suffix;
            string cmdExePath = @"C:\Windows\System32\cmd.exe";

            PkmToPngStartCmd(cmdExePath, param, buffer.Length);
            Debug.Log("pkm to png, output path: " + pngFilePath);
        }
        #endregion
    }

    private static void Unzip(string path, string plistPath)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(plistPath))
            return;

        #region ��ѹzip
        try
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string outputPath = zipOutputRootPath + fileName.Split('_')[0] + @"\";
            outputPath = outputPath + fileName + @"\";

            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            string name = fileName + ".rar";
            string zipOutputPath = outputPath + name;

            if (File.Exists(zipOutputPath))
            {
                return;
            }

            File.WriteAllBytes(zipOutputPath, File.ReadAllBytes(path));

            string zipPath = outputPath + name;
            string commandOptions = string.Format(@" x {0} {1} -y", zipPath, outputPath);
            string winRarExePath = @"C:\Program Files\WinRAR\WinRAR.exe";
            StartCmd(winRarExePath, commandOptions);
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }
        #endregion
    }

    //[MenuItem("Tools/Texture/test (weapon)")]
    //private static void Test()
    //{
    //    string path = @"C:\Users\lihehui\Desktop\yscq\stab\anim\weapon\weapon_0110_0_2.png";
    //    string plistPath = @"C:\Users\lihehui\Desktop\yscq\stab\anim\weapon\weapon_0110_0_2.plist";
    //    TextureToSprite(path, plistPath);

    //    path = @"C:\Users\lihehui\Desktop\yscq\stab\anim\player\player_9999_0_2.png";
    //    plistPath = @"C:\Users\lihehui\Desktop\yscq\stab\anim\player\player_9999_0_2.plist";
    //    TextureToSprite(path, plistPath);
    //}

    private static void TextureToSprite(string path, string plistPath)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(plistPath))
            return;

        string fileName = Path.GetFileNameWithoutExtension(path);
        string[] fileNames = fileName.Split('_');
        string resType = fileNames[0];
        string resid = fileNames[1];
        string resGender = fileNames[2];
        string resActionType = fileNames[3];

        string textureOutputPath = resOutputRootPath + resType + @"\texture\" + resid + @"\" + resGender;
        string pngFilePath = textureOutputPath + @"\" + fileName + ".png";

        ConvertSprite(plistPath, pngFilePath);
    }

    private static void PkmToPngStartCmd(string cmd, string Command, int time)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();//�������̶���
        process.StartInfo.FileName = cmd;//�趨��Ҫִ�е�����
        process.StartInfo.RedirectStandardInput = false;
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.Arguments = Command;// ��������

        process.Start();
        process.WaitForExit(time / 1000);

        process.Kill();
        process.Close();
    }

    private static void StartCmd(string cmd, string Command)
    {
        System.Diagnostics.Process p = new System.Diagnostics.Process();//�������̶���
        p.StartInfo.FileName = cmd;//�趨��Ҫִ�е�����
        //p.StartInfo.CreateNoWindow = true;
        //p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        p.StartInfo.RedirectStandardInput = false;
        p.StartInfo.RedirectStandardOutput = false;
        p.StartInfo.Arguments = Command;// ��������
        p.EnableRaisingEvents = true;
        p.Start();

        p.WaitForExit();//�ȴ�����ִ�����˳�����
        p.Close();
    }

    private static void Dispose()
    {
        int Count = processList.Count;
        for (int i = 0; i < Count; i++)
        {
            System.Diagnostics.Process process = processList[i];
            if (process == null)
                continue;

            process.Kill();
            process.Close();
            process.Dispose();
        }
        processList.Clear();

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
}

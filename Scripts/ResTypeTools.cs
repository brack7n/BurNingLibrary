using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public enum EnumResType
{
    all,      //����
    player,   //��ɫ
    hair,     //ͷ��
    weapon,   //����
}

public class ResTypeTools
{
    private static string rootPath = Application.dataPath + @"\..\";
    private static string ssdRootPath = @"E:\";

    //ɾ������ͼ�Ϳ��ı�·��
    private static List<string> sourcePaths = new List<string>
    {
        ssdRootPath + @"RXCQ\bmp\Hum",
        ssdRootPath + @"RXCQ\bmp\hum2",
        ssdRootPath + @"RXCQ\bmp\hum3",
        ssdRootPath + @"RXCQ\bmp\hair2",
        ssdRootPath + @"RXCQ\bmp\Weapon",
        ssdRootPath + @"RXCQ\bmp\weapon2",
        ssdRootPath + @"RXCQ\bmp\weapon3",
        ssdRootPath + @"RXCQ\bmp\weapon9",
    };

    private static Dictionary<bool, string> genderFileNames = new Dictionary<bool, string>
    {
        [false] = "��ģ", [true] = "Ůģ"
    };

    private static Dictionary<MirAction, string> eToc = new Dictionary<MirAction, string>
    {
        [MirAction.ShiFa] = "ʩ��",
        [MirAction.Attack1] = "����",
        [MirAction.Attack2] = "����1",
        [MirAction.Attack3] = "����2",
        [MirAction.BeiJi] = "����",
        [MirAction.BeiZhan] = "��ս",
        [MirAction.Die] = "����",
        [MirAction.Running] = "�ܲ�",
        [MirAction.ShiQu] = "ʰȡ",
        [MirAction.Standing] = "����",
        [MirAction.Walking] = "��·",
    };

    private static Dictionary<EnumResType, List<string>> sourcePaths_Type = new Dictionary<EnumResType, List<string>>()
    {
        [EnumResType.all] = new List<string>
        {
            ssdRootPath + @"RXCQ\bmp\Hum",
            ssdRootPath + @"RXCQ\bmp\hum2",
            ssdRootPath + @"RXCQ\bmp\hum3",
            ssdRootPath + @"RXCQ\bmp\hair2",
            ssdRootPath + @"RXCQ\bmp\Weapon",
            ssdRootPath + @"RXCQ\bmp\weapon2",
            ssdRootPath + @"RXCQ\bmp\weapon3",
            ssdRootPath + @"RXCQ\bmp\weapon9",
        },

        [EnumResType.player] = new List<string> 
        {
            ssdRootPath + @"RXCQ\bmp\Hum",
            ssdRootPath + @"RXCQ\bmp\hum2",
            ssdRootPath + @"RXCQ\bmp\hum3",
        },

        [EnumResType.hair] = new List<string> 
        {
            ssdRootPath + @"RXCQ\bmp\hair2",
        },

        [EnumResType.weapon] = new List<string>
        {
            ssdRootPath + @"RXCQ\bmp\Weapon",
            ssdRootPath + @"RXCQ\bmp\weapon2",
            ssdRootPath + @"RXCQ\bmp\weapon3",
            ssdRootPath + @"RXCQ\bmp\weapon9",
        },
    };

    private static string outPngParentPath = ssdRootPath + @"RXCQ\png\";

    [MenuItem("Tools/��Ѫ���滳��/ɾ���հ�ͼƬ")]
    public static void DeleteTexture()
    {
        int pathIndex = 1;
        foreach (string sourcePath in sourcePaths)
        {
            List<string> pointPaths = Utils.FindFiles(sourcePath, "*.txt");
            float index = 0;
            foreach (string pointPath in pointPaths)
            {
                index++;
                string text = File.ReadAllText(pointPath);
                string[] offsets = text.Split(new char[2] { '\r', '\n' });
                string fileName = Path.GetFileNameWithoutExtension(pointPath);

                Offset offset = new Offset();
                offset.x = int.Parse(offsets[0]);
                offset.y = int.Parse(offsets[2]);

                if (offset.IsEmpty())
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(pointPath);
                    string bmpParentPath = directoryInfo.Parent.Parent.FullName;
                    string bmpPath = bmpParentPath + @"\" + fileName + @".bmp";

                    if (File.Exists(bmpPath))
                    {
                        byte[] buffer = File.ReadAllBytes(bmpPath);

                        if (buffer.Length != 1082)
                        {
                            Debug.LogError("����ƫ��ֵΪ0,0, ����ͼƬ���ֽڲ���1082�� path: " + pointPath);
                            continue;
                        }

                        File.Delete(bmpPath);
                    }

                    if (File.Exists(pointPath))
                        File.Delete(pointPath);
                }

                EditorUtility.DisplayProgressBar("ɾ���հ�ͼƬ" + string.Format("{0} - {1}, {2}/{3}", pathIndex, sourcePaths.Count, index, pointPaths.Count), pointPath, index / (float)pointPaths.Count);
            }

            pathIndex++;
            EditorUtility.ClearProgressBar();
            Rename(sourcePath);
        }
    }

    [MenuItem("Tools/��Ѫ���滳��/��Դ����-(ͷ�Σ���������ɫ)")]
    public static void ResType_All()
    {
        ResType(EnumResType.all);
    }

    [MenuItem("Tools/��Ѫ���滳��/��Դ����-ͷ��")]
    public static void ResType_Hair()
    {
        ResType(EnumResType.hair);
    }

    [MenuItem("Tools/��Ѫ���滳��/��Դ����-����")]
    public static void ResType_Weapon()
    {
        ResType(EnumResType.weapon);
    }

    [MenuItem("Tools/��Ѫ���滳��/��Դ����-��ɫ")]
    public static void ResType_Player()
    {
        ResType(EnumResType.player);
    }

    public static void ResType(EnumResType enumResType)
    {
        if(!sourcePaths_Type.ContainsKey(enumResType))
        {
            Debug.LogError("����������·��, Res Type = " + enumResType.ToString());
            return;
        }

        int pathIndex = 1;
        List<string> paths = sourcePaths_Type[enumResType];
        foreach (string sourcePath in paths)
        {            
            int maxCount = Utils.FindFiles(sourcePath, "*.bmp").Count;

            Dictionary<MirAction, Frame> playerFrames = FrameSet.Players.Frames;
            int resMaxFrame = FrameSet.Players.resMaxFrame;
            int resIndex = 0;
            bool gender = false; //0�� 1Ů
            int maleResId = 1000;
            int femaleRedId = 999;
            int startFrameIndex = 0;
            string genderFileName = "";
            bool end = false;

            if (enumResType == EnumResType.hair)
            {
                gender = true;
            }
            else
            {
                gender = false;
            }

            BmpData bmpData = new BmpData();

            while (!end)
            {
                string resType = Path.GetFileNameWithoutExtension(sourcePath) + @"\";
                genderFileName = genderFileNames[gender] + @"\";

                foreach (var value in playerFrames)
                {
                    int genderResId = gender == true ? femaleRedId : maleResId;

                    string actionName = eToc[value.Key];
                    string outPngParentPath = ResTypeTools.outPngParentPath + resType + genderFileName + genderResId + @"\" + actionName + @"\";

                    if (!Directory.Exists(outPngParentPath))
                        Directory.CreateDirectory(outPngParentPath);

                    Frame frame = value.Value;
                    for (byte Direction = (byte)MirDirection.Up; Direction <= (byte)MirDirection.UpLeft; Direction++)
                    {
                        int actionIndex = 10000 * (Direction + 1);
                        for (int FrameIndex = 0; FrameIndex < frame.Count; FrameIndex++)
                        {
                            resIndex++;

                            int Frame = frame.Start + (frame.OffSet * Direction) + FrameIndex + startFrameIndex;
                            string bmpFilePath = sourcePath + @"\" + Frame + @".bmp";
                            string offsetPath = sourcePath + @"\Placements\" + Frame + @".txt";

                            if (!File.Exists(bmpFilePath))
                            {
                                end = true;
                                break;
                            }

                            string pngPath = outPngParentPath + actionIndex + @".png";

                            if (!File.Exists(pngPath))
                            {
                                bmpData.bmpPath = bmpFilePath;
                                bmpData.pngPath = pngPath;
                                bmpData.offsetPath = offsetPath;
                                BmpToPng.BmpToPngFunc(bmpData);
                            }

                            actionIndex++;
                            EditorUtility.DisplayProgressBar(enumResType.ToString() + "��Դ����" + string.Format("{0} - {1}, {2}/{3}", pathIndex, paths.Count, resIndex, maxCount),
                                Path.GetFullPath(pngPath), resIndex / (float)maxCount);
                        }

                        if (end)
                            break;
                    }

                    if (end)
                        break;
                }

                gender = !gender;
                if (gender)
                {
                    femaleRedId++;
                }
                else
                {
                    maleResId++;
                    break;
                }

                startFrameIndex += resMaxFrame;
                //#region ���Դ���
                //EditorUtility.ClearProgressBar();
                //break;
                //#endregion
            }

            pathIndex++;
        }
        
        EditorUtility.ClearProgressBar();
    }

    public static void ResType_Hair(EnumResType enumResType)
    {
        if (!sourcePaths_Type.ContainsKey(enumResType))
        {
            Debug.LogError("����������·��, Res Type = " + enumResType.ToString());
            return;
        }

        int pathIndex = 1;
        List<string> paths = sourcePaths_Type[enumResType];
        foreach (string sourcePath in paths)
        {
            int maxCount = Utils.FindFiles(sourcePath, "*.bmp").Count;

            Dictionary<MirAction, Frame> playerFrames = FrameSet.Players.Frames;
            int resMaxFrame = FrameSet.Players.resMaxFrame;
            int resIndex = 0;
            int resId = 1000;
            int startFrameIndex = 0;
            bool end = false;
            BmpData bmpData = new BmpData();

            //���ǵ�һ����Դ�պ���416��, ��һ�׶�����416��ʼ����
            //ͷ�ν�β����һ��ͼƬ��֪����ɶ ���ԼӸ�frameOffset����һ��
            int frameOffset = 1;

            while (!end)
            {
                string resType = Path.GetFileNameWithoutExtension(sourcePath) + @"\";

                foreach (var value in playerFrames)
                {
                    string actionName = eToc[value.Key];
                    string outPngParentPath = ResTypeTools.outPngParentPath + resType + resId + @"\" + actionName + @"\";

                    if (!Directory.Exists(outPngParentPath))
                        Directory.CreateDirectory(outPngParentPath);

                    Frame frame = value.Value;
                    for (byte Direction = (byte)MirDirection.Up; Direction <= (byte)MirDirection.UpLeft; Direction++)
                    {
                        int actionIndex = 10000 * (Direction + 1);
                        for (int FrameIndex = 0; FrameIndex < frame.Count; FrameIndex++)
                        {
                            resIndex++;

                            int Frame = frame.Start + (frame.OffSet * Direction) + FrameIndex + startFrameIndex;
                            string bmpFilePath = sourcePath + @"\" + Frame + @".bmp";
                            string offsetPath = sourcePath + @"\Placements\" + Frame + @".txt";

                            if (!File.Exists(bmpFilePath))
                            {
                                end = true;
                                break;
                            }

                            string pngPath = outPngParentPath + actionIndex + @".png";

                            if (!File.Exists(pngPath))
                            {
                                bmpData.bmpPath = bmpFilePath;
                                bmpData.pngPath = pngPath;
                                bmpData.offsetPath = offsetPath;
                                BmpToPng.BmpToPngFunc(bmpData);
                            }

                            actionIndex++;
                            EditorUtility.DisplayProgressBar(enumResType.ToString() + "��Դ����" + string.Format("{0} - {1}, {2}/{3}", pathIndex, paths.Count, resIndex, maxCount), 
                                Path.GetFullPath(pngPath), resIndex / (float)maxCount);
                        }

                        if (end)
                            break;
                    }

                    if (end)
                        break;
                }

                resId++;
                startFrameIndex += resMaxFrame + frameOffset;
                //#region ���Դ���
                //EditorUtility.ClearProgressBar();
                //break;
                //#endregion
            }

            pathIndex++;
        }

        EditorUtility.ClearProgressBar();
    }

    public static void Rename(string sourcePath)
    {
        List<string> pointPaths = Utils.FindFiles(sourcePath, "*.txt");

        int index = 0;
        foreach (string path in pointPaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            string txtParentPath = directoryInfo.Parent.FullName;
            string txtPath = txtParentPath + @"\" + index + @".txt";

            if (!File.Exists(txtPath))
                File.Move(path, txtPath);

            string bmpParentPath = directoryInfo.Parent.Parent.FullName;
            string bmpPath = bmpParentPath + @"\" + fileName + @".bmp";
            string newBmpPath = bmpParentPath + @"\" + index + @".bmp";

            if (!File.Exists(newBmpPath))
                File.Move(bmpPath, newBmpPath);

            EditorUtility.DisplayProgressBar("������", path, index / (float)pointPaths.Count);
            index++;
        }

        EditorUtility.ClearProgressBar();
    }
}

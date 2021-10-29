using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using B83.Image.BMP;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

public class Offset
{
    public int x;
    public int y;

    public bool IsEmpty()
    {
        return x == 0 && y == 0;
    }
}

public class BmpData
{
    public string bmpPath;
    public string pngPath;
    public string offsetPath;
}

public class BmpToPng
{
    private static List<Color> defaultColors = new List<Color>();
    private static int maxWidth = 1024;
    private static int maxHeight = 1024;

    [MenuItem("Tools/热血传奇怀旧/BmpToPng")]
    public static void BmpToPngFunc(BmpData bmpData)
    {
        InitTransparentColor();
        string text = File.ReadAllText(bmpData.offsetPath);
        string[] offsets = text.Split(new char[2] { '\r', '\n' });

        Offset offset = new Offset();
        offset.x = int.Parse(offsets[0]);
        offset.y = int.Parse(offsets[2]);

        Texture2D t2D = LoadTexture(bmpData.bmpPath);

        #region 重心偏移
        int offsetx = -25;
        int offsety = 19;
        #endregion

        #region 起始点计算
        //该资源坐标系左上角为0,0点
        //unity中左下角为0,0点

        //x起始位置计算大图的中心点+offset.x即可
        //y起始位置计算 由于资源的坐标0,0和unity中的0,0 y轴向相反 图片的高度+offset.y 大图的中心点-减去偏移值

        int startX = maxWidth / 2 + offset.x + offsetx;
        int StartY = maxHeight / 2 - (t2D.height + offset.y) + offsety;
        #endregion

        //放到一张大图上
        Texture2D pngTexture = new Texture2D(maxWidth, maxHeight);
        pngTexture.SetPixels(defaultColors.ToArray());

        for (int colori = 0; colori < t2D.width; colori++)
        {
            for (int colorj = 0; colorj < t2D.height; colorj++)
            {
                Color color = t2D.GetPixel(colori, colorj);
                if (color == Color.black)
                    color = Color.clear;

                pngTexture.SetPixel(colori + startX, colorj + StartY, color);
            }
        }

        pngTexture.Apply();
        Utils.SavePng(bmpData.pngPath, pngTexture);

        UnityEngine.Object.DestroyImmediate(t2D, true);
        UnityEngine.Object.DestroyImmediate(pngTexture, true);
    }

    public static Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;

        if (File.Exists(filePath))
        {
            BMPLoader bmpLoader = new BMPLoader();
            //bmpLoader.ForceAlphaReadWhenPossible = true; //Uncomment to read alpha too

            //Load the BMP data
            BMPImage bmpImg = bmpLoader.LoadBMP(filePath);

            //Convert the Color32 array into a Texture2D
            tex = bmpImg.ToTexture2D();
        }
        return tex;
    }

    public static void InitTransparentColor()
    {
        if (defaultColors.Count > 0)
            return;

        for (int i = 0; i < maxWidth; i++)
        {
            for (int j = 0; j < maxHeight; j++)
            {
                Color color = new Color(0, 0, 0, 0);
                defaultColors.Add(color);
            }
        }
    }
}

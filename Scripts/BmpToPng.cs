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

    [MenuItem("Tools/��Ѫ���滳��/BmpToPng")]
    public static void BmpToPngFunc(BmpData bmpData)
    {
        InitTransparentColor();
        string text = File.ReadAllText(bmpData.offsetPath);
        string[] offsets = text.Split(new char[2] { '\r', '\n' });

        Offset offset = new Offset();
        offset.x = int.Parse(offsets[0]);
        offset.y = int.Parse(offsets[2]);

        Texture2D t2D = LoadTexture(bmpData.bmpPath);

        #region ����ƫ��
        int offsetx = -25;
        int offsety = 19;
        #endregion

        #region ��ʼ�����
        //����Դ����ϵ���Ͻ�Ϊ0,0��
        //unity�����½�Ϊ0,0��

        //x��ʼλ�ü����ͼ�����ĵ�+offset.x����
        //y��ʼλ�ü��� ������Դ������0,0��unity�е�0,0 y�����෴ ͼƬ�ĸ߶�+offset.y ��ͼ�����ĵ�-��ȥƫ��ֵ

        int startX = maxWidth / 2 + offset.x + offsetx;
        int StartY = maxHeight / 2 - (t2D.height + offset.y) + offsety;
        #endregion

        //�ŵ�һ�Ŵ�ͼ��
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

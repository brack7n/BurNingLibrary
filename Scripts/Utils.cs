using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Utils
{
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

    public static List<string> FindFiles(string path, string filter = "*")
    {
        List<string> filePaths = new List<string>();
        if (Directory.Exists(path))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles(filter, SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                filePaths.Add(files[i].FullName);
            }
        }

        return filePaths;
    }
}

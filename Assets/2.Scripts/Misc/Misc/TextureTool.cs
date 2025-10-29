using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

public class TextureTool : MonoBehaviour
{
    public Material material;
    public string aircraftFile;
    public static string FolderPath(string subFolder)
    {
        return Application.persistentDataPath + "/" + subFolder + "/";
    }
    public static string[] ListPaths(string subFolder)
    {
        Directory.CreateDirectory(FolderPath(subFolder));
        return Directory.GetFiles(FolderPath(subFolder));
    }
    public static string[] ListNames(string subFolder)
    {
        string[] names = ListPaths(subFolder);
        for (int i = 0; i < names.Length; i++) names[i] = NameFromPath(names[i]);
        return names;
    }
    public static string NameFromPath(string path)
    {
        string[] split = path.Split('/');
        return split[split.Length - 1];
    }
    public static void Save(Texture2D texture, string subFolder, string overrideName)
    {
        byte[] png = texture.EncodeToPNG();
        Directory.CreateDirectory(FolderPath(subFolder));
        File.WriteAllBytes(FolderPath(subFolder) + overrideName, png);
    }
    public static void Rename(string currentName,ref string newName, string subFolder)
    {
        if (currentName == newName) return;

        newName = PreventDuplicateName(newName, subFolder);

        byte[] bytes = File.ReadAllBytes(FolderPath(subFolder) + currentName);

        File.WriteAllBytes(FolderPath(subFolder) + newName, bytes);
        Remove(subFolder, currentName);
    }
    public static Texture2D Load(string path)
    {
        Texture2D texture = new Texture2D(69, 420);
        texture.LoadImage(File.ReadAllBytes(path));
        return texture;
    }
    public static Texture2D Load(string subFolder, string name)
    {
        return Load(FolderPath(subFolder) + name);
    }
    public static void Remove(string subFolder, string name)
    {
        File.Delete(FolderPath(subFolder) + name);
    }

    public delegate void TexturePickedCallback(Texture2D texture2D);
    public static void Pick(string subfolder, TexturePickedCallback callback)
    {
        Texture2D texture = null;
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                texture = NativeGallery.LoadImageAtPath(path, 2048, false);
                if (texture != null)
                {
                    texture.name = PreventDuplicateName(NameFromPath(path), subfolder);
                    Save(texture, subfolder, texture.name);

                    callback.Invoke(texture);
                }
                // If a procedural texture is not destroyed manually, 
                // it will only be freed after a scene change
                //Destroy(texture, 5f);
            }
        });
    }

    public static string PreventDuplicateName(string ogName, string subFolder)
    {
        while (File.Exists(FolderPath(subFolder) + ogName))
        {
            int number = GetNumberAtTheEnd(ogName);
            if (number == 0) ogName = ogName.Replace(".", "1.");
            ogName = ogName.Replace(number.ToString(), (number + 1).ToString());
        }

        return ogName;
    }

    public static int GetNumberAtTheEnd(string txt)
    {
        txt = txt.Split('.')[0];

        if (!char.IsDigit(txt[^1])) return 0;

        for (int i = txt.Length - 2; i >= 0; i--)
            if (!char.IsDigit(txt[i]))
                return int.Parse(txt.Substring(i + 1));

        return 0;
    }
}

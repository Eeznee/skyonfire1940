using System.Collections;
using System.Collections.Generic;
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
	public static void Save(Texture2D texture, string subFolder)
	{
		Save(texture, subFolder, texture.name);
	}
	public static void Save(Texture2D texture, string subFolder, string overrideName)
	{
		byte[] png = texture.EncodeToPNG();
		Directory.CreateDirectory(FolderPath(subFolder));
		File.WriteAllBytes(FolderPath(subFolder) + overrideName, png);
	}
	public static Texture2D Load(string path)
	{
		Texture2D texture = new Texture2D(69, 420);
		texture.LoadImage(File.ReadAllBytes(path));
		return texture;
	}
	public static Texture2D Load(string subFolder,string name)
	{
		return Load(FolderPath(subFolder) + name);
	}
	public static void Remove(string subFolder,string name)
    {
		File.Delete(FolderPath(subFolder) + name);
    }
	public static void Pick(string subfolder, TexturesDropdown dropdown)
	{
		Texture2D texture = null;
		NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
		{
			if (path != null)
			{
				texture = NativeGallery.LoadImageAtPath(path, 4096, false);
				if (texture == null)
					return;
				texture.name = NameFromPath(path);
				Save(texture, subfolder);
				dropdown.Reset(texture.name);
				// If a procedural texture is not destroyed manually, 
				// it will only be freed after a scene change
				//Destroy(texture, 5f);
			}
		});
	}
	public static void ChangeAircraftTexture(SofAircraft aircraft, string textureName)
	{
		Texture2D texture = Load(FolderPath(aircraft.card.fileName) + textureName);

		MeshRenderer refRenderer = aircraft.GetComponentInChildren<Fuselage>().GetComponent<MeshRenderer>();
		Material refMaterial = refRenderer.sharedMaterial;

		Material changedMaterial = refRenderer.material;
		changedMaterial.mainTexture = texture;
		changedMaterial.name = "Custom Material";

		MeshRenderer[] renderers = aircraft.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in renderers)
		{
			if (renderer.sharedMaterial == refMaterial)
				renderer.sharedMaterial = changedMaterial;
		}
	}
}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FolderColor
{
public class ProjectAssetViewerCustomisation
{
	// Reference to the data
	public static AssetModificationData modificationData = new();

	[InitializeOnLoadMethod]
	private static void Initialize()
	{
		LoadData();

		//for each object
		EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
	}

	private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
	{
		var assetPath = AssetDatabase.GUIDToAssetPath(guid);

		// Ensure assetType is not null before accessing it
		if (modificationData.assetModified != null &&
		    modificationData.assetModified.Contains(assetPath))
		{
			var t = modificationData.assetModified.IndexOf(assetPath);

			var tex = (Texture2D)AssetDatabase.LoadAssetAtPath(
				modificationData.assetModifiedTexturePath[t],
				typeof(Texture2D));

			if (tex == null)
			{
				modificationData.assetModified.RemoveAt(t);
				modificationData.assetModifiedTexturePath.RemoveAt(t);
				SaveData();
				return;
			}

			if (selectionRect.height == 16)
				GUI.DrawTexture(
					new Rect(selectionRect.x + 1.5f, selectionRect.y,
						selectionRect.height, selectionRect.height), tex);
			else
				GUI.DrawTexture(
					new Rect(selectionRect.x, selectionRect.y,
						selectionRect.height - 10, selectionRect.height - 10),
					tex);
		}
	}

	// Add a menu item in the Unity Editor to open the custom modification window
	[MenuItem("Assets/Custom Folder", false, 100)]
	private static void CustomModificationMenuItem()
	{
		var guids = Selection.assetGUIDs;
		for (var i = 0; i < guids.Length; i++)
		{
			var guid = guids[i];
			var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			if (AssetDatabase.IsValidFolder(assetPath))
			{
				CustomWindowFileImage.ShowWindow(assetPath);
				break;
			}
		}
	}

	// Validate function to enable/disable the menu item
	[MenuItem("Assets/Custom Folder", true)]
	private static bool ValidateCustomModificationMenuItem()
	{
		var guids = Selection.assetGUIDs;
		for (var i = 0; i < guids.Length; i++)
		{
			var guid = guids[i];
			var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			if (AssetDatabase.IsValidFolder(assetPath)) return true;
		}

		return false;
	}

	public static void SaveData()
	{
		// Create or update the modificationData
		if (modificationData == null)
			modificationData = new AssetModificationData();

		// Convert to JSON
		var jsonData = JsonUtility.ToJson(modificationData);

		var path = FindScriptPathByName("ProjectAssetViewerCustomisation");
		path = path.Replace("Editor/ProjectAssetViewerCustomisation.cs",
			"SaveSetUp/FolderModificationData.json");

		File.WriteAllText(path, jsonData);
	}

	private static void LoadData()
	{
		var filePath = FindScriptPathByName("ProjectAssetViewerCustomisation");
		filePath = filePath.Replace("Editor/ProjectAssetViewerCustomisation.cs",
			"SaveSetUp/FolderModificationData.json");

		if (File.Exists(filePath))
		{
			var jsonData = File.ReadAllText(filePath);

			modificationData =
				JsonUtility.FromJson<AssetModificationData>(jsonData);
		}
	}

	public static string FindScriptPathByName(string scriptName)
	{
		var guids = AssetDatabase.FindAssets($"{scriptName} t:script");

		if (guids.Length == 0)
		{
			Debug.LogError($"Script with name '{scriptName}' not found!");
			return null;
		}

		var path = AssetDatabase.GUIDToAssetPath(guids[0]);
		return path;
	}

	[Serializable]
	public class AssetModificationData
	{
		public List<string> assetModified = new();
		public List<string> assetModifiedTexturePath = new();
	}
}
}
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace FolderColor
{
public class CustomWindowFileImage : EditorWindow
{
	private string assetPath;

	private void OnGUI()
	{
		if (GUI.Button(new Rect(0, 0, 100, 100), "None"))
		{
			if (ProjectAssetViewerCustomisation.modificationData.assetModified
			    .Contains(assetPath))
			{
				RemoveReference(assetPath);
				ProjectAssetViewerCustomisation.SaveData();
			}

			Close();
		}

		var path =
			ProjectAssetViewerCustomisation.FindScriptPathByName(
				"CustomWindowFileImage");
		path = path.Replace("/Editor/CustomWindowFileImage.cs", "");

		var texturesPath =
			AssetDatabase.FindAssets("t:texture2D", new[] { path });

		var buttonsPerRow = 4;
		var buttonPadding = 10f;

		for (var i = 0; i < texturesPath.Length; i++)
		{
			var texture = (Texture2D)AssetDatabase.LoadAssetAtPath(
				AssetDatabase.GUIDToAssetPath(texturesPath[i]),
				typeof(Texture2D));

			var buttonWidth =
				(position.width - (buttonsPerRow + 1) * buttonPadding) /
				buttonsPerRow;
			var buttonHeight = 100f;

			var x = i % buttonsPerRow * (buttonWidth + buttonPadding) +
			        buttonPadding;
			var y = Mathf.Floor(i / buttonsPerRow) *
				(buttonHeight + buttonPadding) + buttonPadding + 100;

			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), texture))
			{
				if (ProjectAssetViewerCustomisation.modificationData
				    .assetModified
				    .Contains(assetPath)) RemoveReference(assetPath);

				ProjectAssetViewerCustomisation.modificationData.assetModified
					.Add(assetPath);
				ProjectAssetViewerCustomisation.modificationData
					.assetModifiedTexturePath
					.Add(AssetDatabase.GUIDToAssetPath(texturesPath[i]));
				ProjectAssetViewerCustomisation.SaveData();

				Close();
			}
		}
	}

	public static void ShowWindow(string assetPathGive)
	{
		var window = GetWindow<CustomWindowFileImage>("Custom Folder");
		window.assetPath = assetPathGive;
		window.Show();
	}

	private static void RemoveReference(string assetPath)
	{
		var i = ProjectAssetViewerCustomisation.modificationData.assetModified
			.IndexOf(assetPath);
		ProjectAssetViewerCustomisation.modificationData.assetModified
			.RemoveAt(i);
		ProjectAssetViewerCustomisation.modificationData
			.assetModifiedTexturePath.RemoveAt(i);
	}
}
}
#endif
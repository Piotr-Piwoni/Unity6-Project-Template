using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
public class AssemblyDefUpdater : MonoBehaviour
{
	private const string HistoryFileName = "LastProductName.txt";

	[MenuItem("Tools/Update Assembly Definitions")]
	public static void UpdateAssemblyDefinitions()
	{
		// Get and clean the product name.
		var productName = PlayerSettings.productName.Replace(" ", string.Empty);

		var rootFolderPath = "Assets/_Root";
		var historyPath =
			Path.Combine(Application.dataPath, "Editor", HistoryFileName);

		if (!Directory.Exists(rootFolderPath))
		{
			Debug.LogError($"The folder {rootFolderPath} does not exist.");
			return;
		}

		// Load old name or fallback to "Project".
		var oldName = "Project";
		if (File.Exists(historyPath))
			oldName = File.ReadAllText(historyPath).Trim();

		var assemblyDefs = Directory.GetFiles(rootFolderPath, "*.asmdef",
			SearchOption.AllDirectories);
		var anyChanges = false;

		foreach (var assemblyDefPath in assemblyDefs)
		{
			var lines = File.ReadAllLines(assemblyDefPath);
			var changed = false;

			for (var i = 0; i < lines.Length; i++)
			{
				var trimmed = lines[i].TrimStart();

				if (trimmed.StartsWith("\"rootNamespace\""))
				{
					Match match = Regex.Match(lines[i],
						@"(""rootNamespace""\s*:\s*"")(.*?)(\"")");
					if (match.Success)
					{
						var currentNamespace = match.Groups[2].Value;
						if (currentNamespace.Contains(oldName))
						{
							var newNamespace =
								currentNamespace.Replace(oldName, productName);
							lines[i] = match.Groups[1].Value + newNamespace +
							           match.Groups[3].Value + ",";
							changed = true;
							anyChanges = true;
						}
					}
				}
			}

			if (changed)
			{
				File.WriteAllLines(assemblyDefPath, lines);
				Debug.Log($"Updated rootNamespace in: {assemblyDefPath}");
			}
			else
				Debug.Log("Nothing Happened...");
		}

		// Update stored old product name
		Directory.CreateDirectory(Path.GetDirectoryName(historyPath));
		File.WriteAllText(historyPath, productName);

		if (anyChanges)
		{
			AssetDatabase.Refresh();
			Debug.Log("AssetDatabase refreshed.");
		}
	}
}
}
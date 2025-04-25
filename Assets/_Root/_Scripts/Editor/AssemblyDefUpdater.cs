using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ProjectName.Editor
{
public class AssemblyDefUpdater : MonoBehaviour
{
	private const string HistoryFileName = "LastProductName.txt";

	[MenuItem("Tools/Update Assembly Definitions and Namespaces")]
	public static void UpdateAssemblyDefinitions()
	{
		var productName = PlayerSettings.productName.Replace(" ", string.Empty);
		var rootFolderPath = "Assets/_Root";
		var historyPath =
			Path.Combine(Application.dataPath, "_Root/_Scripts/Editor", HistoryFileName);

		if (!Directory.Exists(rootFolderPath))
		{
			Debug.LogError($"The folder {rootFolderPath} does not exist.");
			return;
		}

		// Load old name or default to "Project"
		var oldName = "Project";
		if (File.Exists(historyPath))
			oldName = File.ReadAllText(historyPath).Trim();

		var anyChanges = false;

		// --- Update Assembly Definitions ---
		var asmdefFiles = Directory.GetFiles(rootFolderPath, "*.asmdef",
			SearchOption.AllDirectories);

		foreach (var file in asmdefFiles)
		{
			var lines = File.ReadAllLines(file);
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
				File.WriteAllLines(file, lines);
				Debug.Log($"Updated rootNamespace in: {file}");
			}
		}

		// --- Update Namespaces in Scripts ---
		// This includes all scripts in _Root and all its subfolders.
		var scriptFiles = Directory.GetFiles(rootFolderPath, "*.cs",
			SearchOption.AllDirectories);

		foreach (var file in scriptFiles)
		{
			var content = File.ReadAllText(file);
			var originalContent = content;
			var changed = false;

			// Replace old name in using statements
			var usingPattern = new Regex(@"using\s+([\w\.]+);");
			content = usingPattern.Replace(content, match =>
			{
				var ns = match.Groups[1].Value;
				if (ns.Contains(oldName))
				{
					changed = true;
					var replaced = ns.Replace(oldName, productName);
					return $"using {replaced};";
				}

				return match.Value;
			});

			// Replace in namespace declarations
			Match namespaceMatch =
				Regex.Match(content, @"namespace\s+([\w\.]+)");
			if (namespaceMatch.Success)
			{
				var ns = namespaceMatch.Groups[1].Value;
				if (ns.Contains(oldName))
				{
					var updatedNs = ns.Replace(oldName, productName);
					content = content.Replace(ns, updatedNs);
					changed = true;
				}
			}

			if (changed && content != originalContent)
			{
				File.WriteAllText(file, content);
				Debug.Log(
					$"Updated using statements and/or namespace in: {file}");
				anyChanges = true;
			}
		}

		// --- Store new product name ---
		Directory.CreateDirectory(Path.GetDirectoryName(historyPath));
		File.WriteAllText(historyPath, productName);

		if (anyChanges)
		{
			AssetDatabase.Refresh();
			Debug.Log("AssetDatabase refreshed.");
		}
		else
			Debug.Log("No changes detected.");
	}
}
}
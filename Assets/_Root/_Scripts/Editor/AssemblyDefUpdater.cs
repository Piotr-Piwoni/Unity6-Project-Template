using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PROJECTNAME.Editor
{
public static class AssemblyDefUpdater
{
	private const string HistoryFileName = "LastProductName.txt";

	[MenuItem("Tools/Update Assembly Definitions and Namespaces")]
	public static void UpdateAssemblyDefinitions()
	{
		var productName = PlayerSettings.productName.Replace(" ", string.Empty);
		const string c_RootFolderPath = "Assets/_Root";
		var historyPath = Path.Combine(Application.dataPath,
			"_Root/_Scripts/Editor", HistoryFileName);

		if (!Directory.Exists(c_RootFolderPath))
		{
			Debug.LogError($"The folder {c_RootFolderPath} does not exist.");
			return;
		}

		// Load old name or default to "Project".
		var oldName = File.Exists(historyPath)
			? File.ReadAllText(historyPath).Trim()
			: "Project";

		var asmdefFiles = Directory.GetFiles(c_RootFolderPath, "*.asmdef",
			SearchOption.AllDirectories);
		var scriptFiles = Directory.GetFiles(c_RootFolderPath, "*.cs",
			SearchOption.AllDirectories);

		var changedAsmDefs = new ConcurrentBag<string>();
		var changedScripts = new ConcurrentBag<string>();
		var anyChanges = false;

		// --- Parallel update of Assembly Definitions ---
		Parallel.ForEach(asmdefFiles, file =>
		{
			var lines = File.ReadAllLines(file);
			var changed = false;

			for (var i = 0; i < lines.Length; i++)
			{
				var trimmed = lines[i].TrimStart();
				if (!trimmed.StartsWith("\"rootNamespace\"")) continue;

				// Find the "rootNamespace" field.
				Match match = Regex.Match(lines[i],
					@"(""rootNamespace""\s*:\s*"")(.*?)(\"")");

				if (!match.Success) continue;

				// Once found check its values and update it.
				var currentNamespace = match.Groups[2].Value;
				if (!currentNamespace.Contains(oldName)) continue;

				var newNamespace =
					currentNamespace.Replace(oldName, productName);
				lines[i] = match.Groups[1].Value + newNamespace +
				           match.Groups[3].Value + ",";
				changed = true;
			}

			if (!changed) return;
			File.WriteAllLines(file, lines);
			changedAsmDefs.Add(file);
		});

		// --- Parallel update of scripts ---
		Parallel.ForEach(scriptFiles, file =>
		{
			var content = File.ReadAllText(file);
			var originalContent = content;
			var changed = false;

			// Replace old name in USING statements.
			var usingPattern = new Regex(@"using\s+([\w\.]+);");
			content = usingPattern.Replace(content, match =>
			{
				var ns = match.Groups[1].Value;
				if (!ns.Contains(oldName)) return match.Value;
				changed = true;
				var replaced = ns.Replace(oldName, productName);
				return $"using {replaced};";
			});

			// Replace in namespace declarations.
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

			if (!changed || content == originalContent) return;
			File.WriteAllText(file, content);
			changedScripts.Add(file);
		});

		// --- Store new product name ---
		Directory.CreateDirectory(Path.GetDirectoryName(historyPath));
		File.WriteAllText(historyPath, productName);

		// --- Unity-safe updates ---
		foreach (var file in changedAsmDefs)
			Debug.Log($"Updated rootNamespace in: {file}");

		foreach (var file in changedScripts)
			Debug.Log($"Updated using statements and/or namespace in: {file}");

		anyChanges = changedAsmDefs.Count > 0 || changedScripts.Count > 0;

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
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PROJECTNAME.Editor.Utilities
{
public static class AssemblyDefUpdater
{
	private const string _HISTORY_FILE_NAME = "LastProductName.txt";

	[MenuItem("Tools/Update Assembly Definitions and Namespaces")]
	public static void UpdateAssemblyDefinitions()
	{
		// Remove whitespaces from project name.
		string productName =
			PlayerSettings.productName.Replace(" ", string.Empty);

		const string ROOT_FOLDER_PATH = "Assets/_Root";
		// Construct a path to the project name history file.
		string historyPath = Path.Combine(Application.dataPath,
										"_Root/_Scripts/Editor",
										_HISTORY_FILE_NAME);

		if (!Directory.Exists(ROOT_FOLDER_PATH))
		{
			Debug.LogError($"The folder {ROOT_FOLDER_PATH} does not exist!");
			return;
		}

		// Load an old name or default to "Project".
		string oldName = File.Exists(historyPath)
							? File.ReadAllText(historyPath).Trim()
							: "Project";

		// Lists of scripts and assembly definitions.
		string[] asmdefFiles = Directory.GetFiles(ROOT_FOLDER_PATH, "*.asmdef",
												SearchOption.AllDirectories);
		string[] scriptFiles = Directory.GetFiles(ROOT_FOLDER_PATH, "*.cs",
												SearchOption.AllDirectories);

		var changedAsmDefs = new ConcurrentBag<string>();
		var changedScripts = new ConcurrentBag<string>();
		var anyChanges = false;

		// --- Parallel Update of Assembly Definitions ---
		Parallel.ForEach(asmdefFiles, file =>
		{
			string[] lines = File.ReadAllLines(file);
			var changed = false;

			for (var i = 0; i < lines.Length; i++)
			{
				string trimmed = lines[i].TrimStart();
				if (!trimmed.StartsWith("\"rootNamespace\""))
					continue;

				// Find the "rootNamespace" field.
				Match match = Regex.Match(lines[i],
										@"(""rootNamespace""\s*:\s*"")(.*?)(\"")");

				if (!match.Success)
					continue;

				// Once found check its values and update it.
				string currentNamespace = match.Groups[2].Value;
				if (!currentNamespace.Contains(oldName))
					continue;

				string newNamespace = currentNamespace.Replace(oldName,
						productName);
				lines[i] = match.Groups[1].Value +
							newNamespace +
							match.Groups[3].Value + ",";
				changed = true;
			}

			if (!changed)
				return;

			File.WriteAllLines(file, lines);
			changedAsmDefs.Add(file);
		});

		// --- Parallel Update of Scripts ---
		Parallel.ForEach(scriptFiles, file =>
		{
			string content = File.ReadAllText(file);
			string originalContent = content;
			var changed = false;

			// Replace old name in USING statements.
			var usingPattern = new Regex(@"using\s+([\w\.]+);");
			content = usingPattern.Replace(content, match =>
			{
				string value = match.Groups[1].Value;
				if (!value.Contains(oldName))
					return match.Value;

				changed = true;
				string replaced = value.Replace(oldName, productName);
				return $"using {replaced};";
			});

			// Replace in namespace declarations.
			Match namespaceMatch = Regex.Match(content,
												@"namespace\s+([\w\.]+)");
			if (namespaceMatch.Success)
			{
				string Value = namespaceMatch.Groups[1].Value;
				if (Value.Contains(oldName))
				{
					string updatedNs = Value.Replace(oldName, productName);
					content = content.Replace(Value, updatedNs);
					changed = true;
				}
			}

			if (!changed || content == originalContent)
				return;
			File.WriteAllText(file, content);
			changedScripts.Add(file);
		});

		// Store the new project name.
		Directory.CreateDirectory(Path.GetDirectoryName(historyPath) ??
								string.Empty);
		File.WriteAllText(historyPath, productName);

		// Log updates.
		foreach (string file in changedAsmDefs)
			Debug.Log($"Updated rootNamespace in: {file}");

		foreach (string file in changedScripts)
			Debug.Log($"Updated using statements and/or namespace in: {file}");

		// Refresh the project.
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
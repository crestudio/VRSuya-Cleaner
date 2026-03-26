#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;

using VRSuya.Core;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class UnityLineCleaner : EditorWindow {

		class YamlFormattingRule {
			public string RuleDescription;
			public Regex CompiledSearchRegex;
			public string ReplacementTemplate;

			public YamlFormattingRule(string Description, string Pattern, string Template) {
				RuleDescription = Description;
				CompiledSearchRegex = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.Multiline);
				ReplacementTemplate = Template;
			}
		}

		static readonly List<YamlFormattingRule> FormattingRuleList = new List<YamlFormattingRule> {
			new YamlFormattingRule(
				"Merge Multi-line Object Reference",
				@"(\w+):\s*\{fileID:\s*(-?\d+),\s*guid:\s*([a-f0-9]+),\s+type:\s*(\d+)\}",
				"$1: {fileID: $2, guid: $3, type: $4}"
			),
			new YamlFormattingRule(
				"Merge Multi-line Object Reference (Alternative)",
				@"(\w+):\s*\{fileID:\s*(-?\d+),\s*\n?\s*guid:\s*([a-f0-9]+),",
				"$1: {fileID: $2, guid: $3,"
			)
		};

		[MenuItem("Assets/VRSuya/Fix YAML Broken Lines", true)]
		static bool ValidateAsset() {
			return Selection.objects
				.Select(Item => AssetDatabase.GetAssetPath(Item))
				.Select(Item => Item.EndsWith(".prefab") || Item.EndsWith(".unity"))
				.Contains(true);
		}

		[MenuItem("Assets/VRSuya/Fix YAML Broken Lines")]
		static void RequestFixSelectedAssets() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				Asset AssetInstance = new Asset();
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = AssetInstance.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Cleaning Broken YAML Formatting",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						if (FixYAMLBrokenLines(AssetDatabase.GUIDToAssetPath(AssetGUIDs[Index]))) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Cleaned up broken YAML lines in {ModifiedCount} files");
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Fix YAML Broken Lines", priority = 1100)]
		static void RequestFixAllAssets() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:Scene t:Prefab", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				Asset AssetInstance = new Asset();
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = AssetInstance.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Cleaning Broken YAML Formatting",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						if (FixYAMLBrokenLines(AssetDatabase.GUIDToAssetPath(AssetGUIDs[Index]))) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Cleaned up broken YAML lines in {ModifiedCount} files");
			}
		}

		public static bool FixYAMLBrokenLines(string FilePath) {
			string OriginalFileContent = File.ReadAllText(FilePath);
			if (OriginalFileContent.IndexOf("{fileID:", System.StringComparison.Ordinal) == -1) {
				return false;
			}
			string ModifiedFileContent = OriginalFileContent;
			foreach (YamlFormattingRule Rule in FormattingRuleList) {
				ModifiedFileContent = Rule.CompiledSearchRegex.Replace(ModifiedFileContent, Rule.ReplacementTemplate);
			}
			if (OriginalFileContent != ModifiedFileContent) {
				File.WriteAllText(FilePath, ModifiedFileContent);
				return true;
			}
			return false;
		}
	}
}
#endif
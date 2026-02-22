#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class UnityLineCleaner : EditorWindow {

		class YamlFormattingRule {
			public string RuleDescription;
			public string SearchRegexPattern;
			public string ReplacementTemplate;

			public YamlFormattingRule(string Description, string Pattern, string Template) {
				RuleDescription = Description;
				SearchRegexPattern = Pattern;
				ReplacementTemplate = Template;
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Fix YAML Broken Lines", priority = 1100)]
		static void ExecuteComprehensiveYamlCleanup() {
			string ProjectAssetsDirectoryPath = Application.dataPath;
			List<YamlFormattingRule> FormattingRuleList = new List<YamlFormattingRule> {
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
			string[] TargetFileExtensions = { "*.unity", "*.prefab" };
			List<string> AllTargetFiles = new List<string>();
			foreach (string Extension in TargetFileExtensions) {
				AllTargetFiles.AddRange(Directory.GetFiles(ProjectAssetsDirectoryPath, Extension, SearchOption.AllDirectories));
			}
			int TotalProcessedFileCount = AllTargetFiles.Count;
			int TotalModifiedFileCount = 0;
			try {
				for (int Index = 0; Index < TotalProcessedFileCount; Index++) {
					string CurrentFilePath = AllTargetFiles[Index];
					EditorUtility.DisplayProgressBar("Cleaning Broken YAML Formatting",
						$"Processing : {Path.GetFileName(CurrentFilePath)}",
						(float)Index / TotalProcessedFileCount);
					if (ApplyFormattingRulesToSingleFile(CurrentFilePath, FormattingRuleList)) {
						TotalModifiedFileCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				AssetDatabase.Refresh();
			}
			Debug.Log($"[VRSuya] {TotalModifiedFileCount}개 파일이 정리되었습니다");
			EditorUtility.DisplayDialog("UnityLineCleaner", $"{TotalModifiedFileCount}개의 파일이 성공적으로 정리되었습니다", "확인");
		}

		static bool ApplyFormattingRulesToSingleFile(string FilePath, List<YamlFormattingRule> RuleList) {
			string OriginalFileContent = File.ReadAllText(FilePath);
			string ModifiedFileContent = OriginalFileContent;
			foreach (YamlFormattingRule Rule in RuleList) {
				ModifiedFileContent = Regex.Replace(ModifiedFileContent, Rule.SearchRegexPattern, Rule.ReplacementTemplate);
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
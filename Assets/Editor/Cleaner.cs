using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using VRSuya.Core;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Cleaner {

	public class Cleaner : EditorWindow {

		[MenuItem("Assets/VRSuya/Clean up Prefab", true)]
		static bool ValidatePrefab() {
			return Asset.ContainPrefab(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Clean up Prefab", priority = 1100)]
		static void RequestCleanupPrefab() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = Asset.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Clean up Prefab",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						string TargetAssetPath = AssetDatabase.GUIDToAssetPath(AssetGUIDs[Index]);
						GameObject TargetGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(TargetAssetPath);
						List<bool> Results = new List<bool>();
						Results.Add(PrefabPhysBoneCleaner.ClosePhysBoneComponent(TargetGameObject));
						Results.Add(PrefabCleaner.ClearPrefabObjectRecursively(TargetGameObject));
						Results.Add(UnityLineCleaner.FixYAMLBrokenLines(TargetAssetPath));
						if (Results.Contains(true)) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					if (ModifiedCount > 0) {
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
					}
				}
				Debug.Log($"[VRSuya] Cleaned up of {ModifiedCount} Prefab");
			}
		}

		[MenuItem("Assets/VRSuya/Clean up Scene", true)]
		static bool ValidateScene() {
			return Asset.ContainScene(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Clean up Scene", priority = 1100)]
		static void RequestCleanupScene() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = Asset.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Clean up Scene",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						string TargetAssetPath = AssetDatabase.GUIDToAssetPath(AssetGUIDs[Index]);
						Object TargetObject = AssetDatabase.LoadAssetAtPath<Object>(TargetAssetPath);
						List<bool> Results = new List<bool>();
						Results.Add(UnityCleaner.StandardizeIndirectSpecularColor(AssetGUIDs[Index]));
						Results.Add(PrefabCleaner.ClearSceneObject(TargetObject));
						Results.Add(UnityLineCleaner.FixYAMLBrokenLines(TargetAssetPath));
						if (Results.Contains(true)) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					if (ModifiedCount > 0) {
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
					}
				}
				Debug.Log($"[VRSuya] Cleaned up of {ModifiedCount} Scene");
			}
		}

		[MenuItem("Assets/VRSuya/Clean up Animator", true)]
		static bool ValidateAnimator() {
			return Asset.ContainAnimatorController(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Clean up Animator", priority = 1100)]
		static void RequestCleanupAnimator() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				AnimatorControllerCleaner CleanerInstance = new AnimatorControllerCleaner();
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = Asset.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Clean up Animator",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						string TargetAssetPath = AssetDatabase.GUIDToAssetPath(AssetGUIDs[Index]);
						AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetPath);
						List<bool> Results = new List<bool>();
						Results.Add(UnityCleaner.StandardizefileID(TargetAnimator));
						Results.Add(CleanerInstance.CleanupAnimatorController(TargetAnimator));
						Results.Add(UnityLineCleaner.FixYAMLBrokenLines(TargetAssetPath));
						if (Results.Contains(true)) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					if (ModifiedCount > 0) {
						AssetDatabase.Refresh();
					}
				}
				Debug.Log($"[VRSuya] Cleaned up of {ModifiedCount} Animator Controller");
			}
		}
	}
}
using System.Text.RegularExpressions;
using System.IO;

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using static VRSuya.Core.Asset.AssetController;

/*
 * VRSuya Unity Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.animationcleaner {
	
	[ExecuteInEditMode]
    public class UnityCleanerEditor : EditorWindow {

		[MenuItem("Tools/VRSuya/Unity Cleaner/Standardize fileID", priority = 2000)]
		static void StandardizefileID() {
			string[] AnimatorControllerGUIDs = GetAssetGUIDs(AssetType.AnimatorController);
			int ChangedCount = 0;
			foreach (string AnimatorControllerGUID in AnimatorControllerGUIDs) {
				AnimatorController TargetAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(AnimatorControllerGUID));
				if (TargetAnimatorController && TargetAnimatorController.GetType() == typeof(AnimatorController)) {
					string RawAnimatorController = File.ReadAllText(AssetDatabase.GUIDToAssetPath(AnimatorControllerGUID));
					if (RawAnimatorController.Contains("m_Controller: {fileID: 9100000}")) {
						string newRawAnimatorController = RawAnimatorController.Replace("m_Controller: {fileID: 9100000}", "m_Controller: {fileID: 0}");
						File.WriteAllText(AssetDatabase.GUIDToAssetPath(AnimatorControllerGUID), newRawAnimatorController);
						ChangedCount++;
					}
				}
			}
			Debug.Log("[UnityCleaner] Normalized the fileIDs of " + ChangedCount + " Animator Controllers");
			return;
		}

		[MenuItem("Tools/VRSuya/Unity Cleaner/Standardize IndirectSpecularColor", priority = 2000)]
		static void StandardizeIndirectSpecularColor() {
			string[] SceneGUIDs = GetAssetGUIDs(AssetType.Scene);
			string NewValue = "m_IndirectSpecularColor: {r: 0, g: 0, b: 0, a: 1}";
			int ChangedCount = 0;
			foreach (string SceneGUID in SceneGUIDs) {
				string RawScene = File.ReadAllText(AssetDatabase.GUIDToAssetPath(SceneGUID));
				string Pattern = $@"{"m_IndirectSpecularColor"}:\s*{{[^}}]*}}";
				if (Regex.IsMatch(RawScene, Pattern)) {
					string newRawScene = Regex.Replace(RawScene, Pattern, NewValue);
					if (RawScene != newRawScene) {
						File.WriteAllText(AssetDatabase.GUIDToAssetPath(SceneGUID), newRawScene);
						ChangedCount++;
					}
				}
			}
			Debug.Log("[UnityCleaner] Normalized the IndirectSpecularColors of " + ChangedCount + " Unity Scenes");
			return;
		}
	}
}


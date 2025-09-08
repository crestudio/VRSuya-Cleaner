using System.Text.RegularExpressions;
using System.IO;

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using VRSuya.Core;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.cleaner {
	
	[ExecuteInEditMode]
    public class UnityCleaner : EditorWindow {

		[MenuItem("Tools/VRSuya/Cleaner/Standardize fileID", priority = 1100)]
		public static void StandardizefileID() {
			Asset AssetInstance = new Asset();
			string[] AnimatorControllerGUIDs = AssetInstance.GetAssetGUIDs(Asset.AssetType.AnimatorController);
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
			Debug.Log($"[VRSuya] Normalized the fileIDs of {ChangedCount} Animator Controllers");
			return;
		}

		[MenuItem("Tools/VRSuya/Cleaner/Standardize IndirectSpecularColor", priority = 1100)]
		public static void StandardizeIndirectSpecularColor() {
			Asset AssetInstance = new Asset();
			string[] SceneGUIDs = AssetInstance.GetAssetGUIDs(Asset.AssetType.Scene);
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
			Debug.Log($"[VRSuya] Normalized the IndirectSpecularColors of {ChangedCount} Unity Scenes");
			return;
		}
	}

	public static class PrefabTransformCleanerContextMenu {
		private static readonly float Tolerance = 0.01f;
		private static readonly string[] TransformProperties = { "m_LocalPosition", "m_LocalRotation", "m_LocalScale", "m_LocalEulerAnglesHint" };

		[MenuItem("Assets/VRSuya/Clear Transform Overrides")]
		private static void RequestClearPrefabTransform() {
			foreach (Object TargetObject in Selection.objects) {
				GameObject TargetGameObject = TargetObject as GameObject;
				if (TargetGameObject && TargetGameObject.GetType() == typeof(GameObject)) {
					if (PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) ClearPrefabObject(TargetGameObject);
				}
			}
			return;
		}

		private static void ClearPrefabObject(GameObject TargetGameObject) {
			bool IsChanged = false;
			Transform[] TargetTransforms = TargetGameObject.GetComponentsInChildren<Transform>(true);
			foreach (Transform TargetTransform in TargetTransforms) {
				Transform OriginalTransform = PrefabUtility.GetCorrespondingObjectFromSource(TargetTransform);
				if (!OriginalTransform) continue;
				if (NeedRevert(TargetTransform, OriginalTransform)) {
					PrefabUtility.RevertObjectOverride(TargetTransform, InteractionMode.AutomatedAction);
					Debug.Log($"[VRSuya] Reverted {TargetTransform.name} on {TargetGameObject.name}");
					IsChanged = true;
				}
			}
			if (IsChanged) {
				EditorUtility.SetDirty(TargetGameObject);
				AssetDatabase.SaveAssetIfDirty(TargetGameObject);
			}
			return;
		}

		private static bool NeedRevert(Transform TargetTransform, Transform OriginalTransform) {
			bool NeedRevert = true;
			if (Vector3.Distance(TargetTransform.localPosition, OriginalTransform.localPosition) >= Tolerance) NeedRevert = false;
			if (Quaternion.Angle(TargetTransform.localRotation, OriginalTransform.localRotation) >= Tolerance) NeedRevert = false;
			if (Vector3.Distance(TargetTransform.localScale, OriginalTransform.localScale) >= Tolerance) NeedRevert = false;
			return NeedRevert;
		}
	}
}


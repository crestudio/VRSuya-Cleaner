using System;
using System.IO;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using VRSuya.Core;
using Object = UnityEngine.Object;

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

	public static class PrefabTransformCleaner {

		private static readonly float Tolerance = 0.01f;

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
			if (!PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) return;
			bool IsChanged = false;
			PropertyModification[] PropertyModifications = PrefabUtility.GetPropertyModifications(TargetGameObject);
			foreach (PropertyModification TargetPropertyModification in PropertyModifications) {
				if (IsTransformProperty(TargetPropertyModification.propertyPath)) {
					Transform SourceTransform = (Transform)TargetPropertyModification.target;
					Transform OverriddenTransform = GetOverridenTransform(TargetGameObject, SourceTransform);
					string TargetPropertyPath = TargetPropertyModification.propertyPath;
					float TargetValue = float.Parse(TargetPropertyModification.value);
					if (SourceTransform && OverriddenTransform) {
						if (NeedRevert(SourceTransform, TargetPropertyPath, TargetValue)) {
							SerializedObject SerializedTransform = new SerializedObject(OverriddenTransform);
							SerializedProperty TargetProperty = SerializedTransform.FindProperty(TargetPropertyPath);
							PrefabUtility.RevertPropertyOverride(TargetProperty, InteractionMode.AutomatedAction);
							Debug.Log($"[VRSuya] Reverted {TargetPropertyPath} of {OverriddenTransform.name} transform on {TargetGameObject.name}");
							IsChanged = true;
						}
					}
				}
			}
			if (IsChanged) {
				EditorUtility.SetDirty(TargetGameObject);
				AssetDatabase.SaveAssetIfDirty(TargetGameObject);
			}
			return;
		}

		private static Transform GetOverridenTransform(GameObject TargetGameObject, Transform SourceTransform) {
			Transform[] ChildTransforms = TargetGameObject.GetComponentsInChildren<Transform>(true);
			return Array.Find(ChildTransforms, Item => Item.name == SourceTransform.name);
		}

		private static bool NeedRevert(Transform TargetTransform, string TargetPropertyPath, float TargetValue) {
			float OriginalValue = float.NaN;
			switch (TargetPropertyPath) {
				case "m_LocalPosition.x":
					OriginalValue = TargetTransform.localPosition.x;
					break;
				case "m_LocalPosition.y":
					OriginalValue = TargetTransform.localPosition.y;
					break;
				case "m_LocalPosition.z":
					OriginalValue = TargetTransform.localPosition.z;
					break;
				case "m_LocalRotation.w":
					OriginalValue = TargetTransform.localRotation.w;
					break;
				case "m_LocalRotation.x":
					OriginalValue = TargetTransform.localRotation.x;
					break;
				case "m_LocalRotation.y":
					OriginalValue = TargetTransform.localRotation.y;
					break;
				case "m_LocalRotation.z":
					OriginalValue = TargetTransform.localRotation.z;
					break;
				case "m_LocalEulerAnglesHint.x":
					OriginalValue = TargetTransform.localEulerAngles.x;
					break;
				case "m_LocalEulerAnglesHint.y":
					OriginalValue = TargetTransform.localEulerAngles.y;
					break;
				case "m_LocalEulerAnglesHint.z":
					OriginalValue = TargetTransform.localEulerAngles.z;
					break;
				case "m_LocalScale.x":
					OriginalValue = TargetTransform.localScale.x;
					break;
				case "m_LocalScale.y":
					OriginalValue = TargetTransform.localScale.y;
					break;
				case "m_LocalScale.z":
					OriginalValue = TargetTransform.localScale.z;
					break;
			}
			if (OriginalValue != float.NaN) {
				if (Math.Abs(OriginalValue - TargetValue) <= Tolerance) {
					return true;
				} else {
					return false;
				}
			} else {
				return true;
			}
		}

		/// <summary>PropertyPath가 Transform 관련 속성인지 확인합니다.</summary>
		/// <param name="TargetPropertyPath">Property 경로</param>
		/// <returns>Transform 속성 여부</returns>
		private static bool IsTransformProperty(string TargetPropertyPath) {
			return TargetPropertyPath.StartsWith("m_LocalPosition") ||
				   TargetPropertyPath.StartsWith("m_LocalRotation") ||
				   TargetPropertyPath.StartsWith("m_LocalScale") ||
				   TargetPropertyPath.StartsWith("m_LocalEulerAnglesHint");
		}
	}
}


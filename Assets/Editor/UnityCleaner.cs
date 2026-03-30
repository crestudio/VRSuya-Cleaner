using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using VRSuya.Core;
using Object = UnityEngine.Object;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.cleaner {
	
	[ExecuteInEditMode]
    public class UnityCleaner : EditorWindow {

		[MenuItem("Assets/VRSuya/Animator/Standardize fileID", true)]
		static bool ValidateAnimator() {
			Asset AssetInstance = new Asset();
			return AssetInstance.ContainAnimatorController(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Animator/Standardize fileID", priority = 1000)]
		static void RequestStandardizefileID() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				Asset AssetInstance = new Asset();
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = AssetInstance.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Standardizing fileID",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						string TargetAssetPath = AssetDatabase.GUIDToAssetPath(AssetGUIDs[Index]);
						AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetPath);
						if (StandardizefileID(TargetAnimator)) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Normalized the fileIDs of {ModifiedCount} Animator Controllers");
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Standardize fileID", priority = 1000)]
		static void RequestAllStandardizefileID() {
			Asset AssetInstance = new Asset();
			string[] AnimatorControllerGUIDs = AssetInstance.GetAssetGUIDs(Asset.AssetType.AnimatorController);
			if (AnimatorControllerGUIDs.Length > 0) {
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AnimatorControllerGUIDs.Length; Index++) {
						string TargetAssetName = AssetInstance.GUIDToAssetName(AnimatorControllerGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Standardizing fileID",
							$"Processing : {TargetAssetName}",
							(float)Index / AnimatorControllerGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						string TargetAssetPath = AssetDatabase.GUIDToAssetPath(AnimatorControllerGUIDs[Index]);
						AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetPath);
						if (StandardizefileID(TargetAnimator)) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Normalized the fileIDs of {ModifiedCount} Animator Controllers");
			}
		}

		public static bool StandardizefileID(AnimatorController TargetAnimator) {
			if (TargetAnimator && TargetAnimator is AnimatorController) {
				string TargetAssetPath = AssetDatabase.GetAssetPath(TargetAnimator);
				string RawAnimatorController = File.ReadAllText(TargetAssetPath);
				if (RawAnimatorController.Contains("m_Controller: {fileID: 0}")) {
					string newRawAnimatorController = RawAnimatorController.Replace("m_Controller: {fileID: 0}", "m_Controller: {fileID: 9100000}");
					File.WriteAllText(TargetAssetPath, newRawAnimatorController);
					return true;
				}
			}
			return false;
		}

		[MenuItem("Assets/VRSuya/Scene/Standardize IndirectSpecularColor", true)]
		static bool ValidateScene() {
			Asset AssetInstance = new Asset();
			return AssetInstance.ContainScene(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Scene/Standardize IndirectSpecularColor", priority = 1000)]
		static void RequestStandardizeIndirectSpecularColor() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				Asset AssetInstance = new Asset();
				int ModifiedCount = 0;
				try {
					for (int Index = 0; Index < AssetGUIDs.Length; Index++) {
						string TargetAssetName = AssetInstance.GUIDToAssetName(AssetGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Standardizing fileID",
							$"Processing : {TargetAssetName}",
							(float)Index / AssetGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						if (StandardizeIndirectSpecularColor(AssetGUIDs[Index])) {
							ModifiedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Normalized the IndirectSpecularColors of {ModifiedCount} Unity Scenes");
			}
		}


		[MenuItem("Tools/VRSuya/Cleaner/Scene/Standardize IndirectSpecularColor", priority = 1000)]
		static void RequestStandardizeAllIndirectSpecularColor() {
			Asset AssetInstance = new Asset();
			string[] SceneGUIDs = AssetInstance.GetAssetGUIDs(Asset.AssetType.Scene);
			if (SceneGUIDs.Length > 0) {
				int ModifiedCount = 0;
				foreach (string SceneGUID in SceneGUIDs) {
					if (StandardizeIndirectSpecularColor(SceneGUID)) {
						ModifiedCount++;
					}
				}
				Debug.Log($"[VRSuya] Normalized the IndirectSpecularColors of {ModifiedCount} Unity Scenes");
			}
		}

		public static bool StandardizeIndirectSpecularColor(string TargetSceneGUID) {
			string RawScene = File.ReadAllText(AssetDatabase.GUIDToAssetPath(TargetSceneGUID));
			string Pattern = $@"{"m_IndirectSpecularColor"}:\s*{{[^}}]*}}";
			string NewValue = "m_IndirectSpecularColor: {r: 0, g: 0, b: 0, a: 1}";
			if (Regex.IsMatch(RawScene, Pattern)) {
				string newRawScene = Regex.Replace(RawScene, Pattern, NewValue);
				if (RawScene != newRawScene) {
					File.WriteAllText(AssetDatabase.GUIDToAssetPath(TargetSceneGUID), newRawScene);
					return true;
				}
			}
			return false;
		}
	}

	public static class PrefabTransformCleaner {

		const float Tolerance = 0.001f;

		[MenuItem("Assets/VRSuya/Prefab/Clear Prefab Overrides", true)]
		static bool ValidatePrefab() {
			Asset AssetInstance = new Asset();
			return AssetInstance.ContainPrefab(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Prefab/Clear Prefab Overrides", priority = 1000)]
		static void RequestClearPrefabTransform() {
			foreach (Object TargetObject in Selection.objects) {
				GameObject TargetGameObject = TargetObject as GameObject;
				if (TargetGameObject && TargetGameObject is GameObject) {
					if (PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) ClearPrefabObject(TargetGameObject);
				}
			}
		}

		[MenuItem("Assets/VRSuya/Scene/Clear Scene Overrides", true)]
		static bool ValidateScene() {
			Asset AssetInstance = new Asset();
			return AssetInstance.ContainScene(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Scene/Clear Scene Overrides", priority = 1000)]
		static void RequestClearSceneTransform() {
			foreach (Object TargetObject in Selection.objects) {
				if (TargetObject && AssetDatabase.GetAssetPath(TargetObject).EndsWith(".unity")) {
					string ScenePath = AssetDatabase.GetAssetPath(TargetObject);
					Scene TargetScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
					try {
						bool IsDirty = false;
						foreach (GameObject RootGameObject in TargetScene.GetRootGameObjects()) {
							if (ClearPrefabObject(RootGameObject)) IsDirty = true;
						}
						if (IsDirty) {
							EditorSceneManager.SaveScene(TargetScene);
						}
					} finally {
						EditorSceneManager.CloseScene(TargetScene, true);
					}
				}
			}
		}

		public static bool ClearPrefabObject(GameObject TargetGameObject) {
			bool IsChanged = false;
			int Count = 0;
			if (!PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) return IsChanged;
			PropertyModification[] PropertyModifications = PrefabUtility.GetPropertyModifications(TargetGameObject);
			if (PropertyModifications == null || PropertyModifications.Length == 0) return IsChanged;
			List<PropertyModification> ValidModifications = new List<PropertyModification>();
			foreach (PropertyModification TargetPropertyModification in PropertyModifications) {
				string TargetPropertyPath = TargetPropertyModification.propertyPath;
				Object SourcePrefabObject = TargetPropertyModification.target;
				bool ShouldRemoveModification = false;
				Object OverriddenInstanceObject = GetOriginalObject(TargetGameObject, SourcePrefabObject);
				if (SourcePrefabObject == null || OverriddenInstanceObject == null) {
					ShouldRemoveModification = true;
				} else {
					SerializedObject SerializedInstance = new SerializedObject(OverriddenInstanceObject);
					SerializedProperty TargetProperty = SerializedInstance.FindProperty(TargetPropertyPath);
					if (TargetProperty == null) {
						ShouldRemoveModification = true;
					} else {
						bool IsTransformOverride = IsTransformProperty(TargetPropertyPath);
						bool IsCacheOverride = IsCacheProperty(TargetPropertyPath);
						if (IsCacheOverride) {
							ShouldRemoveModification = true;
						} else if (IsTransformOverride && SourcePrefabObject is Transform SourceTransform) {
							float TargetValue = float.Parse(TargetPropertyModification.value, CultureInfo.InvariantCulture);
							if (NeedRevertTransform(SourceTransform, TargetPropertyPath, TargetValue)) {
								ShouldRemoveModification = true;
							}
						}
					}
				}
				if (ShouldRemoveModification) {
					IsChanged = true;
					Count++;
				} else {
					ValidModifications.Add(TargetPropertyModification);
				}
			}
			if (IsChanged) {
				PrefabUtility.SetPropertyModifications(TargetGameObject, ValidModifications.ToArray());
				EditorUtility.SetDirty(TargetGameObject);
				AssetDatabase.SaveAssetIfDirty(TargetGameObject);
				Debug.Log($"[VRSuya] Reverted/Removed {Count} overridden or orphaned properties on {TargetGameObject.name}");
			}
			return IsChanged;
		}

		/// <summary>Prefab 프로퍼티의 원본 Object를 찾습니다.</summary>
		static Object GetOriginalObject(GameObject TargetPrefabInstanceRoot, Object TargetPrefabObject) {
			if (TargetPrefabObject is Component TargetPrefabComponent) {
				Component[] InstanceComponents = TargetPrefabInstanceRoot.GetComponentsInChildren(TargetPrefabComponent.GetType(), true);
				foreach (Component InstanceComponent in InstanceComponents) {
					if (PrefabUtility.GetCorrespondingObjectFromSource(InstanceComponent) == TargetPrefabObject) {
						return InstanceComponent;
					}
				}
			} else if (TargetPrefabObject is GameObject TargetPrefabGameObject) {
				Transform[] InstanceTransforms = TargetPrefabInstanceRoot.GetComponentsInChildren<Transform>(true);
				foreach (Transform InstanceTransform in InstanceTransforms) {
					if (PrefabUtility.GetCorrespondingObjectFromSource(InstanceTransform.gameObject) == TargetPrefabObject) {
						return InstanceTransform.gameObject;
					}
				}
			}
			return null;
		}

		static bool NeedRevertTransform(Transform SourceTransform, string TargetPropertyPath, float TargetValue) {
			float OriginalValue = float.NaN;
			switch (TargetPropertyPath) {
				case "m_LocalPosition.x":
					OriginalValue = SourceTransform.localPosition.x;
					break;
				case "m_LocalPosition.y":
					OriginalValue = SourceTransform.localPosition.y;
					break;
				case "m_LocalPosition.z":
					OriginalValue = SourceTransform.localPosition.z;
					break;
				case "m_LocalRotation.w":
					OriginalValue = SourceTransform.localRotation.w;
					break;
				case "m_LocalRotation.x":
					OriginalValue = SourceTransform.localRotation.x;
					break;
				case "m_LocalRotation.y":
					OriginalValue = SourceTransform.localRotation.y;
					break;
				case "m_LocalRotation.z":
					OriginalValue = SourceTransform.localRotation.z;
					break;
				case "m_LocalEulerAnglesHint.x":
					OriginalValue = SourceTransform.localEulerAngles.x;
					break;
				case "m_LocalEulerAnglesHint.y":
					OriginalValue = SourceTransform.localEulerAngles.y;
					break;
				case "m_LocalEulerAnglesHint.z":
					OriginalValue = SourceTransform.localEulerAngles.z;
					break;
				case "m_LocalScale.x":
					OriginalValue = SourceTransform.localScale.x;
					break;
				case "m_LocalScale.y":
					OriginalValue = SourceTransform.localScale.y;
					break;
				case "m_LocalScale.z":
					OriginalValue = SourceTransform.localScale.z;
					break;
			}
			if (!float.IsNaN(OriginalValue)) {
				return Math.Abs(OriginalValue - TargetValue) <= Tolerance;
			} else {
				return true;
			}
		}

		/// <summary>PropertyPath가 Transform 관련 속성인지 확인합니다.</summary>
		/// <param name="TargetPropertyPath">Property 경로</param>
		/// <returns>Transform 속성 여부</returns>
		static bool IsTransformProperty(string TargetPropertyPath) {
			return TargetPropertyPath.StartsWith("m_LocalPosition") ||
				   TargetPropertyPath.StartsWith("m_LocalRotation") ||
				   TargetPropertyPath.StartsWith("m_LocalScale") ||
				   TargetPropertyPath.StartsWith("m_LocalEulerAnglesHint");
		}

		/// <summary>자동으로 생성되는 캐시 속성인지 반환합니다.</summary>
		static bool IsCacheProperty(string TargetPropertyPath) {
			if (TargetPropertyPath == "cachedExecutionGroupIndex" ||
				TargetPropertyPath == "latestValidExecutionGroupIndex" ||
				TargetPropertyPath == "unityVersion" ||
				TargetPropertyPath == "fallbackStatus" ||
				TargetPropertyPath == "completedSDKPipeline" ||
				TargetPropertyPath.StartsWith("animationHashSet") ||
				(TargetPropertyPath.StartsWith("baseAnimationLayers") && TargetPropertyPath.EndsWith("mask")) ||
				TargetPropertyPath.StartsWith("m_PositionOffset") ||
				TargetPropertyPath.StartsWith("m_PositionAtRest") ||
				TargetPropertyPath.StartsWith("m_RotationOffset") ||
				TargetPropertyPath.StartsWith("m_RotationAtRest") ||
				TargetPropertyPath.StartsWith("PositionOffset") ||
				TargetPropertyPath.StartsWith("PositionOffset") ||
				TargetPropertyPath.StartsWith("RotationOffset") ||
				TargetPropertyPath.StartsWith("RotationAtRest")) {
				return true;
			}
			return false;
		}
	}
}


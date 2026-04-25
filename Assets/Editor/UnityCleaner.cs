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

using VRC.SDK3.Dynamics.PhysBone.Components;

using VRSuya.Core;

using Object = UnityEngine.Object;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace VRSuya.Cleaner {
	
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

	public class PrefabPhysBoneCleaner {

		[MenuItem("Assets/VRSuya/Prefab/Close Prefab PhysBone", true)]
		static bool ValidatePrefab() {
			Asset AssetInstance = new Asset();
			return AssetInstance.ContainPrefab(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Prefab/Close Prefab PhysBone", priority = 1000)]
		static void RequestClearPrefabTransform() {
			foreach (Object TargetObject in Selection.objects) {
				GameObject TargetGameObject = TargetObject as GameObject;
				if (TargetGameObject && TargetGameObject is GameObject) {
					if (PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) ClosePhysBoneComponent(TargetGameObject);
				}
			}
		}

		static void ClosePhysBoneComponent(GameObject TargetGameObject) {
			VRCPhysBone[] PhysBoneComponents = TargetGameObject.GetComponentsInChildren<VRCPhysBone>(true);
			if (PhysBoneComponents.Length > 0) {
				bool IsPrefabModified = false;
				foreach (VRCPhysBone TargetPhysBone in PhysBoneComponents) {
					if (!PrefabUtility.GetCorrespondingObjectFromSource(TargetPhysBone)) {
						bool IsModified = false;
						if (TargetPhysBone.foldout_transforms) { TargetPhysBone.foldout_transforms = false; IsModified = true; }
						if (TargetPhysBone.foldout_forces) { TargetPhysBone.foldout_forces = false; IsModified = true; }
						if (TargetPhysBone.foldout_collision) { TargetPhysBone.foldout_collision = false; IsModified = true; }
						if (TargetPhysBone.foldout_stretchsquish) { TargetPhysBone.foldout_stretchsquish = false; IsModified = true; }
						if (TargetPhysBone.foldout_limits) { TargetPhysBone.foldout_limits = false; IsModified = true; }
						if (TargetPhysBone.foldout_grabpose) { TargetPhysBone.foldout_grabpose = false; IsModified = true; }
						if (TargetPhysBone.foldout_options) { TargetPhysBone.foldout_options = false; IsModified = true; }
						if (TargetPhysBone.foldout_gizmos) { TargetPhysBone.foldout_gizmos = false; IsModified = true; }
						if (IsModified) {
							IsPrefabModified = true;
							EditorUtility.SetDirty(TargetPhysBone);
						}
					}
				}
				if (IsPrefabModified) {
					AssetDatabase.SaveAssetIfDirty(TargetGameObject);
					Debug.Log($"[VRSuya] {TargetGameObject.name} 프리팹의 PhysBone이 모두 닫혔습니다");
				}
			}
		}

	}

	public class PrefabCleaner {

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
			if (!PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) return IsChanged;
			PropertyModification[] PropertyModifications = PrefabUtility.GetPropertyModifications(TargetGameObject);
			if (PropertyModifications == null || PropertyModifications.Length == 0) return IsChanged;
			List<PropertyModification> ValidModifications = new List<PropertyModification>();
			int RemovedCount = 0;
			foreach (PropertyModification TargetPropertyModification in PropertyModifications) {
				string TargetPropertyPath = TargetPropertyModification.propertyPath;
				Object SourcePrefabObject = TargetPropertyModification.target;
				bool NeedRemoved = false;
				if (SourcePrefabObject == null) {
					NeedRemoved = true;
				} else {
					bool IsCacheOverride = IsCacheProperty(TargetPropertyPath);
					bool IsTransformOverride = IsTransformProperty(TargetPropertyPath);
					if (IsCacheOverride) {
						NeedRemoved = true;
					} else if (IsTransformOverride && SourcePrefabObject is Transform SourcePrefabTransform) {
						if (float.TryParse(TargetPropertyModification.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float TargetValue)) {
							if (NeedRevertTransform(SourcePrefabTransform, TargetPropertyPath, TargetValue)) {
								NeedRemoved = true;
							}
						}
					}
				}
				if (NeedRemoved) {
					IsChanged = true;
					RemovedCount++;
				} else {
					ValidModifications.Add(TargetPropertyModification);
				}
			}
			if (IsChanged) {
				PrefabUtility.SetPropertyModifications(TargetGameObject, ValidModifications.ToArray());
				EditorUtility.SetDirty(TargetGameObject);
				AssetDatabase.SaveAssetIfDirty(TargetGameObject);
				Debug.Log($"[VRSuya] Reverted/Removed {RemovedCount} overridden or orphaned properties on {TargetGameObject.name}");
			}
			return IsChanged;
		}

		static bool NeedRevertTransform(Transform SourcePrefabTransform, string TargetPropertyPath, float TargetValue) {
			SerializedObject SerializedPrefabTransform = new SerializedObject(SourcePrefabTransform);
			SerializedProperty SourceProperty = SerializedPrefabTransform.FindProperty(TargetPropertyPath);
			if (SourceProperty == null) {
				return true;
			}
			if (SourceProperty.propertyType != SerializedPropertyType.Float) {
				return false;
			}
			float OriginalValue = SourceProperty.floatValue;
			return Math.Abs(OriginalValue - TargetValue) <= Tolerance;
		}

		static bool IsTransformProperty(string TargetPropertyPath) {
			return TargetPropertyPath.StartsWith("m_LocalPosition") ||
				   TargetPropertyPath.StartsWith("m_LocalRotation") ||
				   TargetPropertyPath.StartsWith("m_LocalScale") ||
				   TargetPropertyPath.StartsWith("m_LocalEulerAnglesHint");
		}

		static bool IsCacheProperty(string TargetPropertyPath) {
			if (TargetPropertyPath.StartsWith("cachedExecutionGroupIndex") ||
				TargetPropertyPath.StartsWith("latestValidExecutionGroupIndex") ||
				TargetPropertyPath.StartsWith("unityVersion.") ||
				TargetPropertyPath.StartsWith("fallbackStatus") ||
				TargetPropertyPath.StartsWith("completedSDKPipeline") ||
				TargetPropertyPath.StartsWith("animationHashSet") ||
				(TargetPropertyPath.StartsWith("baseAnimationLayers") && TargetPropertyPath.EndsWith("mask")) ||
				TargetPropertyPath.StartsWith("m_TranslationOffsets") ||
				TargetPropertyPath.StartsWith("m_TranslationAtRest") ||
				TargetPropertyPath.StartsWith("m_PositionOffset") ||
				TargetPropertyPath.StartsWith("m_PositionAtRest") ||
				TargetPropertyPath.StartsWith("m_RotationOffset") ||
				TargetPropertyPath.StartsWith("m_RotationAtRest") ||
				TargetPropertyPath.StartsWith("PositionOffset") ||
				TargetPropertyPath.StartsWith("PositionAtRest") ||
				TargetPropertyPath.StartsWith("RotationOffset") ||
				TargetPropertyPath.StartsWith("RotationAtRest") ||
				TargetPropertyPath.StartsWith("foldout_")) {
				return true;
			}
			return false;
		}
	}
}


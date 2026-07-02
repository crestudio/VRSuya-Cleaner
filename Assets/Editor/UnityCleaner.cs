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
	
    public class UnityCleaner : EditorWindow {

		[MenuItem("Assets/VRSuya/Animator/Standardize fileID", true)]
		static bool ValidateAnimator() {
			return AssetUtility.ContainAnimatorController(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Animator/Standardize fileID", priority = 1000)]
		static void RequestStandardizefileID() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				StandardizeAnimators(AssetGUIDs);
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Standardize fileID", priority = 1000)]
		static void RequestAllStandardizefileID() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				StandardizeAnimators(AssetGUIDs);
			}
		}

		static void StandardizeAnimators(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = AssetUtility.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Standardizing AnimatorController",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUIDs[Index]);
					AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetPath);
					if (StandardizefileID(TargetAnimator)) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				if (ModifiedCount > 0) {
					AssetDatabase.Refresh();
				}
			}
			Debug.Log($"[VRSuya] Normalized the fileIDs of {ModifiedCount} Animator Controllers");
		}

		public static bool StandardizefileID(AnimatorController TargetAnimator) {
			if (TargetAnimator && TargetAnimator is AnimatorController) {
				string TargetAssetPath = AssetDatabase.GetAssetPath(TargetAnimator);
				string RawAnimatorController = File.ReadAllText(TargetAssetPath);
				if (RawAnimatorController.Contains("m_Controller: {fileID: 0}")) {
					string NewRawAnimatorController = RawAnimatorController.Replace("m_Controller: {fileID: 0}", "m_Controller: {fileID: 9100000}");
					File.WriteAllText(TargetAssetPath, NewRawAnimatorController);
					return true;
				}
			}
			return false;
		}

		[MenuItem("Assets/VRSuya/Animator/Standardize Exit Time", true)]
		static bool ValidateAnimatorExitTime() {
			return AssetUtility.ContainAnimatorController(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Animator/Standardize Exit Time", priority = 1000)]
		static void RequestFixAnimatorExitTime() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				FixAnimatorTransitionExitTime(AssetGUIDs);
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Standardize Exit Time", priority = 1000)]
		static void RequestAllFixAnimatorExitTime() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				FixAnimatorTransitionExitTime(AssetGUIDs);
			}
		}

		static void FixAnimatorTransitionExitTime(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = AssetUtility.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Standardize Exit Time",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUIDs[Index]);
					AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetPath);
					if (FixTransitionExitTime(TargetAnimator)) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				if (ModifiedCount > 0) {
					AssetDatabase.Refresh();
				}
			}
			Debug.Log($"[VRSuya] Normalized the Transitions of {ModifiedCount} Animator Controllers");
		}

		public static bool FixTransitionExitTime(AnimatorController TargetAnimator) {
			if (TargetAnimator && TargetAnimator is AnimatorController) {
				AnimatorStateTransition[] AllTransition = AnimatorHelper.GetAllTransitions(TargetAnimator)
					.Where(Item => Item != null)
					.Where(Item => Item.hasExitTime == false)
					.Where(Item => Item.exitTime != 0f)
					.ToArray();
				if (AllTransition.Length > 0) {
					foreach (AnimatorStateTransition TargetTransition in AllTransition) {
						TargetTransition.exitTime = 0f;
						EditorUtility.SetDirty(TargetTransition);
					}
					return true;
				}
			}
			return false;
		}

		[MenuItem("Assets/VRSuya/Animator/Clear Transitions", true)]
		static bool ValidateAnimatorTransition() {
			return AssetUtility.ContainAnimatorController(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Animator/Clear Transitions", priority = 1000)]
		static void RequestFixAnimatorTransition() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				bool IsApproved = EditorUtility.DisplayDialog(
					"VRSuya Cleaner",
					"Are you sure clear all of transitions in Animator Controllers?",
					"Clear",
					"Cancel"
				);
				if (IsApproved) FixAnimatorTransitionTransition(AssetGUIDs);
			}
		}

		static void FixAnimatorTransitionTransition(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = AssetUtility.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Clear Transitions",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUIDs[Index]);
					AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetPath);
					if (ClearTransitions(TargetAnimator)) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				if (ModifiedCount > 0) {
					AssetDatabase.Refresh();
				}
			}
			Debug.Log($"[VRSuya] Cleared transitions of {ModifiedCount} Animator Controllers");
		}

		public static bool ClearTransitions(AnimatorController TargetAnimator) {
			if (TargetAnimator && TargetAnimator is AnimatorController) {
				AnimatorStateTransition[] AllTransition = AnimatorHelper.GetAllTransitions(TargetAnimator)
					.Where(Item => Item != null)
					.ToArray();
				if (AllTransition.Length > 0) {
					foreach (AnimatorStateTransition TargetTransition in AllTransition) {
						TargetTransition.hasExitTime = false;
						TargetTransition.exitTime = 0f;
						TargetTransition.hasFixedDuration = true;
						TargetTransition.duration = 0f;
						TargetTransition.offset = 0f;
						TargetTransition.interruptionSource = TransitionInterruptionSource.None;
						TargetTransition.orderedInterruption = true;
						TargetTransition.canTransitionToSelf = false;
						EditorUtility.SetDirty(TargetTransition);
					}
					return true;
				}
			}
			return false;
		}


		[MenuItem("Assets/VRSuya/Scene/Standardize IndirectSpecularColor", true)]
		static bool ValidateScene() {
			return AssetUtility.ContainScene(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Scene/Standardize IndirectSpecularColor", priority = 1000)]
		static void RequestStandardizeIndirectSpecularColor() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				StandardizeScenes(AssetGUIDs);
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Scene/Standardize IndirectSpecularColor", priority = 1000)]
		static void RequestStandardizeAllIndirectSpecularColor() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				StandardizeScenes(AssetGUIDs);
			}
		}

		static void StandardizeScenes(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = AssetUtility.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Standardizing Scene",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					if (StandardizeIndirectSpecularColor(TargetGUIDs[Index])) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				if (ModifiedCount > 0) {
					AssetDatabase.Refresh();
				}
			}
			Debug.Log($"[VRSuya] Normalized the IndirectSpecularColors of {ModifiedCount} Unity Scenes");
		}

		public static bool StandardizeIndirectSpecularColor(string TargetSceneGUID) {
			string RawScene = File.ReadAllText(AssetDatabase.GUIDToAssetPath(TargetSceneGUID));
			string Pattern = $@"{"m_IndirectSpecularColor"}:\s*{{[^}}]*}}";
			string NewValue = "m_IndirectSpecularColor: {r: 0, g: 0, b: 0, a: 1}";
			if (Regex.IsMatch(RawScene, Pattern)) {
				string NewRawScene = Regex.Replace(RawScene, Pattern, NewValue);
				if (RawScene != NewRawScene) {
					File.WriteAllText(AssetDatabase.GUIDToAssetPath(TargetSceneGUID), NewRawScene);
					return true;
				}
			}
			return false;
		}
	}

	public class PrefabPhysBoneCleaner {

		[MenuItem("Assets/VRSuya/Prefab/Close Prefab PhysBone", true)]
		static bool ValidatePrefab() {
			return AssetUtility.ContainPrefab(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Prefab/Close Prefab PhysBone", priority = 1000)]
		static void RequestClearPrefabTransform() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				StandardizePrefabs(AssetGUIDs);
			}
		}

		static void StandardizePrefabs(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = AssetUtility.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Closing PhysBones in Prefab",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUIDs[Index]);
					GameObject TargetGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(TargetAssetPath);
					if (ClosePhysBoneComponent(TargetGameObject)) {
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
			Debug.Log($"[VRSuya] Closed the PhysBone components of {ModifiedCount} Prefabs");
		}

		public static bool ClosePhysBoneComponent(GameObject TargetGameObject) {
			if (PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) {
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
						return true;
					}
				}
			}
			return false;
		}
	}

	public class PrefabCleaner {

		const float Tolerance = 0.001f;

		[MenuItem("Assets/VRSuya/Prefab/Clear Prefab Overrides", true)]
		static bool ValidatePrefab() {
			return AssetUtility.ContainPrefab(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Prefab/Clear Prefab Overrides", priority = 1000)]
		static void RequestClearPrefabTransform() {
			foreach (Object TargetObject in Selection.objects) {
				GameObject TargetGameObject = TargetObject as GameObject;
				if (TargetGameObject && TargetGameObject is GameObject) {
					if (PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) ClearPrefabObjectRecursively(TargetGameObject);
				}
			}
		}

		[MenuItem("Assets/VRSuya/Scene/Clear Scene Overrides", true)]
		static bool ValidateScene() {
			return AssetUtility.ContainScene(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Scene/Clear Scene Overrides", priority = 1000)]
		static void RequestClearSceneTransform() {
			foreach (Object TargetObject in Selection.objects) {
				ClearSceneObject(TargetObject);
			}
		}

		public static bool ClearSceneObject(Object TargetObject) {
			bool IsModified = false;
			if (TargetObject && AssetDatabase.GetAssetPath(TargetObject).EndsWith(".unity")) {
				string ScenePath = AssetDatabase.GetAssetPath(TargetObject);
				Scene TargetScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
				try {
					bool IsDirty = false;
					foreach (GameObject RootGameObject in TargetScene.GetRootGameObjects()) {
						if (ClearPrefabObjectRecursively(RootGameObject)) IsDirty = true;
					}
					if (IsDirty) {
						EditorSceneManager.SaveScene(TargetScene);
						IsModified = true;
					}
				} finally {
					EditorSceneManager.CloseScene(TargetScene, true);
				}
			}
			return IsModified;
		}

		public static bool ClearPrefabObjectRecursively(GameObject TargetGameObject) {
			bool IsModified = false;
			foreach (Transform ChildTransform in TargetGameObject.GetComponentsInChildren<Transform>(true)) {
				if (ChildTransform.gameObject == TargetGameObject) continue;
				if (PrefabUtility.IsAnyPrefabInstanceRoot(ChildTransform.gameObject)) {
					if (ClearPrefabObjectRecursively(ChildTransform.gameObject)) IsModified = true;
				}
			}
			if (ClearPrefabObject(TargetGameObject)) IsModified = true;
			return IsModified;
		}

		static bool ClearPrefabObject(GameObject TargetGameObject) {
			bool IsModified = false;
			if (!PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) return IsModified;
			PropertyModification[] PropertyModifications = PrefabUtility.GetPropertyModifications(TargetGameObject);
			if (PropertyModifications == null || PropertyModifications.Length == 0) return IsModified;
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
					IsModified = true;
					RemovedCount++;
				} else {
					ValidModifications.Add(TargetPropertyModification);
				}
			}
			if (IsModified) {
				PrefabUtility.SetPropertyModifications(TargetGameObject, ValidModifications.ToArray());
				EditorUtility.SetDirty(TargetGameObject);
				AssetDatabase.SaveAssetIfDirty(TargetGameObject);
				Debug.Log($"[VRSuya] Reverted/Removed {RemovedCount} overridden or orphaned properties on {TargetGameObject.name}");
			}
			return IsModified;
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
			if (TargetPropertyPath.StartsWith("animationHashSet") ||
				(TargetPropertyPath.StartsWith("baseAnimationLayers") && TargetPropertyPath.EndsWith("mask")) ||
				TargetPropertyPath.StartsWith("cachedExecutionGroupIndex") ||
				TargetPropertyPath.StartsWith("completedSDKPipeline") ||
				TargetPropertyPath.StartsWith("fallbackStatus") ||
				TargetPropertyPath.StartsWith("foldout_") ||
				TargetPropertyPath.StartsWith("latestValidExecutionGroupIndex") ||
				TargetPropertyPath.StartsWith("m_Bones") ||
				TargetPropertyPath.StartsWith("m_PositionAtRest") ||
				TargetPropertyPath.StartsWith("m_PositionOffset") ||
				TargetPropertyPath.StartsWith("m_RotationAtRest") ||
				TargetPropertyPath.StartsWith("m_RotationOffset") ||
				TargetPropertyPath.StartsWith("m_TranslationAtRest") ||
				TargetPropertyPath.StartsWith("m_TranslationOffsets") ||
				TargetPropertyPath.StartsWith("PositionAtRest") ||
				TargetPropertyPath.StartsWith("PositionOffset") ||
				TargetPropertyPath.StartsWith("RotationAtRest") ||
				TargetPropertyPath.StartsWith("RotationOffset") ||
				TargetPropertyPath.StartsWith("unityVersion.")) {
				return true;
			}
			return false;
		}
	}
}

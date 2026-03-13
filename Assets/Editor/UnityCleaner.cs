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

		[MenuItem("Tools/VRSuya/Cleaner/Standardize fileID", priority = 1100)]
		static void RequestStandardizefileID() {
			Asset AssetInstance = new Asset();
			string[] AnimatorControllerGUIDs = AssetInstance.GetAssetGUIDs(Asset.AssetType.AnimatorController);
			if (AnimatorControllerGUIDs.Length > 0) {
				int ChangedCount = 0;
				try {
					for (int Index = 0; Index < AnimatorControllerGUIDs.Length; Index++) {
						string TargetAssetName = AssetInstance.GUIDToAssetName(AnimatorControllerGUIDs[Index], true);
						EditorUtility.DisplayProgressBar("Standardizing fileID",
							$"Processing : {TargetAssetName}",
							(float)Index / AnimatorControllerGUIDs.Length);
						if (TargetAssetName.EndsWith("Original")) continue;
						if (StandardizefileID(AnimatorControllerGUIDs[Index])) {
							ChangedCount++;
						}
					}
				} finally {
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
				}
				Debug.Log($"[VRSuya] Normalized the fileIDs of {ChangedCount} Animator Controllers");
			}
		}

		static bool StandardizefileID(string TargetGUID) {
			string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUID);
			AnimatorController TargetAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetPath);
			if (TargetAnimatorController && TargetAnimatorController is AnimatorController) {
				string RawAnimatorController = File.ReadAllText(TargetAssetPath);
				if (RawAnimatorController.Contains("m_Controller: {fileID: 0}")) {
					string newRawAnimatorController = RawAnimatorController.Replace("m_Controller: {fileID: 0}", "m_Controller: {fileID: 9100000}");
					File.WriteAllText(TargetAssetPath, newRawAnimatorController);
					return true;
				}
			}
			return false;
		}

		[MenuItem("Tools/VRSuya/Cleaner/Standardize IndirectSpecularColor", priority = 1100)]
		static void StandardizeIndirectSpecularColor() {
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
		}
	}

	public static class PrefabTransformCleaner {

		static readonly float Tolerance = 0.001f;

		[MenuItem("Assets/VRSuya/Clear Prefab Overrides", true)]
		static bool ValidatePrefab() {
			return Selection.objects
				.Select(Item => AssetDatabase.GetAssetPath(Item))
				.Select(Item => Item.EndsWith(".prefab"))
				.Contains(true);
		}

		[MenuItem("Assets/VRSuya/Clear Prefab Overrides", priority = 1100)]
		static void RequestClearPrefabTransform() {
			foreach (Object TargetObject in Selection.objects) {
				GameObject TargetGameObject = TargetObject as GameObject;
				if (TargetGameObject && TargetGameObject.GetType() == typeof(GameObject)) {
					if (PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) ClearPrefabObject(TargetGameObject);
				}
			}
		}

		[MenuItem("Assets/VRSuya/Clear Scene Overrides", true)]
		static bool ValidateScene() {
			return Selection.objects
				.Select(Item => AssetDatabase.GetAssetPath(Item))
				.Select(Item => Item.EndsWith(".unity"))
				.Contains(true);
		}

		[MenuItem("Assets/VRSuya/Clear Scene Overrides", priority = 1100)]
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

		static bool ClearPrefabObject(GameObject TargetGameObject) {
			bool IsChanged = false;
			int Count = 0;
			if (!PrefabUtility.IsPartOfPrefabInstance(TargetGameObject)) return IsChanged;
			PropertyModification[] PropertyModifications = PrefabUtility.GetPropertyModifications(TargetGameObject);
			if (PropertyModifications == null) return IsChanged;
			foreach (PropertyModification TargetPropertyModification in PropertyModifications) {
				string TargetPropertyPath = TargetPropertyModification.propertyPath;
				Object SourcePrefabObject = TargetPropertyModification.target; // Prefab Asset 내의 원본 Object
				bool IsTransformOverride = CheckIsTransformProperty(TargetPropertyPath);
				bool IsCacheOverride = CheckIsAutoGeneratedCacheProperty(TargetPropertyPath);
				if (IsTransformOverride || IsCacheOverride) {
					Object OverriddenInstanceObject = GetInstanceObjectFromPrefabObject(TargetGameObject, SourcePrefabObject);
					if (SourcePrefabObject && OverriddenInstanceObject) {
						bool ShouldRevert = false;
						if (IsCacheOverride) {
							ShouldRevert = true;
						} else if (IsTransformOverride && SourcePrefabObject is Transform SourceTransform) {
							float TargetValue = float.Parse(TargetPropertyModification.value, CultureInfo.InvariantCulture);
							if (CheckNeedRevertTransform(SourceTransform, TargetPropertyPath, TargetValue)) {
								ShouldRevert = true;
							}
						}
						if (ShouldRevert) {
							SerializedObject SerializedInstance = new SerializedObject(OverriddenInstanceObject);
							SerializedProperty TargetProperty = SerializedInstance.FindProperty(TargetPropertyPath);
							if (TargetProperty != null) {
								PrefabUtility.RevertPropertyOverride(TargetProperty, InteractionMode.AutomatedAction);
								IsChanged = true;
								Count++;
							}
						}
					}
				}
			}
			if (IsChanged) {
				EditorUtility.SetDirty(TargetGameObject);
				AssetDatabase.SaveAssetIfDirty(TargetGameObject);
			}
			Debug.Log($"[VRSuya] Reverted {Count} overridden transforms on {TargetGameObject.name}");
			return IsChanged;
		}

		/// <summary>Prefab Asset의 원본 Object와 매핑되는 Scene(또는 Prefab Instance) 내의 Object를 찾습니다.</summary>
		static Object GetInstanceObjectFromPrefabObject(GameObject TargetPrefabInstanceRoot, Object TargetPrefabObject) {
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

		static bool CheckNeedRevertTransform(Transform SourceTransform, string TargetPropertyPath, float TargetValue) {
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
		static bool CheckIsTransformProperty(string TargetPropertyPath) {
			return TargetPropertyPath.StartsWith("m_LocalPosition") ||
				   TargetPropertyPath.StartsWith("m_LocalRotation") ||
				   TargetPropertyPath.StartsWith("m_LocalScale") ||
				   TargetPropertyPath.StartsWith("m_LocalEulerAnglesHint");
		}

		/// <summary>자동 생성되는 내부 캐시 및 애니메이션 관련 속성인지 확인합니다.</summary>
		static bool CheckIsAutoGeneratedCacheProperty(string TargetPropertyPath) {
			if (TargetPropertyPath == "cachedExecutionGroupIndex" ||
				TargetPropertyPath == "latestValidExecutionGroupIndex" ||
				TargetPropertyPath.StartsWith("animationHashSet")) {
				return true;
			}
			return false;
		}
	}
}


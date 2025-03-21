using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class AnimatorControllerCleanerEditor : EditorWindow {

		AnimatorControllerCleaner AnimatorControllerCleanerInstance;
		SerializedObject SerializedAnimatorControllerCleaner;
		SerializedProperty SerializedTargetFolderPath;
		SerializedProperty SerializedTargetAnimatorControllers;
		SerializedProperty SerializedTargetUserRemovefileIDs;

		void OnEnable() {
			AnimatorControllerCleanerInstance = CreateInstance<AnimatorControllerCleaner>();
			SerializedAnimatorControllerCleaner = new SerializedObject(AnimatorControllerCleanerInstance);
			SerializedTargetFolderPath = SerializedAnimatorControllerCleaner.FindProperty("TargetFolderPath");
			SerializedTargetAnimatorControllers = SerializedAnimatorControllerCleaner.FindProperty("TargetAnimatorControllers");
			SerializedTargetUserRemovefileIDs = SerializedAnimatorControllerCleaner.FindProperty("TargetUserRemovefileIDs");
		}

		[MenuItem("Tools/VRSuya/Cleaner/AnimatorController Cleaner", priority = 1000)]
		static void CreateWindow() {
			AnimatorControllerCleanerEditor AppWindow = (AnimatorControllerCleanerEditor)GetWindow(typeof(AnimatorControllerCleanerEditor));
			AppWindow.titleContent = new GUIContent("AnimatorController Cleaner");
			return;
		}

		void OnGUI() {
			if (SerializedAnimatorControllerCleaner == null) {
				Close();
				return;
			}
			SerializedAnimatorControllerCleaner.Update();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			GUILayout.BeginHorizontal();
			GUILayout.Space(15);
			EditorGUILayout.PropertyField(SerializedTargetFolderPath, new GUIContent("검색 경로"));
			if (GUILayout.Button("추가", GUILayout.Width(100))) {
				AnimatorControllerCleanerInstance.AddAnimatorControllers();
			}
			GUILayout.Space(15);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Space(15);
			EditorGUILayout.PropertyField(SerializedTargetAnimatorControllers, new GUIContent("대상 AnimatorController"));
			GUILayout.Space(15);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Space(15);
			EditorGUILayout.PropertyField(SerializedTargetUserRemovefileIDs, new GUIContent("정리할 fileID 리스트"));
			GUILayout.Space(15);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("정리", GUILayout.Width(100))) {
				AnimatorControllerCleanerInstance.RemoveStructureByFileID();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			SerializedAnimatorControllerCleaner.ApplyModifiedProperties();
			return;
		}
	}

	public class AnimatorControllerMaskCleaner : EditorWindow {

		[MenuItem("Tools/VRSuya/Cleaner/Clear FX Layer Mask", priority = 1000)]
		public static void ClearAllFXLayerMask() {
			string[] FXLayerGUIDs = AssetDatabase.FindAssets("FX t:AnimatorController", new[] { "Assets/" });
			if (FXLayerGUIDs.Length > 0) {
				foreach (string TargetFXLayerGUID in FXLayerGUIDs) {
					AnimatorController TargetFXLayer = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(TargetFXLayerGUID));
					if (TargetFXLayer) ClearAnimatorMask(TargetFXLayer);
				}
				AssetDatabase.Refresh();
			}
			return;
		}

		private static void ClearAnimatorMask(AnimatorController TargetAnimator) {
			if (TargetAnimator.layers.Any(Layer => Layer.avatarMask != null)) {
				AnimatorControllerLayer[] NewAnimatorLayers = new AnimatorControllerLayer[TargetAnimator.layers.Length];
				for (int Index = 0; Index < TargetAnimator.layers.Length; Index++) {
					if (TargetAnimator.layers[Index].avatarMask != null) {
						AnimatorControllerLayer NewAnimatorLayer = TargetAnimator.layers[Index];
						NewAnimatorLayer.avatarMask = null;
						NewAnimatorLayers[Index] = NewAnimatorLayer;
					} else {
						NewAnimatorLayers[Index] = TargetAnimator.layers[Index];
					}
				}
				TargetAnimator.layers = NewAnimatorLayers;
				EditorUtility.SetDirty(TargetAnimator);
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {TargetAnimator.name} have been cleared mask successfully");
			}
			return;
		}
	}
}
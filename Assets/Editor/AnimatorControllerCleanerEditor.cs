using UnityEditor;
using UnityEngine;

/*
 * VRSuya AnimatorControllerCleaner Editor
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.animationcleaner {

	[CustomEditor(typeof(AnimatorControllerCleaner))]
	public class AnimatorControllerCleanerEditor : EditorWindow {

		AnimatorControllerCleaner AnimatorControllerCleanerInstance;
		SerializedObject SerializedAnimatorControllerCleaner;
		SerializedProperty SerializedTargetAnimatorController;
		SerializedProperty SerializedTargetRemovefileIDs;

		void OnEnable() {
			AnimatorControllerCleanerInstance = CreateInstance<AnimatorControllerCleaner>();
			SerializedAnimatorControllerCleaner = new SerializedObject(AnimatorControllerCleanerInstance);
			SerializedTargetAnimatorController = SerializedAnimatorControllerCleaner.FindProperty("TargetAnimatorController");
			SerializedTargetRemovefileIDs = SerializedAnimatorControllerCleaner.FindProperty("TargetRemovefileIDs");
		}

		[MenuItem("Tools/VRSuya/AnimatorController Cleaner", priority = 1000)]
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
			GUILayout.FlexibleSpace();
			GUILayout.Label("정리할 AnimatorController", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.PropertyField(SerializedTargetAnimatorController, GUIContent.none);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("제거할 fileID 리스트", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.PropertyField(SerializedTargetRemovefileIDs, GUIContent.none);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("추가", GUILayout.Width(100))) {
				AnimatorControllerCleanerInstance.GetNULLfileID();
			}
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
}
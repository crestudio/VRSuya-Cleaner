using UnityEditor;
using UnityEngine;

/*
 * VRSuya AnimatorControllerCleaner Editor
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.animationcleaner {

	[CustomEditor(typeof(AnimatorControllerCleaner))]
	public class AnimatorControllerCleanerEditor : EditorWindow {

		SerializedObject SerializedObject;
		AnimatorControllerCleaner AnimatorControllerCleanerInstance;

		SerializedProperty SerializedTargetAnimatorController;
		SerializedProperty SerializedTargetfileIDs;

		void OnEnable() {
			if (!AnimatorControllerCleanerInstance) {
				AnimatorControllerCleanerInstance = CreateInstance<AnimatorControllerCleaner>();
			}
			SerializedObject = new SerializedObject(AnimatorControllerCleanerInstance);
			SerializedTargetAnimatorController = SerializedObject.FindProperty("TargetAnimatorController");
			SerializedTargetfileIDs = SerializedObject.FindProperty("TargetfileIDs");
		}

		[MenuItem("Tools/VRSuya/AnimatorController Cleaner", priority = 1000)]
		static void CreateWindow() {
			AnimatorControllerCleanerEditor AppWindow = (AnimatorControllerCleanerEditor)GetWindow(typeof(AnimatorControllerCleanerEditor));
			AppWindow.titleContent = new GUIContent("AnimatorController Cleaner");
			return;
		}

		void OnGUI() {
			SerializedObject.Update();
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
			EditorGUILayout.PropertyField(SerializedTargetfileIDs, GUIContent.none);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			if (GUILayout.Button("정리", GUILayout.Width(100))) {
				AnimatorControllerCleaner.RemoveStructureByFileID();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			SerializedObject.ApplyModifiedProperties();
			return;
		}
	}
}
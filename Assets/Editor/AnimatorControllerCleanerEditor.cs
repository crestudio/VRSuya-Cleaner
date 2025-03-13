using UnityEditor;
using UnityEngine;

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
}
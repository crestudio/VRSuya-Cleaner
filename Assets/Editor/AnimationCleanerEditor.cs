using UnityEngine;
using UnityEditor;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.cleaner {

    [CustomEditor(typeof(AnimationCleaner))]
    public class AnimationCleanerEditor : Editor {

		SerializedProperty SerializedArmAnimationClips;
		SerializedProperty SerializedForearmAnimationClips;
		SerializedProperty SerializedHandAnimationClips;

		void OnEnable() {
			SerializedArmAnimationClips = serializedObject.FindProperty("ArmAnimationClips");
			SerializedForearmAnimationClips = serializedObject.FindProperty("ForearmAnimationClips");
			SerializedHandAnimationClips = serializedObject.FindProperty("HandAnimationClips");
		}

        public override void OnInspectorGUI() {
            serializedObject.Update();
			EditorGUILayout.PropertyField(SerializedArmAnimationClips, new GUIContent("팔 전체"));
			EditorGUILayout.PropertyField(SerializedForearmAnimationClips, new GUIContent("팔뚝"));
			EditorGUILayout.PropertyField(SerializedHandAnimationClips, new GUIContent("손가락"));
			serializedObject.ApplyModifiedProperties();
			if (GUILayout.Button("애니메이션 분석")) {
				(target as AnimationCleaner).CountTargetProperty();
			}
			if (GUILayout.Button("불필요 속성 제거")) {
				(target as AnimationCleaner).UpdateAnimations();
			}
			if (GUILayout.Button("포즈 애니메이션으로 변경")) {
				(target as AnimationCleaner).MakePoseAnimations();
			}
		}
    }
}


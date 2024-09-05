#if UNITY_EDITOR
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/*
 * VRSuya AnimatorControllerCleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 */

namespace com.vrsuya.animationcleaner {

	public class AnimatorControllerCleaner : Editor {

		public static void CleanInvalidAnimatorStates(AnimatorController TargetAnimatorController) {
			if (TargetAnimatorController == null) {
				Debug.LogError("[AnimatorControllerCleaner] 유효하지 않은 AnimatorController 입니다!");
				return;
			}
			int RemovedCount = 0;
			foreach (var TargetLayer in TargetAnimatorController.layers) {
				AnimatorStateMachine TargetStateMachine = TargetLayer.stateMachine;
				List<AnimatorState> InvalidStates = new List<AnimatorState>();
				List<AnimatorStateTransition> InvalidTransitions = new List<AnimatorStateTransition>();
				List<AnimatorStateMachine> InvalidStateMachines = new List<AnimatorStateMachine>();
				foreach (var TargetState in TargetStateMachine.states) {
					if (TargetState.state == null) {
						InvalidStates.Add(TargetState.state);
					}
				}
				foreach (var InvalidState in InvalidStates) {
					TargetStateMachine.RemoveState(InvalidState);
					RemovedCount++;
				}
				foreach (var TargetState in TargetStateMachine.states) {
					foreach (var TargetTransition in TargetState.state.transitions) {
						if (TargetTransition.destinationState == null) {
							InvalidTransitions.Add(TargetTransition);
						}
					}
				}
				foreach (var InvalidTransition in InvalidTransitions) {
					foreach (var TargetState in TargetStateMachine.states) {
						TargetState.state.RemoveTransition(InvalidTransition);
					}
					RemovedCount++;
				}
				foreach (var SubStateMachine in TargetStateMachine.stateMachines) {
					if (SubStateMachine.stateMachine == null) {
						InvalidStateMachines.Add(SubStateMachine.stateMachine);
					}
				}
				foreach (var InvalidStateMachine in InvalidStateMachines) {
					TargetStateMachine.RemoveStateMachine(InvalidStateMachine);
					RemovedCount++;
				}
			}
			if (RemovedCount > 0) {
				Debug.LogWarning("[AnimatorControllerCleaner] AnimatorController에서 총 " + RemovedCount + "건의 데이터가 정리 되었습니다!");
			} else {
				Debug.Log("[AnimatorControllerCleaner] AnimatorController의 모든 구성요소가 유효합니다!");
			}
			return;
		}
	}

	[ExecuteInEditMode]
	public class AnimatorControllerCleanerEditor : EditorWindow {

		public static AnimatorController TargetAnimatorController = null;

		[MenuItem("Tools/VRSuya/AnimatorController Cleaner", priority = 1000)]
		static void CreateWindow() {
			AnimatorControllerCleanerEditor AppWindow = (AnimatorControllerCleanerEditor)GetWindowWithRect(typeof(AnimatorControllerCleanerEditor), new Rect(0, 0, 230, 100));
			AppWindow.titleContent = new GUIContent("AnimatorController Cleaner");
			return;
		}

		void OnGUI() {
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Clean to AnimatorController", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			TargetAnimatorController = (AnimatorController)EditorGUILayout.ObjectField(GUIContent.none, TargetAnimatorController, typeof(AnimatorController), true, GUILayout.Width(200));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			if (GUILayout.Button("Clean", GUILayout.Width(100))) {
				AnimatorControllerCleaner.CleanInvalidAnimatorStates(TargetAnimatorController);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			return;
		}
	}
}
#endif
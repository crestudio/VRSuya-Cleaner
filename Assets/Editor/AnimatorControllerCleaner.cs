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

		public static void RemoveUnreferencedStates(AnimatorController TargetAnimatorController) {
			if (TargetAnimatorController == null) {
				Debug.LogError("[AnimatorControllerCleaner] 유효하지 않은 AnimatorController 입니다!");
				return;
			}
			int RemovedCount = 0;
			List<AnimatorState> AllStates = new List<AnimatorState>();
			HashSet<AnimatorState> ReferencedStates = new HashSet<AnimatorState>();
			foreach (var TargetLayer in TargetAnimatorController.layers) {
				AnimatorStateMachine TargetStateMachine = TargetLayer.stateMachine;
				CollectAllStates(TargetStateMachine, AllStates);
				CollectReferencedStates(TargetStateMachine, ReferencedStates);
			}
			foreach (var TargetState in AllStates) {
				if (!ReferencedStates.Contains(TargetState)) {
					Debug.LogWarning("[AnimatorControllerCleaner] " + TargetState.name + " 이름의 State를 정리 중!");
					RemoveStateFromStateMachine(TargetAnimatorController, TargetState);
					RemovedCount++;
				}
			}
			if (RemovedCount > 0) {
				Debug.LogWarning("[AnimatorControllerCleaner] AnimatorController에서 총 " + RemovedCount + "건의 데이터가 정리 되었습니다!");
			} else {
				Debug.Log("[AnimatorControllerCleaner] AnimatorController의 모든 구성요소가 유효합니다!");
			}
		}

		private static void CollectAllStates(AnimatorStateMachine TargetStateMachine, List<AnimatorState> AllStates) {
			foreach (var TargetState in TargetStateMachine.states) {
				AllStates.Add(TargetState.state);
			}
			foreach (var SubStateMachine in TargetStateMachine.stateMachines) {
				CollectAllStates(SubStateMachine.stateMachine, AllStates);
			}
		}

		private static void CollectReferencedStates(AnimatorStateMachine TargetStateMachine, HashSet<AnimatorState> ReferencedStates) {
			foreach (var TargetState in TargetStateMachine.states) {
				foreach (var TargetTransition in TargetState.state.transitions) {
					if (TargetTransition.destinationState != null) {
						ReferencedStates.Add(TargetTransition.destinationState);
					}
					if (TargetTransition.destinationStateMachine != null) {
						CollectReferencedStates(TargetTransition.destinationStateMachine, ReferencedStates);
					}
				}
			}
			foreach (var SubStateMachine in TargetStateMachine.stateMachines) {
				CollectReferencedStates(SubStateMachine.stateMachine, ReferencedStates);
			}
			return;
		}

		private static void RemoveStateFromStateMachine(AnimatorController TargetAnimatorController, AnimatorState TargetState) {
			foreach (var TargetLayer in TargetAnimatorController.layers) {
				AnimatorStateMachine TargetStateMachine = TargetLayer.stateMachine;
				RemoveStateRecursively(TargetStateMachine, TargetState);
			}
			return;
		}

		private static void RemoveStateRecursively(AnimatorStateMachine TargetStateMachine, AnimatorState TargetState) {
			for (int Index = 0; Index < TargetStateMachine.states.Length; Index++) {
				if (TargetStateMachine.states[Index].state == TargetState) {
					TargetStateMachine.RemoveState(TargetState);
					return;
				}
			}
			foreach (var SubStateMachine in TargetStateMachine.stateMachines) {
				RemoveStateRecursively(SubStateMachine.stateMachine, TargetState);
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
				AnimatorControllerCleaner.RemoveUnreferencedStates(TargetAnimatorController);
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
			return;
		}
	}
}
#endif
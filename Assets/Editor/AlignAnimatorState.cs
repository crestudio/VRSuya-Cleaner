#if UNITY_EDITOR
using System;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using VRSuya.Core;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 * Forked from https://forum.unity.com/threads/get-animator-in-editor-mode.461838/
 */

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class AlignAnimatorState : MonoBehaviour {

		[MenuItem("Tools/VRSuya/Cleaner/Align States in Current AnimatorController")]
		public static void AlignStates() {
			AnimatorController CurrentAnimator = GetCurrentAnimatorController();
			if (CurrentAnimator != null) {
				foreach (var AnimatorLayer in CurrentAnimator.layers) {
					AlignStates(AnimatorLayer.stateMachine);
				}
				EditorUtility.SetDirty(CurrentAnimator);
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
				Debug.Log("[VRSuya AlignState] " + CurrentAnimator.name + " 애니메이터의 정렬이 완료되었습니다!");
			}
			return;
		}

		/// <summary>Animator 윈도우에서 현재 열려있는 AnimatorController 오브젝트를 반환합니다.</summary>
		/// <returns>현재 활성화 되어 있는 AnimatorController</returns>
		private static AnimatorController GetCurrentAnimatorController() {
			AnimatorController CurrentAnimatorController = null;
			DuplicateGameObject DuplicatorInstance = new DuplicateGameObject();
			Type AnimatorWindowType = Type.GetType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs");
			EditorWindow CurrentWindow = DuplicatorInstance.FindFirstWindow(AnimatorWindowType);
			System.Type CurrentWindowType = CurrentWindow.GetType();
			System.Reflection.PropertyInfo CurrentWindowProperty = CurrentWindowType.GetProperty("animatorController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (CurrentWindowProperty != null) {
				CurrentAnimatorController = CurrentWindowProperty.GetValue(CurrentWindow, null) as AnimatorController;
			}
			return CurrentAnimatorController;
		}

		/// <summary>해당 StateMachine 내에 있는 State 위치를 정렬합니다.</summary>
		private static void AlignStates(AnimatorStateMachine TargetStateMachine) {
			TargetStateMachine.entryPosition = new Vector3(40.00f, 100.00f, 0.00f);
			TargetStateMachine.anyStatePosition = new Vector3(40.00f, 200.00f, 0.00f);
			TargetStateMachine.exitPosition = new Vector3(800.00f, 100.00f, 0.00f);
			ChildAnimatorState[] TargetStates = TargetStateMachine.states;
			float Space = 100.00f;
			if (TargetStates.Length > 2) Space = 50.00f;
			for (int Index = 0; Index < TargetStates.Length; Index++) {
				TargetStates[Index].position = new Vector3(400.00f, 100.00f + (Space * Index), 0.00f);
			}
			TargetStateMachine.states = TargetStates;
		}
	}
}
#endif
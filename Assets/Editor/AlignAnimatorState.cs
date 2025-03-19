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
		public static void AlignAllAnimatorState() {
			AnimatorController CurrentAnimator = GetCurrentAnimatorController();
			if (CurrentAnimator) {
				foreach (AnimatorControllerLayer AnimatorLayer in CurrentAnimator.layers) {
					AlignAnimationStates(AnimatorLayer.stateMachine);
				}
				EditorUtility.SetDirty(CurrentAnimator);
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {CurrentAnimator.name} Animator states have been sorted successfully");
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
		/// <param name="TargetStateMachine">정렬을 원하는 StateMachine</param>
		private static void AlignAnimationStates(AnimatorStateMachine TargetStateMachine) {
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
			return;
		}
	}
}
#endif
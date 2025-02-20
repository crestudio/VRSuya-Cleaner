#if UNITY_EDITOR
using System;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using Object = UnityEngine.Object;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 * Forked from https://forum.unity.com/threads/get-animator-in-editor-mode.461838/
 */

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class AlignState : MonoBehaviour {

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

		/// <summary>요청한 타입의 첫번째 윈도우 창 오브젝트를 반환합니다.</summary>
		/// <returns>해당 타입의 첫번째 윈도우</returns>
		private static EditorWindow FindFirstWindow(Type EditorWindowType) {
			if (EditorWindowType == null)
				throw new ArgumentNullException(nameof(EditorWindowType));
			if (!typeof(EditorWindow).IsAssignableFrom(EditorWindowType))
				throw new ArgumentException("The given type (" + EditorWindowType.Name + ") does not inherit from " + nameof(EditorWindow) + ".");
			Object[] TypeOpenWindows = Resources.FindObjectsOfTypeAll(EditorWindowType);
			if (TypeOpenWindows.Length <= 0) return null;
			EditorWindow Window = (EditorWindow)TypeOpenWindows[0];
			return Window;
		}

		/// <summary>Animator 윈도우에서 현재 열려있는 AnimatorController 오브젝트를 반환합니다.</summary>
		/// <returns>현재 활성화 되어 있는 AnimatorController</returns>
		private static AnimatorController GetCurrentAnimatorController() {
			AnimatorController CurrentAnimatorController = null;
			Type AnimatorWindowType = Type.GetType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs");
			EditorWindow CurrentWindow = FindFirstWindow(AnimatorWindowType);
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
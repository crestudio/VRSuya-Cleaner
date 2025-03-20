#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using VRC.SDK3.Avatars.ScriptableObjects;

using VRSuya.Core;
using Avatar = VRSuya.Core.Avatar;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 * Forked from https://forum.unity.com/threads/get-animator-in-editor-mode.461838/
 */

namespace com.vrsuya.cleaner {

	[ExecuteInEditMode]
	public class AlignSort : EditorWindow {

		[MenuItem("Tools/VRSuya/Cleaner/Sort Parameters", priority = 1000)]
		public static void SortAllParameters() {
			string[] ParameterGUIDs = AssetDatabase.FindAssets("Parameter", new[] { "Assets/" });
			if (ParameterGUIDs.Length > 0) {
				foreach (string TargetParameterGUID in ParameterGUIDs) {
					VRCExpressionParameters TargetParameter = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(AssetDatabase.GUIDToAssetPath(TargetParameterGUID));
					if (TargetParameter) {
						SortParameters(TargetParameter);
						EditorUtility.SetDirty(TargetParameter);
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						Debug.Log($"[VRSuya] {TargetParameter.name} have been sorted successfully");
					}
				}
			}
			return;
		}

		private static void SortParameters(VRCExpressionParameters TargetParameter) {
			Avatar AvatarInstance = new Avatar();
			string[] AvatarNames = AvatarInstance.GetAvatarNames();
			VRCExpressionParameters.Parameter[] NewParameterList = new VRCExpressionParameters.Parameter[TargetParameter.parameters.Length];
			List<string> ParameterNameList = TargetParameter.parameters.Select(Parameter => Parameter.name).ToList();
			ParameterNameList = ParameterNameList.OrderBy(Parameter =>
				Parameter.Contains("VRCEmote") ? 0 :
				AvatarNames.Any(AvatarName => Parameter.StartsWith(AvatarName, StringComparison.Ordinal)) ? 1 :
				2
			)
				.ThenBy(Parameter => Parameter, StringComparer.Ordinal)
				.ToList();
			for (int Index = 0; Index < TargetParameter.parameters.Length; Index++) {
				NewParameterList[Index] = TargetParameter.parameters.First(Parameter => Parameter.name == ParameterNameList[Index]);
			}
			TargetParameter.parameters = NewParameterList;
			return;
		}

		[MenuItem("Tools/VRSuya/Cleaner/Sort Animator States", priority = 1000)]
		public static void AlignAllAnimatorState() {
			AnimatorController CurrentAnimator = GetCurrentAnimatorController();
			if (CurrentAnimator) {
				foreach (AnimatorControllerLayer AnimatorLayer in CurrentAnimator.layers) {
					AlignAnimationStates(AnimatorLayer.stateMachine);
				}
				EditorUtility.SetDirty(CurrentAnimator);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
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
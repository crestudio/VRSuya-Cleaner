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

		private static readonly string[] OldAvatarNames = new string[] { "Haku", "Miko" };

		[MenuItem("Tools/VRSuya/Cleaner/Sort VRChat Parameters", priority = 1000)]
		public static void SortAllParameters() {
			string[] ParameterGUIDs = AssetDatabase.FindAssets("Parameter", new[] { "Assets/" });
			if (ParameterGUIDs.Length > 0) {
				foreach (string TargetParameterGUID in ParameterGUIDs) {
					VRCExpressionParameters TargetParameter = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(AssetDatabase.GUIDToAssetPath(TargetParameterGUID));
					if (TargetParameter) SortParameters(TargetParameter);
				}
				AssetDatabase.Refresh();
			}
			return;
		}

		private static void SortParameters(VRCExpressionParameters TargetParameter) {
			Avatar AvatarInstance = new Avatar();
			string[] AvatarNames = AvatarInstance.GetAvatarNames();
			AvatarNames = AvatarNames.Concat(OldAvatarNames).ToArray();
			List<string> OldParameterNameList = TargetParameter.parameters.Select(Parameter => Parameter.name).ToList();
			List<string> NewParameterNameList = OldParameterNameList
				.OrderBy(Parameter =>
					Parameter.Contains("VRCEmote") ? 0 :
					AvatarNames.Any(AvatarName => Parameter.StartsWith(AvatarName, StringComparison.Ordinal)) ? 1 :
					2
				)
				.ThenBy(Parameter => Parameter, StringComparer.Ordinal)
				.ToList();
			if (!OldParameterNameList.SequenceEqual(NewParameterNameList)) {
				VRCExpressionParameters.Parameter[] NewParameters = new VRCExpressionParameters.Parameter[TargetParameter.parameters.Length];
				for (int Index = 0; Index < TargetParameter.parameters.Length; Index++) {
					NewParameters[Index] = TargetParameter.parameters.First(Parameter => Parameter.name == NewParameterNameList[Index]);
				}
				TargetParameter.parameters = NewParameters;
				EditorUtility.SetDirty(TargetParameter);
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {TargetParameter.name} have been sorted successfully");
			}
			return;
		}

		[MenuItem("Tools/VRSuya/Cleaner/Sort VRChat Menus", priority = 1000)]
		public static void SortAllMenus() {
			string[] MenuGUIDs = AssetDatabase.FindAssets("Menu", new[] { "Assets/" });
			if (MenuGUIDs.Length > 0) {
				foreach (string TargetMenuGUID in MenuGUIDs) {
					VRCExpressionsMenu TargetMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(AssetDatabase.GUIDToAssetPath(TargetMenuGUID));
					if (TargetMenu) SortMenus(TargetMenu);
				}
				AssetDatabase.Refresh();
			}
			return;
		}

		private static void SortMenus(VRCExpressionsMenu TargetMenu) {
			List<string> OldMenuNameList = TargetMenu.controls.Select(Menu => Menu.name).ToList();
			List<string> NewMenuNameList = OldMenuNameList
				.OrderBy(Menu => Menu.Contains("VRSuya") ? 3 : Menu.Contains("Emote") ? 2 : Menu.Contains("Modular") ? 1 : 0)
				.ThenBy(Menu => Menu, StringComparer.Ordinal)
				.ToList();
			if (!OldMenuNameList.SequenceEqual(NewMenuNameList)) {
				List<VRCExpressionsMenu.Control> NewMenus = new List<VRCExpressionsMenu.Control>();
				for (int Index = 0; Index < TargetMenu.controls.Count; Index++) {
					NewMenus.Add(TargetMenu.controls.First(Menu => Menu.name == NewMenuNameList[Index]));
				}
				TargetMenu.controls = NewMenus;
				EditorUtility.SetDirty(TargetMenu);
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {TargetMenu.name} have been sorted successfully");
			}
			return;
		}

		[MenuItem("Tools/VRSuya/Cleaner/Sort Animator Layer Parameter", priority = 1000)]
		public static void SortAllAnimator() {
			string[] AnimatorGUIDs = AssetDatabase.FindAssets("FX t:AnimatorController", new[] { "Assets/" });
			if (AnimatorGUIDs.Length > 0) {
				foreach (string TargetAnimatorGUID in AnimatorGUIDs) {
					AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(TargetAnimatorGUID));
					if (TargetAnimator) SortAnimatorLayerParameter(TargetAnimator);
				}
				AssetDatabase.Refresh();
			}
			return;
		}

		private static void SortAnimatorLayerParameter(AnimatorController TargetAnimator) {
			Avatar AvatarInstance = new Avatar();
			string[] AvatarNames = AvatarInstance.GetAvatarNames();
			AvatarNames = AvatarNames.Concat(OldAvatarNames).ToArray();
			bool IsDirty = false;
			List<string> OldLayerNameList = TargetAnimator.layers.Select(Layer => Layer.name).ToList();
			List<string> NewLayerNameList = OldLayerNameList
				.OrderBy(Layer =>
					(Layer.Contains("Base Layer") || Layer.Contains("AllParts")) ? 0 :
					Layer.Contains("Left Hand") ? 1 :
					Layer.Contains("Right Hand") ? 2 :
					AvatarNames.Any(AvatarName => Layer.StartsWith(AvatarName, StringComparison.Ordinal)) ? 3 :
					4
				)
				.ThenBy(Layer => Layer, StringComparer.Ordinal)
				.ToList();
			if (!OldLayerNameList.SequenceEqual(NewLayerNameList)) {
				AnimatorControllerLayer[] NewLayers = new AnimatorControllerLayer[TargetAnimator.layers.Length];
				for (int Index = 0; Index < TargetAnimator.layers.Length; Index++) {
					NewLayers[Index] = TargetAnimator.layers.First(Layer => Layer.name == NewLayerNameList[Index]);
				}
				TargetAnimator.layers = NewLayers;
				IsDirty = true;
			}
			List<string> OldParameterNameList = TargetAnimator.parameters.Select(Parameter => Parameter.name).ToList();
			List<string> NewParameterNameList = OldParameterNameList
				.OrderBy(Parameter =>
					Parameter.Contains("GestureLeft") ? 0 :
					Parameter.Contains("GestureLeftWeight") ? 1 :
					Parameter.Contains("GestureRight") ? 2 :
					Parameter.Contains("GestureRightWeight") ? 3 :
					AvatarNames.Any(AvatarName => Parameter.StartsWith(AvatarName, StringComparison.Ordinal)) ? 4 :
					5
				)
				.ThenBy(Parameter => Parameter, StringComparer.Ordinal)
				.ToList();
			if (!OldParameterNameList.SequenceEqual(NewParameterNameList)) {
				AnimatorControllerParameter[] NewParameters = new AnimatorControllerParameter[TargetAnimator.parameters.Length];
				for (int Index = 0; Index < TargetAnimator.parameters.Length; Index++) {
					NewParameters[Index] = TargetAnimator.parameters.First(Parameter => Parameter.name == NewParameterNameList[Index]);
				}
				TargetAnimator.parameters = NewParameters;
				IsDirty = true;
			}
			if (IsDirty) {
				EditorUtility.SetDirty(TargetAnimator);
				AssetDatabase.SaveAssets();
				Debug.Log($"[VRSuya] {TargetAnimator.name} have been sorted successfully");
			}
			return;
		}

		[MenuItem("Tools/VRSuya/Cleaner/Sort Animator States", priority = 1000)]
		public static void AlignAllAnimatorState() {
			AnimatorController CurrentAnimator = GetCurrentAnimatorController();
			if (CurrentAnimator) {
				foreach (AnimatorControllerLayer AnimatorLayer in CurrentAnimator.layers) {
					AlignAnimationStates(AnimatorLayer.stateMachine, AnimatorLayer.name);
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
		private static void AlignAnimationStates(AnimatorStateMachine TargetStateMachine, string TargetLayerName) {
			TargetStateMachine.entryPosition = new Vector3(40.00f, 100.00f, 0.00f);
			TargetStateMachine.anyStatePosition = new Vector3(40.00f, 200.00f, 0.00f);
			TargetStateMachine.exitPosition = new Vector3(800.00f, 100.00f, 0.00f);
			ChildAnimatorState[] NewAnimationStates = TargetStateMachine.states;
			if (TargetLayerName == "Left Hand" || TargetLayerName == "Right Hand") {
				NewAnimationStates = NewAnimationStates
				.OrderBy(State =>
					State.state.name.Contains("Idle") ? 0 :
					State.state.name.Contains("Fist") ? 1 :
					State.state.name.Contains("Open") ? 2 :
					State.state.name.Contains("Point") ? 3 :
					State.state.name.Contains("Peace") ? 4 :
					State.state.name.Contains("Rock") ? 5 :
					State.state.name.Contains("Gun") ? 6 :
					State.state.name.Contains("Thumbs") ? 7 :
					8
				)
				.ToArray();
			}
			float Space = 100.00f;
			if (NewAnimationStates.Length > 2) Space = 50.00f;
			for (int Index = 0; Index < NewAnimationStates.Length; Index++) {
				NewAnimationStates[Index].position = new Vector3(400.00f, 100.00f + (Space * Index), 0.00f);
			}
			TargetStateMachine.states = NewAnimationStates;
			return;
		}
	}
}
#endif
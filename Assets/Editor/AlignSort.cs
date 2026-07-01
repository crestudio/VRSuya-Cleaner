#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using VRC.SDK3.Avatars.ScriptableObjects;

using VRSuya.Core;

/*
 * VRSuya Cleaner
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 * Forked from https://forum.unity.com/threads/get-animator-in-editor-mode.461838/
 */

namespace VRSuya.Cleaner {

	public class AlignSortVRChat : EditorWindow {

		static readonly string[] OldAvatarNames = new string[] { "Haku", "Miko" };

		[MenuItem("Assets/VRSuya/Asset/Sort VRChat Parameter", true)]
		static bool ValidateVRChatParameter() {
			return AssetUtility.ContainAsset(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Asset/Sort VRChat Parameter", priority = 1000)]
		static void RequestSortVRChatParameters() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				SortVRChatParameters(AssetGUIDs);
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/VRChat/Sort VRChat Parameters", priority = 1000)]
		static void RequestSortAllVRChatParameters() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:VRCExpressionParameters", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				SortVRChatParameters(AssetGUIDs);
			}
		}

		static void SortVRChatParameters(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = AssetUtility.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Sorting VRChat Parameter",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUIDs[Index]);
					VRCExpressionParameters TargetParameter = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(TargetAssetPath);
					if (SortParameter(TargetParameter)) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				if (ModifiedCount > 0) {
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}
			Debug.Log($"[VRSuya] Sorted parameters of {ModifiedCount} VRChat Parameters");
		}

		static bool SortParameter(VRCExpressionParameters TargetParameter) {
			if (TargetParameter && TargetParameter is VRCExpressionParameters) {
				string[] AvatarNames = AvatarUtility.GetAvatarNames();
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
					return true;
				}
			}
			return false;
		}

		[MenuItem("Assets/VRSuya/Asset/Sort VRChat Menu", true)]
		static bool ValidateVRChatMenu() {
			return AssetUtility.ContainAsset(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Asset/Sort VRChat Menu", priority = 1000)]
		static void RequestSortVRChatMenus() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				SortVRChatMenus(AssetGUIDs);
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/VRChat/Sort VRChat Menus", priority = 1000)]
		static void RequestSortAllVRChatMenus() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:VRCExpressionsMenu", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				SortVRChatMenus(AssetGUIDs);
			}
		}

		static void SortVRChatMenus(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = AssetUtility.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Sorting VRChat Menu",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUIDs[Index]);
					VRCExpressionsMenu TargetMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(TargetAssetPath);
					if (SortMenu(TargetMenu)) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				if (ModifiedCount > 0) {
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}
			Debug.Log($"[VRSuya] Sorted menus of {ModifiedCount} VRChat Menus");
		}

		static bool SortMenu(VRCExpressionsMenu TargetMenu) {
			if (TargetMenu && TargetMenu is VRCExpressionsMenu) {
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
					return true;
				}
			}
			return false;
		}
	}

	public class AlignSortAnimator : EditorWindow {

		static readonly string[] OldAvatarNames = new string[] { "Haku", "Miko" };

		[MenuItem("Assets/VRSuya/Animator/Sort Animator", true)]
		static bool ValidateAnimator() {
			return AssetUtility.ContainAnimatorController(Selection.objects);
		}

		[MenuItem("Assets/VRSuya/Animator/Sort Animator", priority = 1000)]
		static void RequestSortAnimator() {
			string[] AssetGUIDs = Selection.objects.Select(Item => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Item))).ToArray();
			if (AssetGUIDs.Length > 0) {
				SortAnimators(AssetGUIDs);
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Sort Animator", priority = 1000)]
		static void RequestAllSortAnimator() {
			string[] AssetGUIDs = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/" });
			if (AssetGUIDs.Length > 0) {
				SortAnimators(AssetGUIDs);
			}
		}

		static void SortAnimators(string[] TargetGUIDs) {
			int ModifiedCount = 0;
			try {
				for (int Index = 0; Index < TargetGUIDs.Length; Index++) {
					string TargetAssetName = AssetUtility.GUIDToAssetName(TargetGUIDs[Index], true);
					EditorUtility.DisplayProgressBar("Sorting AnimatorController",
						$"Processing : {TargetAssetName}",
						(float)Index / TargetGUIDs.Length);
					if (TargetAssetName.EndsWith("Original")) continue;
					string TargetAssetPath = AssetDatabase.GUIDToAssetPath(TargetGUIDs[Index]);
					AnimatorController TargetAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(TargetAssetPath);
					if (SortAnimator(TargetAnimator)) {
						ModifiedCount++;
					}
				}
			} finally {
				EditorUtility.ClearProgressBar();
				if (ModifiedCount > 0) {
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}
			Debug.Log($"[VRSuya] Sorted layers and parameters of {ModifiedCount} Animator Controllers");
		}

		static bool SortAnimator(AnimatorController TargetAnimator) {
			bool IsDirty = false;
			if (TargetAnimator && TargetAnimator is AnimatorController) {
				if (SortLayers(TargetAnimator)) IsDirty = true;
				if (SortParameters(TargetAnimator)) IsDirty = true;
				if (IsDirty) EditorUtility.SetDirty(TargetAnimator);
			}
			return IsDirty;
		}

		static bool SortLayers(AnimatorController TargetAnimator) {
			string[] AvatarNames = AvatarUtility.GetAvatarNames();
			AvatarNames = AvatarNames.Concat(OldAvatarNames).ToArray();
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
				return true;
			}
			return false;
		}

		static bool SortParameters(AnimatorController TargetAnimator) {
			string[] AvatarNames = AvatarUtility.GetAvatarNames();
			AvatarNames = AvatarNames.Concat(OldAvatarNames).ToArray();
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
				return true;
			}
			return false;
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Sort Animator States", priority = 1000)]
		static void AlignAllAnimatorState() {
			AnimatorController CurrentAnimator = GetCurrentAnimatorController();
			if (CurrentAnimator) {
				AnimatorStateMachine[] AllAnimatorStateMachines = CurrentAnimator.layers
					.SelectMany(Item => AnimatorHelper.GetAllStateMachines(Item.stateMachine).ToArray())
					.ToArray();
				foreach (AnimatorStateMachine TargetStateMachine in AllAnimatorStateMachines) {
					string LayerName = CurrentAnimator.layers
					.First(Item => AnimatorHelper.GetAllStateMachines(Item.stateMachine).Contains(TargetStateMachine)).name;
					AlignAnimationStates(TargetStateMachine, LayerName);
				}
				EditorUtility.SetDirty(CurrentAnimator);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				Debug.Log($"[VRSuya] {CurrentAnimator.name} Animator states have been sorted successfully");
			}
		}

		[MenuItem("Tools/VRSuya/Cleaner/Animator/Rename Copied Animator States", priority = 1000)]
		static void RenameAllAnimatorState() {
			AnimatorController CurrentAnimator = GetCurrentAnimatorController();
			if (CurrentAnimator) {
				foreach (AnimatorControllerLayer AnimatorLayer in CurrentAnimator.layers) {
					RenameAnimationStates(AnimatorLayer.stateMachine);
				}
				EditorUtility.SetDirty(CurrentAnimator);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				Debug.Log($"[VRSuya] {CurrentAnimator.name} Animator states have been renamed successfully");
			}
		}

		static AnimatorController GetCurrentAnimatorController() {
			AnimatorController CurrentAnimatorController = null;
			Type AnimatorWindowType = Type.GetType("UnityEditor.Graphs.AnimatorControllerTool, UnityEditor.Graphs");
			EditorWindow CurrentWindow = DuplicateUtility.GetEditorWindow(AnimatorWindowType);
			System.Type CurrentWindowType = CurrentWindow.GetType();
			System.Reflection.PropertyInfo CurrentWindowProperty = CurrentWindowType.GetProperty("animatorController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
			if (CurrentWindowProperty != null) {
				CurrentAnimatorController = CurrentWindowProperty.GetValue(CurrentWindow, null) as AnimatorController;
			}
			return CurrentAnimatorController;
		}

		static void AlignAnimationStates(AnimatorStateMachine TargetStateMachine, string TargetLayerName) {
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
		}

		static void RenameAnimationStates(AnimatorStateMachine TargetStateMachine) {
			ChildAnimatorState[] NewAnimationStates = TargetStateMachine.states;
			for (int Index = 0; Index < NewAnimationStates.Length; Index++) {
				if (NewAnimationStates[Index].state.name.Contains(" 1")) {
					NewAnimationStates[Index].state.name = NewAnimationStates[Index].state.name.Replace(" 1", "");
				}
			}
			TargetStateMachine.states = NewAnimationStates;
		}
	}
}
#endif